using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class TransformProperties
    {
        static Dictionary<APropertyDecl, AEnrichmentDecl> OldEnrichmentParents = new Dictionary<APropertyDecl, AEnrichmentDecl>();
        static Dictionary<APropertyDecl, AStructDecl> OldStructParents = new Dictionary<APropertyDecl, AStructDecl>();

        /*
         * Convert
         * 
         * static int Prop
         * {
         *   get
         *   {
         *      return foo;
         *   }
         *   set
         *   {
         *      foo = value;
         *   }
         * }
         * 
         * ...
         * Prop = 2;
         * return Prop;
         * ...
         * 
         * Into
         * 
         * static int GetProp()
         * {
         *  return foo;
         * }
         * 
         * static void SetProp(int value)
         * {
         *  foo = value;
         * }
         * 
         * ...
         * SetProp(2);
         * return GetProp();
         *  
         * 
         */ 
        public class Phase1
        {
            /*
             * Apply before Transform method decls, and remove dead code
             * 
             * Convert properties to methods
             */ 

            

            public static void Parse(FinalTransformations finalTrans)
            {
                OldEnrichmentParents.Clear();
                OldStructParents.Clear();
                SharedData data = finalTrans.data;

                foreach (APropertyDecl property in data.Properties.Select(p => p.Decl))
                {
                    AASourceFile parent = (AASourceFile) property.Parent();
                    if (property.GetGetter() != null)
                    {
                        AMethodDecl getter = new AMethodDecl(new APublicVisibilityModifier(), null,
                                                             property.GetStatic() == null
                                                                 ? null
                                                                 : (TStatic)property.GetStatic().Clone(), null, null, null,
                                                             Util.MakeClone(property.GetType(), data),
                                                             new TIdentifier("Get" + property.GetName().Text, property.GetName().Line, 0),
                                                             new ArrayList(), property.GetGetter());
                        data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(parent, getter));
                        parent.GetDecl().Insert(parent.GetDecl().IndexOf(property), getter);
                        data.Getters[property] = getter;
                    }
                    if (property.GetSetter() != null)
                    {
                        AALocalDecl valueLocal = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                                 Util.MakeClone(property.GetType(), data),
                                                                 new TIdentifier("value"), null);
                        AMethodDecl setter = new AMethodDecl(new APublicVisibilityModifier(), null,
                                                             property.GetStatic() == null
                                                                 ? null
                                                                 : (TStatic)property.GetStatic().Clone(), null, null, null,
                                                             new AVoidType(new TVoid("void")),
                                                             new TIdentifier("Set" + property.GetName().Text, property.GetName().Line, 0),
                                                             new ArrayList(){valueLocal}, property.GetSetter());
                        data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(parent, setter));
                        parent.GetDecl().Insert(parent.GetDecl().IndexOf(property), setter);
                        setter.GetBlock().Apply(new FixValue(valueLocal, data));
                        data.Setters[property] = setter;
                    }
                    property.Parent().RemoveChild(property);
                }

                foreach (AStructDecl structDecl in data.Structs.Select(s => s.Decl))
                {
                    foreach (APropertyDecl property in data.StructProperties[structDecl])
                    {
                        //Due to enheritance, they might not be in same struct
                        if (structDecl != Util.GetAncestor<AStructDecl>(property))
                            continue;
                        OldStructParents[property] = structDecl;

                        if (property.GetGetter() != null)
                        {
                            AALocalDecl indexLocal = null;
                            string methodName = "Get" + property.GetName().Text;
                            if (property.GetName().Text == "")
                            {
                                methodName = "GetThis";
                                indexLocal = data.ArrayPropertyLocals[property][0];
                            }

                            AMethodDecl getter = new AMethodDecl(new APublicVisibilityModifier(), null,
                                                                 property.GetStatic() == null
                                                                     ? null
                                                                     : (TStatic)property.GetStatic().Clone(), null, null, null,
                                                                 Util.MakeClone(property.GetType(), data),
                                                                 new TIdentifier(methodName, property.GetName().Line, 0),
                                                                 new ArrayList(), property.GetGetter());

                            if (indexLocal != null)
                            {
                                indexLocal.Parent().Parent().RemoveChild(indexLocal.Parent());
                                //data.Locals[(AABlock) getter.GetBlock()].Remove(indexLocal);
                                getter.GetFormals().Insert(0, indexLocal);
                            }

                            data.StructMethods[structDecl].Add(getter);
                            structDecl.GetLocals().Insert(structDecl.GetLocals().IndexOf(property.Parent()), new ADeclLocalDecl(getter));
                            data.Getters[property] = getter;
                        }
                        if (property.GetSetter() != null)
                        {
                            AALocalDecl indexLocal = null;
                            string methodName = "Set" + property.GetName().Text;
                            if (property.GetName().Text == "")
                            {
                                methodName = "SetThis";
                                indexLocal = data.ArrayPropertyLocals[property][data.ArrayPropertyLocals[property].Length - 1];
                            }

                            AALocalDecl valueLocal = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                                     Util.MakeClone(property.GetType(), data),
                                                                     new TIdentifier("value"), null);
                            AMethodDecl setter = new AMethodDecl(new APublicVisibilityModifier(), null,
                                                                 property.GetStatic() == null
                                                                     ? null
                                                                     : (TStatic)property.GetStatic().Clone(), null, null, null,
                                                                 new AVoidType(new TVoid("void")),
                                                                 new TIdentifier(methodName, property.GetName().Line, 0),
                                                                 new ArrayList() { valueLocal }, property.GetSetter());

                            if (indexLocal != null)
                            {
                                indexLocal.Parent().Parent().RemoveChild(indexLocal.Parent());
                                //data.Locals[(AABlock)setter.GetBlock()].Remove(indexLocal);
                                setter.GetFormals().Insert(0, indexLocal);
                            }

                            data.StructMethods[structDecl].Add(setter);
                            structDecl.GetLocals().Insert(structDecl.GetLocals().IndexOf(property.Parent()), new ADeclLocalDecl(setter));
                            setter.GetBlock().Apply(new FixValue(valueLocal, data));
                            data.Setters[property] = setter; 
                        }
                        structDecl.RemoveChild(property.Parent());
                    }
                }

                foreach (AEnrichmentDecl enrichment in data.Enrichments)
                {
                    for (int i = 0; i < enrichment.GetDecl().Count; i++)
                    {
                        if (!(enrichment.GetDecl()[i] is APropertyDecl))
                            continue;


                        APropertyDecl property = (APropertyDecl) enrichment.GetDecl()[i];

                        OldEnrichmentParents[property] = enrichment;
                        if (property.GetGetter() != null)
                        {
                            AALocalDecl indexLocal = null;
                            string methodName = "Get" + property.GetName().Text;
                            if (property.GetName().Text == "")
                            {
                                methodName = "GetThis";
                                indexLocal = data.ArrayPropertyLocals[property][0];
                            }

                            AMethodDecl getter = new AMethodDecl(new APublicVisibilityModifier(), null,
                                                                 property.GetStatic() == null
                                                                     ? null
                                                                     : (TStatic)property.GetStatic().Clone(), null, null, null,
                                                                 Util.MakeClone(property.GetType(), data),
                                                                 new TIdentifier(methodName, property.GetName().Line, 0),
                                                                 new ArrayList(), property.GetGetter());
                            if (indexLocal != null)
                            {
                                indexLocal.Parent().Parent().RemoveChild(indexLocal.Parent());
                                //data.Locals[(AABlock)getter.GetBlock()].Remove(indexLocal);
                                getter.GetFormals().Insert(0, indexLocal);
                            }

                            enrichment.GetDecl().Insert(enrichment.GetDecl().IndexOf(property), getter);
                            data.Getters[property] = getter;
                        }
                        if (property.GetSetter() != null)
                        {
                            AALocalDecl indexLocal = null;
                            string methodName = "Set" + property.GetName().Text;
                            if (property.GetName().Text == "")
                            {
                                methodName = "SetThis";
                                indexLocal = data.ArrayPropertyLocals[property][data.ArrayPropertyLocals[property].Length - 1];
                            }

                            AALocalDecl valueLocal = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                                     Util.MakeClone(property.GetType(), data),
                                                                     new TIdentifier("value"), null);
                            AMethodDecl setter = new AMethodDecl(new APublicVisibilityModifier(), null,
                                                                 property.GetStatic() == null
                                                                     ? null
                                                                     : (TStatic)property.GetStatic().Clone(), null, null, null,
                                                                 new AVoidType(new TVoid("void")),
                                                                 new TIdentifier(methodName, property.GetName().Line, 0),
                                                                 new ArrayList() { valueLocal }, property.GetSetter());
                            if (indexLocal != null)
                            {
                                indexLocal.Parent().Parent().RemoveChild(indexLocal.Parent());
                                data.Locals[(AABlock)setter.GetBlock()].Remove(indexLocal);
                                setter.GetFormals().Insert(0, indexLocal);
                            }

                            enrichment.GetDecl().Insert(enrichment.GetDecl().IndexOf(property), setter);
                            setter.GetBlock().Apply(new FixValue(valueLocal, data));
                            data.Setters[property] = setter; 
                        }
                        enrichment.RemoveChild(property);
                    }
                }
            }

            private class FixValue : DepthFirstAdapter
            {
                private AALocalDecl valueDecl;
                private SharedData data;

                public FixValue(AALocalDecl valueDecl, SharedData data)
                {
                    this.valueDecl = valueDecl;
                    this.data = data;
                }

                public override void OutAValueLvalue(AValueLvalue node)
                {
                    ALocalLvalue replacer = new ALocalLvalue(new TIdentifier("value"));
                    node.ReplaceBy(replacer);
                    data.LocalLinks[replacer] = valueDecl;
                    data.LvalueTypes[replacer] = valueDecl.GetType();
                }
            }
        }

        public class Phase2 : DepthFirstAdapter
        {
            /*
             * Apply after assignement fixup
             * Assume no i++
             * 
             * Convert usages to method invocations.
             */

            public static void Parse(FinalTransformations finalTrans)
            {
                SharedData data = finalTrans.data;

                foreach (KeyValuePair<APropertyLvalue, APropertyDecl> pair in data.PropertyLinks)
                {
                    APropertyLvalue lvalue = pair.Key;
                    APropertyDecl property = pair.Value;

                    if (Util.GetAncestor<AAProgram>(lvalue) == null)
                        continue;

                    if (lvalue.Parent() is AAssignmentExp)
                    {
                        AAssignmentExp assignment = (AAssignmentExp) lvalue.Parent();
                        ASimpleInvokeExp invoke =
                            new ASimpleInvokeExp(
                                new TIdentifier("Set" + property.GetName().Text, lvalue.GetName().Line,
                                                lvalue.GetName().Pos), new ArrayList(){assignment.GetExp()});
                        assignment.ReplaceBy(invoke);
                        data.SimpleMethodLinks[invoke] = data.Setters[property];
                        data.ExpTypes[invoke] = new AVoidType(new TVoid("void"));
                    }
                    else
                    {
                        ALvalueExp exp = (ALvalueExp) lvalue.Parent();
                        ASimpleInvokeExp invoke =
                            new ASimpleInvokeExp(
                                new TIdentifier("Get" + property.GetName().Text, lvalue.GetName().Line,
                                                lvalue.GetName().Pos), new ArrayList() {});
                        exp.ReplaceBy(invoke);
                        data.SimpleMethodLinks[invoke] = data.Getters[property];
                        data.ExpTypes[invoke] = property.GetType();
                    }
                }
                foreach (KeyValuePair<AStructLvalue, APropertyDecl> pair in data.StructPropertyLinks)
                {
                    AStructLvalue lvalue = pair.Key;
                    APropertyDecl property = pair.Value;
                    AEnrichmentDecl enrichmentDecl = null;
                    AStructDecl structDecl = null;
                    if (data.EnrichmentTypeLinks.ContainsKey(data.ExpTypes[lvalue.GetReceiver()]))
                        enrichmentDecl = data.EnrichmentTypeLinks[data.ExpTypes[lvalue.GetReceiver()]];
                    if (enrichmentDecl == null)
                        structDecl = data.StructTypeLinks[(ANamedType)data.ExpTypes[lvalue.GetReceiver()]];

                    if (Util.GetAncestor<AAProgram>(lvalue) == null)
                        continue;

                    PExp structArg;
                    if (structDecl == null || structDecl.GetClassToken() == null)
                    {
                        structArg = lvalue.GetReceiver();
                    }
                    else
                    {
                        //Send pointer
                        ALvalueExp lvalueExp = (ALvalueExp) lvalue.GetReceiver();
                        APointerLvalue pointerValue = (APointerLvalue) lvalueExp.GetLvalue();
                        structArg = pointerValue.GetBase();
                    }

                    if (lvalue.Parent() is AAssignmentExp)
                    {
                        AAssignmentExp assignment = (AAssignmentExp)lvalue.Parent();
                        ASimpleInvokeExp invoke =
                            new ASimpleInvokeExp(
                                new TIdentifier("Set" + property.GetName().Text, lvalue.GetName().Line,
                                                lvalue.GetName().Pos), new ArrayList() { assignment.GetExp(), structArg });
                        assignment.ReplaceBy(invoke);
                        data.SimpleMethodLinks[invoke] = data.Setters[property];
                        data.ExpTypes[invoke] = new AVoidType(new TVoid("void"));
                    }
                    else
                    {
                        ALvalueExp exp = (ALvalueExp)lvalue.Parent();
                        ASimpleInvokeExp invoke =
                            new ASimpleInvokeExp(
                                new TIdentifier("Get" + property.GetName().Text, lvalue.GetName().Line,
                                                lvalue.GetName().Pos), new ArrayList() { structArg });
                        exp.ReplaceBy(invoke);
                        data.SimpleMethodLinks[invoke] = data.Getters[property];
                        data.ExpTypes[invoke] = property.GetType();
                    }
                }
                foreach (KeyValuePair<AArrayLvalue, Util.Pair<APropertyDecl, bool>> pair in data.ArrayPropertyLinks)
                {
                    AArrayLvalue lvalue = pair.Key;
                    APropertyDecl property = pair.Value.First;
                    bool implicitMatch = pair.Value.Second;
                    AEnrichmentDecl enrichmentDecl = null;
                    AStructDecl structDecl = null;

                    if (OldEnrichmentParents.ContainsKey(property))
                        enrichmentDecl = OldEnrichmentParents[property];
                    else
                        structDecl = OldStructParents[property];
                    if (Util.GetAncestor<AAProgram>(lvalue) == null)
                        continue;

                    PExp structArg;
                    if (structDecl == null || structDecl.GetClassToken() == null)
                    {
                        structArg = lvalue.GetBase();
                    }
                    else
                    {
                        //Send pointer
                        if (implicitMatch)
                            structArg = lvalue.GetBase();
                        else
                        {
                            ALvalueExp lvalueExp = (ALvalueExp) lvalue.GetBase();
                            APointerLvalue pointerValue = (APointerLvalue) lvalueExp.GetLvalue();
                            structArg = pointerValue.GetBase();
                        }
                    }

                  /*  if (!(structArg is ALvalueExp && 
                        (((ALvalueExp)structArg).GetLvalue() is ALocalLvalue || ((ALvalueExp)structArg).GetLvalue() is AFieldLvalue ||
                        ((ALvalueExp)structArg).GetLvalue() is AStructLvalue || ((ALvalueExp)structArg).GetLvalue() is AStructFieldLvalue))
                    {
                        //Make new local
                        AALocalDecl decl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                           Util.MakeClone(data.ExpTypes[structArg], data),
                                                           new TIdentifier("propertyVar"), structArg);
                        ALocalLvalue declRef = new ALocalLvalue(new TIdentifier("propertyVar"));
                        structArg = new ALvalueExp(declRef);
                        PStm stm = Util.GetAncestor<PStm>(lvalue);
                    }*/

                    if (lvalue.Parent() is AAssignmentExp)
                    {
                        AAssignmentExp assignment = (AAssignmentExp)lvalue.Parent();
                        ASimpleInvokeExp invoke =
                            new ASimpleInvokeExp(
                                new TIdentifier("SetThis", lvalue.GetToken().Line,
                                                lvalue.GetToken().Pos), new ArrayList() { lvalue.GetIndex() ,assignment.GetExp(), structArg });
                        assignment.ReplaceBy(invoke);
                        data.SimpleMethodLinks[invoke] = data.Setters[property];
                        data.ExpTypes[invoke] = new AVoidType(new TVoid("void"));
                    }
                    else
                    {
                        ALvalueExp exp = (ALvalueExp)lvalue.Parent();
                        ASimpleInvokeExp invoke =
                            new ASimpleInvokeExp(
                                new TIdentifier("GetThis", lvalue.GetToken().Line,
                                                lvalue.GetToken().Pos), new ArrayList() {  lvalue.GetIndex(), structArg });
                        exp.ReplaceBy(invoke);
                        data.SimpleMethodLinks[invoke] = data.Getters[property];
                        data.ExpTypes[invoke] = property.GetType();
                    }
                }

            }
        }

    }
}

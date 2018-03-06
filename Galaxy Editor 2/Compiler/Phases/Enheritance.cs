using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Generated.parser;
using Galaxy_Editor_2.Compiler.Phases.Transformations;
using Galaxy_Editor_2.Compiler.Phases.Transformations.Util_classes;

namespace Galaxy_Editor_2.Compiler.Phases
{
    //After Envirioment checking, before type linking
    class Enheritance : DepthFirstAdapter
    {
        private ErrorCollection errors;
        private SharedData data;
        private List<AStructDecl> checkedStructs = new List<AStructDecl>();
        private Dictionary<AALocalDecl, AStructDecl> fieldOriginatesFrom = new Dictionary<AALocalDecl, AStructDecl>();
        private Dictionary<AMethodDecl, AStructDecl> methodOriginatesFrom = new Dictionary<AMethodDecl, AStructDecl>();
        private Dictionary<APropertyDecl, AStructDecl> propertyOriginatesFrom = new Dictionary<APropertyDecl, AStructDecl>();
        private Dictionary<AThisArrayPropertyDecl, AStructDecl> arrayPropertyOriginatesFrom = new Dictionary<AThisArrayPropertyDecl, AStructDecl>();

        public Enheritance(SharedData data, ErrorCollection errors)
        {
            this.data = data;
            this.errors = errors;
        }

        public override void CaseAStructDecl(AStructDecl node)
        {
            if (checkedStructs.Contains(node))
                return;
            checkedStructs.Add(node);

            //Set where they originate from.
            foreach (PLocalDecl localDecl in node.GetLocals())
            {
                if (localDecl is AALocalDecl)
                {
                    fieldOriginatesFrom[(AALocalDecl)localDecl] = node;
                }
                else //Is DeclLocalDecl
                {
                    ADeclLocalDecl aLocalDecl = (ADeclLocalDecl)localDecl;
                    PDecl decl = aLocalDecl.GetDecl();
                    if (decl is AMethodDecl)
                    {
                        methodOriginatesFrom[(AMethodDecl)decl] = node;
                    }
                    else if (decl is APropertyDecl)
                    {
                        propertyOriginatesFrom[(APropertyDecl)decl] = node;
                    }
                    else if (decl is AThisArrayPropertyDecl)
                    {
                        arrayPropertyOriginatesFrom[(AThisArrayPropertyDecl) decl] = node;
                    }
                }
            }


            if (node.GetBase() == null)
                return;

            AStructDecl baseStr = Lookup((ANamedType) node.GetBase());
            if (!checkedStructs.Contains(baseStr))
                CaseAStructDecl(baseStr);
            CheckEnheritanceList(node, new List<AStructDecl>());
            //A struct may not enhrit from a class 
            if (node.GetClassToken() == null && baseStr.GetClassToken() != null)
            {
                errors.Add(new ErrorCollection.Error(node.GetName(), "A struct can not enherit from a class.", false,
                                                     new ErrorCollection.Error(baseStr.GetName(), "Enherited class")));
            }
            
            //Copy everything in base struct to here (Except from constructors)
            List<PLocalDecl> stuffToAdd = new List<PLocalDecl>();
            foreach (AALocalDecl baseLocalVar in data.StructFields[baseStr])
            {
                //Check that it is not overwritten
                foreach (AALocalDecl localVar in node.GetLocals().OfType<AALocalDecl>())
                {
                    if (baseLocalVar.GetName().Text == localVar.GetName().Text)
                    {
                        errors.Add(new ErrorCollection.Error(localVar.GetName(),
                                                             "It is not possible to override fields.", false,
                                                             new ErrorCollection.Error(
                                                                 fieldOriginatesFrom[baseLocalVar].GetName(),
                                                                 "Overridden " +
                                                                 Util.GetTypeName(fieldOriginatesFrom[baseLocalVar]))));
                        throw new ParserException(null, null);
                    }
                }
                foreach (APropertyDecl localProperty in data.StructProperties[node])
                {
                    if (localProperty.GetName().Text == baseLocalVar.GetName().Text)
                    {
                        errors.Add(new ErrorCollection.Error(localProperty.GetName(),
                                                             "It is not possible to override fields.", false,
                                                             new ErrorCollection.Error(
                                                                 fieldOriginatesFrom[baseLocalVar].GetName(),
                                                                 "Overridden " +
                                                                 Util.GetTypeName(fieldOriginatesFrom[baseLocalVar]))));
                        throw new ParserException(null, null);
                    }
                }
                //Insert at top
                AALocalDecl newLocalVar = (AALocalDecl)baseLocalVar.Clone();
                baseLocalVar.Apply(new FixNamedRefferences(newLocalVar, data));
                stuffToAdd.Add(newLocalVar);
                data.StructFields[node].Add(newLocalVar);
                fieldOriginatesFrom[newLocalVar] = fieldOriginatesFrom[baseLocalVar];
                if (data.EnheritanceLocalMap.ContainsKey(baseLocalVar))
                    data.EnheritanceLocalMap[newLocalVar] = data.EnheritanceLocalMap[baseLocalVar];
                else
                    data.EnheritanceLocalMap[newLocalVar] = baseLocalVar;
            }
            for (int i = stuffToAdd.Count - 1; i >= 0; i--)
            {
                node.GetLocals().Insert(0, stuffToAdd[i]);
            }
            //Methods
            foreach (AMethodDecl baseMethod in data.StructMethods[baseStr])
            {
                //Check that it is not overwritten
                foreach (AMethodDecl localMethod in node.GetLocals().OfType<ADeclLocalDecl>().Select(l => l.GetDecl()).OfType<AMethodDecl>())
                {
                    if (Util.GetMethodSignature(baseMethod) == Util.GetMethodSignature(localMethod))
                    {
                        errors.Add(new ErrorCollection.Error(localMethod.GetName(),
                                                             "It is not possible to override methods.", false,
                                                             new ErrorCollection.Error(
                                                                 methodOriginatesFrom[baseMethod].GetName(),
                                                                 "Overridden " +
                                                                 Util.GetTypeName(methodOriginatesFrom[baseMethod]))));
                        throw new ParserException(null, null);
                    }
                }
                data.StructMethods[node].Add(baseMethod);
            }
            //Properties
            foreach (APropertyDecl baseProperty in data.StructProperties[baseStr])
            {
                //Check that it is not overwritten
                foreach (APropertyDecl localProperty in data.StructProperties[node])
                {
                    if (localProperty.GetName().Text == baseProperty.GetName().Text)
                    {
                        errors.Add(new ErrorCollection.Error(localProperty.GetName(),
                                                             "It is not possible to override properties.", false,
                                                             new ErrorCollection.Error(
                                                                 propertyOriginatesFrom[baseProperty].GetName(),
                                                                 "Overridden " +
                                                                 Util.GetTypeName(propertyOriginatesFrom[baseProperty]))));
                        throw new ParserException(null, null);
                    }
                }
                foreach (AALocalDecl localVar in node.GetLocals().OfType<AALocalDecl>())
                {
                    if (baseProperty.GetName().Text == localVar.GetName().Text)
                    {
                        errors.Add(new ErrorCollection.Error(localVar.GetName(),
                                                             "It is not possible to override properties.", false,
                                                             new ErrorCollection.Error(
                                                                 propertyOriginatesFrom[baseProperty].GetName(),
                                                                 "Overridden " +
                                                                 Util.GetTypeName(propertyOriginatesFrom[baseProperty]))));
                        throw new ParserException(null, null);
                    }
                }
                data.StructProperties[node].Add(baseProperty);
            }
        }

        private class FixNamedRefferences : DepthFirstAdapter
        {
            private SharedData data;
            private bool isFirst = true;
            private Node currentClone;

            public FixNamedRefferences(Node currentClone, SharedData data)
            {
                this.currentClone = currentClone;
                this.data = data;
            }

            public override void DefaultIn(Node node)
            {
                if (isFirst)
                    isFirst = false;
                else
                {
                    int index = 0;
                    CloneMethod.GetChildTypeIndex getChildTypeIndex = new CloneMethod.GetChildTypeIndex()
                                                                          {Parent = node.Parent(), Child = node};
                    node.Parent().Apply(getChildTypeIndex);
                    index = getChildTypeIndex.Index;
                    CloneMethod.GetChildTypeByIndex getChildTypeByIndex = new CloneMethod.GetChildTypeByIndex()
                                                                              {
                                                                                  Child = node,
                                                                                  Index = index,
                                                                                  Parent = currentClone
                                                                              };
                    currentClone.Apply(getChildTypeByIndex);
                    currentClone = getChildTypeByIndex.Child;
                }

                if (node is ANamedType && data.StructTypeLinks.ContainsKey((ANamedType) node))
                    data.StructTypeLinks.Add((ANamedType) currentClone, data.StructTypeLinks[(ANamedType) node]);
            }
            public override void DefaultOut(Node node)
            {
                currentClone = currentClone.Parent();
            }
        }


        //Check that there are no circular enhritance
        private void CheckEnheritanceList(AStructDecl str, List<AStructDecl> list)
        {
            if (list.Contains(str))
            {
                List<ErrorCollection.Error> subErrors = new List<ErrorCollection.Error>();
                for (int i = list.IndexOf(str); i < list.Count; i++)
                {
                    subErrors.Add(new ErrorCollection.Error(list[i].GetName(), (list[i].GetClassToken() == null ? "Struct" : "class") + " in cycle."));
                }
                errors.Add(new ErrorCollection.Error(str.GetName(), "You can not make a cycle of enheritance.", false, subErrors.ToArray()));
                throw new ParserException(str.GetName(), "");
            }
            list.Add(str);
            if (str.GetBase() != null)
                CheckEnheritanceList(Lookup((ANamedType)str.GetBase()), list);
        }

        private AStructDecl Lookup(ANamedType type)
        {
            //Look for structs
            List<AStructDecl> structs = new List<AStructDecl>();
            bool matchingPrimitive;
            LinkNamedTypes.GetMatchingTypes(type, new List<ATypedefDecl>(), structs, new List<AMethodDecl>(), new List<TIdentifier>(), out matchingPrimitive);

            if (structs.Count == 0)
            {
                errors.Add(new ErrorCollection.Error(type.GetToken(), "No type found named " + ((AAName)type.GetName()).AsString()), true);
                throw new ParserException(null, null);
            }
            else if (structs.Count > 1)
            {
                List<ErrorCollection.Error> subError = new List<ErrorCollection.Error>();
                foreach (AStructDecl structDecl in structs)
                {
                    subError.Add(new ErrorCollection.Error(structDecl.GetName(), "Matching " + Util.GetTypeName(structDecl)));
                }
                errors.Add(
                    new ErrorCollection.Error(type.GetToken(),
                                              "Found multiple structs/classes matching " + ((AAName)type.GetName()).AsString(),
                                              false, subError.ToArray()), true);
            }

            return structs[0];
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Generated.parser;
using Galaxy_Editor_2.Compiler.Phases.Transformations.Util_classes;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    /*
     * Assume after assign fixup
     * Best before bulkcopy
     */ 
    class Pointers
    {
        private SharedData data;

        public Pointers(SharedData data)
        {
            this.data = data;
        }

        public void Parse(AAProgram ast)
        {
            FindNullChecks finder = new FindNullChecks(data);
            ast.Apply(finder);
            ast.Apply(new Phase0(data));
            //ast.Apply(new Phase0(data));
            ast.Apply(new Phase2(data, finder.TypesWithIdentifierArray));
        }

        private class FindNullChecks : DepthFirstAdapter
        {
            public List<PType> TypesWithIdentifierArray = new List<PType>();
            private SharedData data;

            public FindNullChecks(SharedData data)
            {
                this.data = data;
            }

            public override void CaseAAssignmentExp(AAssignmentExp node)
            {
                if (node.GetExp() is ANullExp)
                {
                    PType type = data.LvalueTypes[node.GetLvalue()];
                    if (type is APointerType)
                    {
                        bool add = true;
                        foreach (PType pType in TypesWithIdentifierArray)
                        {
                            if (Util.TypesEqual(((APointerType)type).GetType(), pType, data))
                            {
                                add = false;
                                break;
                            }
                        }
                        if (add)
                            TypesWithIdentifierArray.Add(((APointerType) type).GetType());
                    }
                }


                base.CaseAAssignmentExp(node);
            }

            public override void CaseAValueReturnStm(AValueReturnStm node)
            {
                AMethodDecl pMethod = Util.GetAncestor<AMethodDecl>(node);


                if (pMethod != null)
                {
                    PType type = pMethod.GetReturnType();
                    if (node.GetExp() is ANullExp)
                    {
                        if (type is APointerType)
                        {
                            bool add = true;
                            foreach (PType pType in TypesWithIdentifierArray)
                            {
                                if (Util.TypesEqual(((APointerType)type).GetType(), pType, data))
                                {
                                    add = false;
                                    break;
                                }
                            }
                            if (add)
                                TypesWithIdentifierArray.Add(((APointerType)type).GetType());
                        }
                    }

                }
                base.CaseAValueReturnStm(node);
            }

            public override void CaseAFieldDecl(AFieldDecl node)
            {
                PType type = node.GetType();
                if (node.GetInit() is ANullExp)
                {
                    if (type is APointerType)
                    {
                        bool add = true;
                        foreach (PType pType in TypesWithIdentifierArray)
                        {
                            if (Util.TypesEqual(((APointerType)type).GetType(), pType, data))
                            {
                                add = false;
                                break;
                            }
                        }
                        if (add)
                            TypesWithIdentifierArray.Add(((APointerType)type).GetType());
                    }
                }

                base.CaseAFieldDecl(node);
            }

            public override void OutABinopExp(ABinopExp node)
            {
                if (node.GetBinop() is AEqBinop || node.GetBinop() is ANeBinop)
                {
                    if (node.GetLeft() is ANullExp || node.GetRight() is ANullExp)
                    {
                        if (node.GetLeft() is ANullExp)
                        {
                            //Swap left and right
                            PExp temp = node.GetLeft();
                            node.SetLeft(node.GetRight());
                            node.SetRight(temp);
                        }
                        PExp exp = node.GetLeft();
                        PType type = data.ExpTypes[exp];
                        if (type is APointerType)
                        {
                            if (node.GetBinop() is ANeBinop)
                            {
                                //Convert a != null to !(a == null)
                                AUnopExp unop = new AUnopExp(new AComplementUnop(new TComplement("!")), null);
                                node.ReplaceBy(unop);
                                unop.SetExp(node);
                                node.SetBinop(new AEqBinop(new TEq("==")));
                                data.ExpTypes[unop] = new ANamedType(new TIdentifier("bool"), null);
                            }
                            if (Util.IsIntPointer(node, ((APointerType)type).GetType(), data))
                            {
                                bool add = true;
                                foreach (PType pType in TypesWithIdentifierArray)
                                {
                                    if (Util.TypesEqual(((APointerType)type).GetType(), pType, data))
                                    {
                                        add = false;
                                        break;
                                    }
                                }
                                if (add)
                                    TypesWithIdentifierArray.Add(((APointerType)type).GetType());
                            }
                        }
                    }
                }
            }
        }

        //Convert bulk copy to pointers
        private class Phase0 : DepthFirstAdapter
        {
            private SharedData data;

            private Dictionary<AALocalDecl, PType> oldParameterTypes = new Dictionary<AALocalDecl, PType>();
            private Dictionary<AMethodDecl, PType> oldReturnTypes = new Dictionary<AMethodDecl, PType>();

            private class GetOldParameterTypes : DepthFirstAdapter
            {

                public Dictionary<AALocalDecl, PType> OldParameterTypes = new Dictionary<AALocalDecl, PType>();
                public Dictionary<AMethodDecl, PType> OldReturnTypes = new Dictionary<AMethodDecl, PType>();

                public override void CaseAALocalDecl(AALocalDecl node)
                {
                    OldParameterTypes.Add(node, node.GetType());
                    base.CaseAALocalDecl(node);
                }

                public override void CaseAMethodDecl(AMethodDecl node)
                {
                    OldReturnTypes.Add(node, node.GetReturnType());
                    base.CaseAMethodDecl(node);
                }
            }

            public Phase0(SharedData data)
            {
                this.data = data;
                mover = new MoveMethodDeclsOut("bulkCopyVar", data);
            }


            public override void  CaseAAProgram(AAProgram node)
            {
                GetOldParameterTypes getOldParameterTypes = new GetOldParameterTypes();
                node.Apply(getOldParameterTypes);
                oldParameterTypes = getOldParameterTypes.OldParameterTypes;
                oldReturnTypes = getOldParameterTypes.OldReturnTypes;
                foreach (AASourceFile sourceFile in node.GetSourceFiles())
                {
                    foreach (PDecl decl in sourceFile.GetDecl())
                    {
                        if (decl is AMethodDecl)
                        { 
                            GetUsedParameters getUses = new GetUsedParameters(data);
                            decl.Apply(getUses);
                            UsedParameters[(AMethodDecl) decl] = getUses.UsedParameters;
                        }
                    }
                }
 	            base.CaseAAProgram(node);
                while (mover.NewStatements.Count > 0)
                {
                    List<PStm> stms = mover.NewStatements;
                    mover = new MoveMethodDeclsOut("bulkCopyVar", data);
                    foreach (PStm stm in stms)
                    {
                        stm.Apply(this);
                    }
                }
            }

            List<AMethodDecl> processedMethods = new List<AMethodDecl>();
            public override void  CaseAMethodDecl(AMethodDecl node)
            {
                if (processedMethods.Contains(node))
                    return;
                processedMethods.Add(node);
                //For each bulk copy param, make it a pointer type
                foreach (AALocalDecl formal in node.GetFormals())
                {
                    PType type = formal.GetType();
                    if (Util.IsBulkCopy(type) || formal.GetRef() != null || formal.GetOut() != null)
                    {
                        if (type is AArrayTempType)
                        {//make dynamic array
                            AArrayTempType aType = (AArrayTempType) type;
                            List<PExp> exps = new List<PExp>();
                            PType newType = new APointerType(new TStar("*"),
                                                             new ADynamicArrayType((TLBracket)aType.GetToken().Clone(),
                                                                         Util.MakeClone(aType.GetType(), data)));
                            /*exps.AddRange(data.ExpTypes.Where(k => k.Value == type).Select(k => k.Key));
                            foreach (PExp exp in exps)
                            {
                                data.ExpTypes[exp] = newType;
                            }*/
                            formal.SetType(newType);
                            foreach (KeyValuePair<ALocalLvalue, AALocalDecl> pair in data.LocalLinks)
                            {
                                if (pair.Value == formal)
                                {//Replace with *lvalue
                                    ALvalueExp innerExp = new ALvalueExp();
                                    APointerLvalue replacement = new APointerLvalue(new TStar("*"), innerExp);
                                    pair.Key.ReplaceBy(replacement);
                                    innerExp.SetLvalue(pair.Key);
                                    data.ExpTypes[innerExp] = data.LvalueTypes[pair.Key] = formal.GetType();
                                    data.LvalueTypes[replacement] = ((APointerType)newType).GetType();

                                    //if (replacement.Parent() is ALvalueExp)
                                    //    data.ExpTypes[(PExp)replacement.Parent()] = data.LvalueTypes[replacement];
                                }
                            }
                        }
                        else
                        {//Make pointer
                            formal.SetType(new APointerType(new TStar("*"), type));
                            foreach (KeyValuePair<ALocalLvalue, AALocalDecl> pair in data.LocalLinks)
                            {
                                if (pair.Value == formal)
                                {//Replace with *lvalue
                                    ALvalueExp innerExp = new ALvalueExp();
                                    APointerLvalue replacement = new APointerLvalue(new TStar("*"), innerExp);
                                    pair.Key.ReplaceBy(replacement);
                                    innerExp.SetLvalue(pair.Key);
                                    data.ExpTypes[innerExp] = data.LvalueTypes[pair.Key] = formal.GetType();
                                    data.LvalueTypes[replacement] = type;
                                }
                            }
                        }
                    }
                }
                if (Util.IsBulkCopy(node.GetReturnType()))
                {
                    PType oldType = node.GetReturnType();
                    PType newType;
                    if (node.GetReturnType() is AArrayTempType)
                    {//make dynamic array
                        AArrayTempType aType = (AArrayTempType) node.GetReturnType();
                        newType = new APointerType(new TStar("*"),
                                                   new ADynamicArrayType((TLBracket) aType.GetToken().Clone(),
                                                                         Util.MakeClone(aType.GetType(), data)));
                        node.SetReturnType(newType);
                    }
                    else
                    {//Make pointer
                        newType = new APointerType(new TStar("*"), node.GetReturnType());
                        node.SetReturnType(newType);
                    }
                    /*List<PExp> exps = new List<PExp>();
                    exps.AddRange(data.ExpTypes.Where(k => k.Value == oldType).Select(k => k.Key));
                    foreach (PExp exp in exps)
                    {
                        data.ExpTypes[exp] = newType;
                    }
                    List<PLvalue> lvalues = new List<PLvalue>();
                    lvalues.AddRange(data.LvalueTypes.Where(k => k.Value == oldType).Select(k => k.Key));
                    foreach (PLvalue lvalue in lvalues)
                    {
                        data.LvalueTypes[lvalue] = newType;
                    }*/
                }
 	            base.CaseAMethodDecl(node);
            }
            
            public Dictionary<AMethodDecl, List<List<AALocalDecl>>> UsedParameters = new Dictionary<AMethodDecl, List<List<AALocalDecl>>>();
            private class GetUsedParameters : DepthFirstAdapter
            {
                private SharedData data;
                public List<List<AALocalDecl>> UsedParameters = new List<List<AALocalDecl>>();

                public GetUsedParameters(SharedData data)
                {
                    this.data = data;
                }

                public override void CaseALocalLvalue(ALocalLvalue node)
                {
                    //Register this unless parent is an lvalueExp, and then a structlvalu
                    AALocalDecl decl = data.LocalLinks[node];
                    RegiseterUse(node, new List<AALocalDecl> { decl });
                }

                private void RegiseterUse(PLvalue lvalue, List<AALocalDecl> tail)
                {
                    if (tail == null) tail = new List<AALocalDecl>();
                    if (lvalue.Parent().Parent() is AStructLvalue)
                    {
                        AStructLvalue parent = (AStructLvalue)lvalue.Parent().Parent();
                        AALocalDecl local = data.StructFieldLinks[parent];
                        AALocalDecl baseLocal = local;
                        if (data.EnheritanceLocalMap.ContainsKey(baseLocal))
                            baseLocal = data.EnheritanceLocalMap[baseLocal];
                        AStructDecl pStruct = (AStructDecl) local.Parent();
                        List<AALocalDecl> localsToTail = new List<AALocalDecl>();
                        localsToTail.AddRange(
                            data.EnheritanceLocalMap.Where(
                                pair =>
                                pair.Value == baseLocal && Util.Extends(pStruct, (AStructDecl) pair.Key.Parent(), data))
                                .Select(pair => pair.Key));
                        //If local == baseLocal, nothing is stored in enhritancelocalmap for them.
                        if (local == baseLocal)
                            localsToTail.Add(local);
                        foreach (AALocalDecl l in localsToTail)
                        {
                            List<AALocalDecl> newTail = new List<AALocalDecl>();
                            newTail.AddRange(tail);
                            newTail.Add(l);
                            RegiseterUse(parent, newTail);
                        }
                        return;
                    }
                    UsedParameters.Add(tail);
                }
            }

            private List<PStm> MakeAssignments(PLvalue lvalue, PExp exp, List<AALocalDecl> declChain)
            {
                PType type = data.LvalueTypes[lvalue]; //data.ExpTypes[exp];
                PType oldType = data.ExpTypes[exp];
                List<PStm> returner = new List<PStm>();
                if (type is ADynamicArrayType ||type is AArrayTempType)
                {
                    AArrayTempType aType;
                    if (type is ADynamicArrayType)
                        aType = (AArrayTempType) data.LvalueTypes[lvalue];
                    else
                        aType = (AArrayTempType) type;
                    for (int j = 0; j < int.Parse(aType.GetIntDim().Text); j++)
                    {
                        ALvalueExp bulkCopyRefExp = new ALvalueExp(Util.MakeClone(lvalue, data));
                        AIntConstExp index1 = new AIntConstExp(new TIntegerLiteral(j.ToString()));
                        AArrayLvalue leftSideIndex = new AArrayLvalue(new TLBracket("{"), bulkCopyRefExp, index1);
                                
                        AIntConstExp index2 = new AIntConstExp(new TIntegerLiteral(j.ToString()));
                        AArrayLvalue rightSideIndex = new AArrayLvalue(new TLBracket("{"), Util.MakeClone(exp, data), index2);
                        ALvalueExp rightSideExp = new ALvalueExp(rightSideIndex);
                        data.ExpTypes[bulkCopyRefExp] = data.LvalueTypes[lvalue];
                        data.ExpTypes[index1] =
                            data.ExpTypes[index2] = new ANamedType(new TIdentifier("int"), null);
                        data.LvalueTypes[leftSideIndex] =
                            data.LvalueTypes[rightSideIndex] =
                            data.ExpTypes[rightSideExp] = aType.GetType();
                        returner.AddRange(MakeAssignments(leftSideIndex, rightSideExp, declChain));
                    }
                }
                else if (type is ANamedType)
                {
                    ANamedType aType = (ANamedType) type;
                    if (Util.IsBulkCopy(type))
                    {
                        AStructDecl str = data.StructTypeLinks[aType];
                        foreach (AALocalDecl localDecl in str.GetLocals())
                        {
                            ALvalueExp bulkCopyRefExp = new ALvalueExp(Util.MakeClone(lvalue, data));
                            AStructLvalue leftSide = new AStructLvalue(bulkCopyRefExp, new ADotDotType(new TDot(".")),
                                                                       new TIdentifier(localDecl.GetName().Text));

                            AStructLvalue rightSide = new AStructLvalue(Util.MakeClone(exp, data), new ADotDotType(new TDot(".")),
                                                                       new TIdentifier(localDecl.GetName().Text));
                            ALvalueExp rightSideExp = new ALvalueExp(rightSide);
                            List<AALocalDecl> newDeclChain = new List<AALocalDecl>();
                            newDeclChain.AddRange(declChain);
                            newDeclChain.Add(localDecl);
                            data.StructFieldLinks[leftSide] =
                                data.StructFieldLinks[rightSide] = localDecl;
                            data.ExpTypes[bulkCopyRefExp] = data.LvalueTypes[lvalue];
                            data.LvalueTypes[leftSide] =
                                data.LvalueTypes[rightSide] =
                                data.ExpTypes[rightSideExp] = localDecl.GetType();
                            returner.AddRange(MakeAssignments(leftSide, rightSideExp, newDeclChain));
                        }
                    }
                    else
                    {
                        //Primitive, find out if it is used
                        if (declChain[0] == null || UsedParameters[Util.GetAncestor<AMethodDecl>(declChain[0])].Where(usedParameter => usedParameter.Count <= declChain.Count).Any(
                                usedParameter => !usedParameter.Where((t, i) => t != declChain[i]).Any()))
                        {
                            AAssignmentExp assignment = new AAssignmentExp(new TAssign("="),
                                                                           Util.MakeClone(lvalue, data),
                                                                           Util.MakeClone(exp, data));
                            data.ExpTypes[assignment] = data.LvalueTypes[lvalue];
                            returner.Add(new AExpStm(new TSemicolon(";"), assignment));
                        }
                    }
                }
                else if (type is APointerType)
                {
                    //Assign as primitive, find out if it is used
                    if (declChain[0] == null || UsedParameters[Util.GetAncestor<AMethodDecl>(declChain[0])].Where(usedParameter => usedParameter.Count <= declChain.Count).Any(
                            usedParameter => !usedParameter.Where((t, i) => t != declChain[i]).Any()))
                    {
                        AAssignmentExp assignment = new AAssignmentExp(new TAssign("="),
                                                                       Util.MakeClone(lvalue, data),
                                                                       Util.MakeClone(exp, data));
                        data.ExpTypes[assignment] = data.LvalueTypes[lvalue];
                        returner.Add(new AExpStm(new TSemicolon(";"), assignment));
                    }
                }
                else
                {
                    throw new Exception("Unexpected type. Got " + (type == null ? "null" : type.ToString()));
                }
                return returner;
            }

            private MoveMethodDeclsOut mover;

            private AALocalDecl TurnDynamic(PExp exp, AALocalDecl targetDecl, PType type, bool assignBefore = true, bool assignAfter = false, bool makeDelete = true)
            {

                exp.Apply(mover);
                //insert
                //<basetype>[]* bulkCopyVar = new <baseType>[<dim>]();
                //bulkCopyVar[0] = arg[0];
                //bulkCopyVar[1] = arg[1];
                //bulkCopyVar[2] = arg[2];
                //...
                //Invoke(..., bulkCopyVar, ...);

                //If we need to clean up after, move the exp to it's own statement.


                AALocalDecl newLocal;
                if (type is AArrayTempType)
                {
                    AArrayTempType aType = (AArrayTempType) type;
                    ANewExp newExp = new ANewExp(new TNew("new"), new AArrayTempType(
                                                                       new TLBracket("["),
                                                                       Util.MakeClone(aType.GetType(),
                                                                                      data),
                                                                       Util.MakeClone(aType.GetDimention(), data),
                                                                       (TIntegerLiteral)
                                                                       aType.GetIntDim().Clone()),
                                                 new ArrayList());
                    newLocal = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                               new APointerType(new TStar("*"),
                                                                new ADynamicArrayType(
                                                                    new TLBracket("["),
                                                                    Util.MakeClone(aType.GetType(),
                                                                                   data))),
                                               new TIdentifier("bulkCopyVar"),
                                               newExp
                        );
                }
                else
                {
                    ANewExp newExp = new ANewExp(new TNew("new"), Util.MakeClone(type, data),
                                                 new ArrayList());
                    newLocal = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                               new APointerType(new TStar("*"),
                                                                Util.MakeClone(type, data)),
                                               new TIdentifier("bulkCopyVar"),
                                               newExp
                        );
                }
                PStm pStm = Util.GetAncestor<PStm>(exp);
                AABlock pBlock = (AABlock) pStm.Parent();
                pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm),
                                                new ALocalDeclStm(new TSemicolon(";"), newLocal));
                PLvalue bulkCopyRef = new ALocalLvalue(new TIdentifier("bulkCopyVar"));
                data.LocalLinks[(ALocalLvalue) bulkCopyRef] = newLocal;
                data.LvalueTypes[bulkCopyRef] = newLocal.GetType();

                ALvalueExp bulkCopyRefExp;
                //if (!(type is AArrayTempType))
                {
                    bulkCopyRefExp = new ALvalueExp(bulkCopyRef);
                    bulkCopyRef = new APointerLvalue(new TStar("*"), bulkCopyRefExp);
                    data.ExpTypes[bulkCopyRefExp] = newLocal.GetType();
                    data.LvalueTypes[bulkCopyRef] = type;
                } 

                if (assignBefore)
                {
                    List<PStm> stms = MakeAssignments(bulkCopyRef, exp, new List<AALocalDecl>() {targetDecl});
                    foreach (PStm stm in stms)
                    {
                        pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm), stm);
                    }
                }
                int addStms = 1;
                if (assignAfter)
                {
                    bulkCopyRefExp = new ALvalueExp(bulkCopyRef);
                    data.ExpTypes[bulkCopyRefExp] = data.LvalueTypes[bulkCopyRef];
                    List<PStm> stms = MakeAssignments(((ALvalueExp)exp).GetLvalue(), bulkCopyRefExp, new List<AALocalDecl>() {targetDecl});
                    addStms += stms.Count;
                    foreach (PStm stm in stms)
                    {
                        pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm) + 1, stm);
                    }
                }

                bulkCopyRef = new ALocalLvalue(new TIdentifier("bulkCopyVar"));
                bulkCopyRefExp = new ALvalueExp(bulkCopyRef);
                data.LocalLinks[(ALocalLvalue) bulkCopyRef] = newLocal;
                data.LvalueTypes[bulkCopyRef] = data.ExpTypes[bulkCopyRefExp] = newLocal.GetType();
                exp.ReplaceBy(bulkCopyRefExp);

                /*if (formal.GetRef() != null || formal.GetOut() != null)
                {
                    //Get args back
                }*/
                //Delete object
                
                if (makeDelete)
                {
                    bulkCopyRef = new ALocalLvalue(new TIdentifier("bulkCopyVar"));
                    bulkCopyRefExp = new ALvalueExp(bulkCopyRef);
                    data.LocalLinks[(ALocalLvalue) bulkCopyRef] = newLocal;
                    data.LvalueTypes[bulkCopyRef] = data.ExpTypes[bulkCopyRefExp] = newLocal.GetType();
                    pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm) + addStms,
                                                  new ADeleteStm(new TDelete("delete"), bulkCopyRefExp));
                }
                return newLocal;
            }

            

            public override void  CaseAValueReturnStm(AValueReturnStm node)
            {
                PType type = data.ExpTypes[node.GetExp()];
                AMethodDecl pMethod = Util.GetAncestor<AMethodDecl>(node);
                if (Util.IsBulkCopy(type))
                {
                    TurnDynamic(node.GetExp(), null,
                                oldReturnTypes.ContainsKey(pMethod) ? oldReturnTypes[pMethod] : pMethod.GetReturnType(),
                                true, false, false);
                }
                if (pMethod != null && oldReturnTypes.ContainsKey(pMethod))
                {
                    PType oldType = oldReturnTypes[pMethod];
                    
                    
                    if (node.GetExp() is ANullExp &&
                        oldType is APointerType &&
                        Util.IsIntPointer(node, ((APointerType)oldType).GetType(), data))
                    {
                        AIntConstExp replacer = new AIntConstExp(new TIntegerLiteral("0"));
                        data.ExpTypes[replacer] = new ANamedType(new TIdentifier("int"), null);
                        node.GetExp().ReplaceBy(replacer);
                    }
                }
 	            base.CaseAValueReturnStm(node);
            }

            public override void  OutASimpleInvokeExp(ASimpleInvokeExp node)
            {
                if (data.BulkCopyProcessedInvokes.Contains(node))
                {
                    base.OutASimpleInvokeExp(node);
                    return;
                }

                //If anything needs to be put after the invoke, move it to it's own local decl or exp statement
                AMethodDecl method = data.SimpleMethodLinks[node];

                if (!processedMethods.Contains(method))
                    CaseAMethodDecl(method);

                bool moveOut = Util.IsBulkCopy(data.ExpTypes[node]);
                PType type;
                if (!moveOut)
                    for (int i = 0; i < node.GetArgs().Count; i++)
                    {
                        PExp arg = (PExp)node.GetArgs()[i];
                        AALocalDecl formal = (AALocalDecl)method.GetFormals()[i];
                        type = data.ExpTypes[arg];
                        if (Util.IsBulkCopy(type) || formal.GetRef() != null || formal.GetOut() != null)
                        {
                            moveOut = true;
                            break;
                        }
                    }
                if (moveOut && !(node.Parent() is AExpStm || node.Parent() is AALocalDecl))
                {
                    PStm pStm = Util.GetAncestor<PStm>(node);
                    AABlock pBlock = (AABlock) pStm.Parent();

                    //Can not be a void type, since it is not in an expStm
                    AALocalDecl localDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, Util.MakeClone(data.ExpTypes[node], data),
                                                            new TIdentifier("bulkCopyVar"), null);
                    ALocalLvalue bulkCopyVarRef = new ALocalLvalue(new TIdentifier("bulkCopyVar"));
                    ALvalueExp bulkCopyVarRefExp = new ALvalueExp(bulkCopyVarRef);
                    node.ReplaceBy(bulkCopyVarRefExp);
                    localDecl.SetInit(node);
                    pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm), new ALocalDeclStm(new TSemicolon(";"), localDecl));

                    data.LocalLinks[bulkCopyVarRef] = localDecl;
                    data.LvalueTypes[bulkCopyVarRef] =
                        data.ExpTypes[bulkCopyVarRefExp] = localDecl.GetType();
                }
               


                //Replace bulk copy arguments with a new pointer
                for (int i = 0; i < node.GetArgs().Count; i++)
                {
                    PExp arg = (PExp) node.GetArgs()[i];
                    AALocalDecl formal = (AALocalDecl) method.GetFormals()[i];
                    if (oldParameterTypes.ContainsKey(formal))
                        type = oldParameterTypes[formal];// data.ExpTypes[arg];
                    else
                        type = formal.GetType();
                    if (Util.IsBulkCopy(type) || formal.GetRef() != null || formal.GetOut() != null)
                    {
                        if (formal.GetRef() != null && arg is ALvalueExp)
                        {
                            ALvalueExp aArg = (ALvalueExp) arg;
                            if (aArg.GetLvalue() is APointerLvalue)
                            {
                                APointerLvalue pointer = (APointerLvalue) aArg.GetLvalue();
                                if (Util.TypesEqual(formal.GetType(), data.ExpTypes[pointer.GetBase()], data))
                                {//Just send the arg
                                    aArg.ReplaceBy(pointer.GetBase());
                                    continue;
                                }
                            }
                        }

                        TurnDynamic(arg, formal,
                                    oldParameterTypes.ContainsKey(formal) ? oldParameterTypes[formal] : formal.GetType(),
                                    formal.GetOut() == null,
                                    formal.GetRef() != null || formal.GetOut() != null);
                    }
                }
                //Do return stm
                type = data.ExpTypes[node];
                if (Util.IsBulkCopy(type))
                {
                    PStm pStm = Util.GetAncestor<PStm>(node);
                    AABlock pBlock = (AABlock)pStm.Parent();
                    bool isReturnUsed = !(node.Parent() is AExpStm);
                    if (isReturnUsed)
                    {
                        //Make 
                        //var bulkCopyVar = <node>(...);
                        //<usage>... *bulkCopyVar ... </usage>
                        //delete bulkCopyVar;
                        bool isArray = type is AArrayTempType;
                        AALocalDecl newLocal = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                               new APointerType(new TStar("*"),
                                                                                isArray
                                                                                    ? new ADynamicArrayType(
                                                                                          new TLBracket("["),
                                                                                          Util.MakeClone(
                                                                                              ((AArrayTempType) type).
                                                                                                  GetType(), data))
                                                                                    : Util.MakeClone(type, data)),
                                                               new TIdentifier("bulkCopyVar"), null);
                        data.ExpTypes[node] = newLocal.GetType();

                        ALocalLvalue newLocalRef;
                        ALvalueExp newLocalRefExp;
                        newLocalRef = new ALocalLvalue(new TIdentifier("bulkCopyVar"));
                        newLocalRefExp = new ALvalueExp(newLocalRef);
                        APointerLvalue newLocalPointer = new APointerLvalue(new TStar("*"), newLocalRefExp);
                        ALvalueExp newLocalPointerExp = new ALvalueExp(newLocalPointer);
                        node.ReplaceBy(newLocalPointerExp);

                        data.LocalLinks[newLocalRef] = newLocal;
                        data.LvalueTypes[newLocalRef] =
                            data.ExpTypes[newLocalRefExp] = newLocal.GetType();
                        data.LvalueTypes[newLocalPointer] =
                            data.ExpTypes[newLocalPointerExp] = ((APointerType) newLocal.GetType()).GetType();
                        

                        newLocal.SetInit(node);


                        newLocalRef = new ALocalLvalue(new TIdentifier("bulkCopyVar"));
                        newLocalRefExp = new ALvalueExp(newLocalRef);
                        data.LocalLinks[newLocalRef] = newLocal;
                        data.LvalueTypes[newLocalRef] =
                            data.ExpTypes[newLocalRefExp] = newLocal.GetType();

                        pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm),
                                                      new ALocalDeclStm(new TSemicolon(";"), newLocal));
                        pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm) + 1,
                                                      new ADeleteStm(new TDelete("delete"), newLocalRefExp));
                    }
                    else
                    {
                        //Make delete <node>(...);
                        pStm.ReplaceBy(new ADeleteStm(new TDelete("delete"), node));
                    }
                }
                base.OutASimpleInvokeExp(node);
            }

            public override void OutAALocalDecl(AALocalDecl node)
            {
                if (node.GetInit() != null)
                {
                    PType type = node.GetType();
                    if (Util.IsBulkCopy(type))
                    {
                        node.Apply(mover);
                        ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier(node.GetName().Text));
                        data.LocalLinks[lvalue] = node;
                        data.LvalueTypes[lvalue] = type;
                        List<PStm> replacementStatements = MakeAssignments(lvalue, node.GetInit(),
                                                                           new List<AALocalDecl>() { null });
                        PStm pStm = Util.GetAncestor<PStm>(node);
                        AABlock pBlock = (AABlock)pStm.Parent();
                        foreach (PStm stm in replacementStatements)
                        {
                            pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm) + 1, stm);
                        }
                        node.SetInit(null);
                    }
                    
                }
                base.OutAALocalDecl(node);
            }

            public override void OutAAssignmentExp(AAssignmentExp node)
            {
                PType type = data.ExpTypes[node];
                if (Util.IsBulkCopy(type))
                {
                    node.Apply(mover);
                    List<PStm> replacementStatements = MakeAssignments(node.GetLvalue(), node.GetExp(),
                                                                       new List<AALocalDecl>() {null});
                    PStm pStm = Util.GetAncestor<PStm>(node);
                    AABlock pBlock = (AABlock) pStm.Parent();
                    foreach (PStm stm in replacementStatements)
                    {
                        pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm), stm);
                    }
                    pBlock.RemoveChild(pStm);
                }
                base.OutAAssignmentExp(node);
            }
        }

            


        //Insert runtime checks
        private class Phase1v2 : DepthFirstAdapter
        {
            /*
             * Analyze what values each variable can have (a = val, a < val, etc..)
             * When doing this, try to get the value of array indexes, if possible (index = val), and set analyze data for that specifc index
             * Also, try to analyze the length of dynamic arrays
             * 
             * When writing *<exp>. If this exp is not garaunteed to have a value, make a check
             * When writing <exp>[index], and exp is a dynamic array, if index can't be resolved, or array length is not resolved, or the index is 
             *      calculated to be outside the array, make a runtime check.
             *      
             * Keep in mind that control flow has an effect on theese things. (ifs, whiles, returns, breaks, etc..)
             * 
             * Conditional expressions to analyze: 
             *  - <exp> && <exp>
             *  - <variable> [==, !=, <, >, <=, >=] <exp> //or vice verca
             *  - Ignore others
             *  
             * Predicate := [==, !=, <, >, <=, >=] [int, fixed] || [==, !=] all other || unknown
             * 
             * assigned variables in whiles are unknown
             * after while & if, intersect the lists after with before
             * similar for if then else.
             */

            /* Variables
             * 
             * currentVariable : List<PointerType>
             * currentValue : List<predicate>
             * isExposed : bool (not pointer => false)
             * hasReturned : bool
             * hasBreakOrContinue : bool
             * 
             * localValues : Dictonary<Var, List<predicate>>
             * exposedVariables : List<Var>
             * 
             */

            private interface PointerType
            {

            }

            private class LocalDeclPointer : PointerType
            {
                public AALocalDecl LocalDecl;

                public LocalDeclPointer(AALocalDecl localDecl)
                {
                    LocalDecl = localDecl;
                }
            }

            private class FieldDeclPointer : PointerType
            {
                public AFieldDecl FieldDecl;

                public FieldDeclPointer(AFieldDecl fieldDecl)
                {
                    FieldDecl = fieldDecl;
                }
            }

            private class PointerPointer : PointerType
            {
            }

            private class ArrayLengthPointer : PointerType
            {
                public int FixedLength = -1;

                public ArrayLengthPointer(int fixedLength)
                {
                    FixedLength = fixedLength;
                }

                public ArrayLengthPointer()
                {
                }
            }

            private class ArrayIndexPointer : PointerType
            {
                public int Index = -1;

                public ArrayIndexPointer(int fixedLength)
                {
                    Index = fixedLength;
                }

                public ArrayIndexPointer()
                {
                }
            }

            private enum PredicateType
            {// [==, !=, <, >, <=, >=] [int, fixed] || [==, !=] all other || unknown
                Eq, Neq, Lt, Gt, Lteq, Gteq, Unknown
            }

            private class Predicate
            {
                public Predicate()
                {
                }
                public Predicate(PredicateType predicateType, string value)
                {
                    PredicateType = predicateType;
                    Value = value;
                }

                public PredicateType PredicateType = PredicateType.Unknown;
                public string Type;
                public string Value;
            }
            /*
             * currentVariable : List<PointerType>
             * currentValue : List<predicate>
             * isExposed : bool (not pointer => false)
             * hasReturned : bool
             * hasBreakOrContinue : bool
             * 
             * localValues : Dictonary<Var, List<predicate>>
             * exposedVariables : List<Var>
             */

            private SharedData data;

            public Phase1v2(SharedData data)
            {
                this.data = data;
            }

            List<PointerType> currentVariable = new List<PointerType>();
            List<Predicate> currentValue = new List<Predicate>();
            private bool isExposed;
            private bool hasReturned;
            private bool hasBreaked;

            private Dictionary<List<PointerType>, List<Predicate>> variableValues = new Dictionary<List<PointerType>, List<Predicate>>();
            private List<List<PointerType>> expoesedVariables = new List<List<PointerType>>();

            //int* a => local a; local a, pointer
            //struct foo{int* bar; int[] foobar} foo* baz => local baz
            /* local baz
             * local baz -> pointer
             * local baz -> pointer -> local bar
             * local baz -> pointer -> local bar -> pointer
             * local baz -> pointer -> local foobar
             * local baz -> pointer -> local foobar -> arrayLength
             * local baz -> pointer -> local foobar -> arrayIndex i
             * 
             * 
             * 
             * 
             * 
             */
            private List<List<PointerType>> GetAllVariables(AALocalDecl localDecl)
            {
                //You always want to append a 
                List<List<PointerType>> ret = GetAllVariables(localDecl, localDecl.GetType());
                foreach (List<PointerType> value in ret)
                {
                    value.Insert(0, new LocalDeclPointer(localDecl));
                }
                ret.Add(new List<PointerType>(){new LocalDeclPointer(localDecl)});
                return ret;
            }



            /*private List<List<PointerType>> GetAllVariables(AALocalDecl localDecl, PType type)
            {
                List<List<PointerType>> ret = new List<List<PointerType>>();
                if (type is APointerType)
                {
                    //This pointer can have a value
                    ret.Add(new List<PointerType>(){new PointerPointer()});

                    foreach (List<PointerType> child in GetAllVariables(localDecl, ((APointerType)type).GetType()))
                    {
                        child.Insert(0, new PointerPointer());
                        ret.Add(child);
                    }
                }
                else if (type is ANamedType && Util.IsBulkCopy(type))
                {
                    AStructDecl structDecl = data.StructTypeLinks[(ANamedType)type];
                    foreach (AALocalDecl local in structDecl.GetLocals())
                    {
                        ret.AddRange(GetAllVariables(local));
                    }
                }
                else if (type is ADynamicArrayType)
                {
                    
                }
            }*/

            private List<List<PointerType>> GetAllVariables(AALocalDecl localDecl, PType type, bool wasPointer = false)
            {
                List<List<PointerType>> returner = new List<List<PointerType>>();
                if (type is APointerType)
                {
                    returner.Add(type == localDecl.GetType() && !wasPointer
                                     ? new List<PointerType>() { new LocalDeclPointer(localDecl) }
                                     : new List<PointerType>() { new PointerPointer() });
                    foreach (List<PointerType> pointer in GetAllVariables(localDecl, ((APointerType)type).GetType(), true))
                    {
                        pointer.Insert(0, type == localDecl.GetType() && !wasPointer
                                                ? new LocalDeclPointer(localDecl)
                                                : (PointerType)new PointerPointer());
                        returner.Add(pointer);
                    }
                }
                else if (type is ANamedType)
                {
                    if (Util.IsBulkCopy(type))
                    {
                        AStructDecl structDecl = data.StructTypeLinks[(ANamedType) type];
                        foreach (AALocalDecl local in structDecl.GetLocals())
                        {
                            List<List<PointerType>> returnedStuff = GetAllVariables(local, local.GetType());
                            foreach (List<PointerType> pointers in returnedStuff)
                            {
                                pointers.Insert(0,
                                                wasPointer
                                                    ? (PointerType) new PointerPointer()
                                                    : new LocalDeclPointer(localDecl));
                                returner.Add(pointers);
                            }
                        }
                    }
                    else
                    {
                        //returner.a
                    }
                }
                return returner;
            }

            private List<List<PointerType>> GetAllVariables(AFieldDecl fieldDecl, PType type, bool wasPointer = false)
            {
                List<List<PointerType>> returner = new List<List<PointerType>>();
                if (type is APointerType)
                {
                    returner.Add(type == fieldDecl.GetType() && !wasPointer
                                     ? new List<PointerType>() { new FieldDeclPointer(fieldDecl) }
                                     : new List<PointerType>() { new PointerPointer() });
                    foreach (List<PointerType> pointer in GetAllVariables(fieldDecl, ((APointerType)type).GetType(), true))
                    {
                        pointer.Insert(0, type == fieldDecl.GetType() && !wasPointer
                                                ? new FieldDeclPointer(fieldDecl)
                                                : (PointerType)new PointerPointer());
                        returner.Add(pointer);
                    }
                }
                else if (type is ANamedType && Util.IsBulkCopy(type))
                {
                    AStructDecl structDecl = data.StructTypeLinks[(ANamedType)type];
                    foreach (AALocalDecl local in structDecl.GetLocals())
                    {
                        List<List<PointerType>> returnedStuff = GetAllVariables(local, local.GetType());
                        foreach (List<PointerType> pointers in returnedStuff)
                        {
                            pointers.Insert(0, wasPointer ? (PointerType)new PointerPointer() : new FieldDeclPointer(fieldDecl));
                            returner.Add(pointers);
                        }
                    }
                }
                return returner;
            }

            private List<PointerType> GetValue(List<List<PointerType>> list, List<PointerType> pointer)
            {
                foreach (List<PointerType> p in list)
                {
                    if (p.Count != pointer.Count)
                        continue;
                    bool match = true;
                    for (int i = 0; i < pointer.Count; i++)
                    {
                        if (p[i].GetType() != pointer[i].GetType())
                        {
                            match = false;
                            break;
                        }
                        if (p[i] is LocalDeclPointer && ((LocalDeclPointer)p[i]).LocalDecl != ((LocalDeclPointer)pointer[i]).LocalDecl)
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                        return p;
                }
                return null;
            }

            private bool Contains(List<List<PointerType>> list, List<PointerType> pointer)
            {
                return GetValue(list, pointer) != null;
            }

            public override void CaseAAProgram(AAProgram node)
            {
                //Add all fields to exposed
                foreach (AASourceFile sourceFile in node.GetSourceFiles())
                {
                    foreach (PDecl decl in sourceFile.GetDecl())
                    {
                        if (decl is AFieldDecl)
                        {
                            AFieldDecl aDecl = (AFieldDecl) decl;
                            //foreach (List<PointerType> variable in GetAllPointerTypes(aDecl, aDecl.GetType()))
                            {
                                
                            }
                        }
                    }
                }
                base.CaseAAProgram(node);
            }

            public override void CaseAMethodDecl(AMethodDecl node)
            {
                variableValues.Clear();
                base.CaseAMethodDecl(node);
                hasReturned = false;
            }
        }
        private class Phase1 : DepthFirstAdapter
        {
            private SharedData data;

            public Phase1(SharedData data)
            {
                this.data = data;
            }

            /*
             * Analyze what values each variable can have (a = val, a < val, etc..)
             * When doing this, try to get the value of array indexes, if possible (index = val), and set analyze data for that specifc index
             * Also, try to analyze the length of dynamic arrays
             * 
             * When writing *<exp>. If this exp is not garaunteed to have a value, make a check
             * When writing <exp>[index], and exp is a dynamic array, if index can't be resolved, or array length is not resolved, or the index is 
             *      calculated to be outside the array, make a runtime check.
             *      
             * Keep in mind that control flow has an effect on theese things. (ifs, whiles, returns, breaks, etc..)
             * 
             * 
             */ 

            /* Null pointer checks
             * 
             * null/set/unsure
             * 
             * Arrays are always unsure
             * Uninitialized pointers are always null
             * Pointers recieved from globals are always unsure.
             * If a local pointer is exposed to the outside, and a user method call or wait is called, those pointers are unsure
             * 
             * Remember
             * int* a = new...;
             * int* b = a;
             * foo(b); <- a and b is exposed
             * 
             * 
             * int** a;//a can be null, and *a can be null
             * local a
             * local a, pointer
             * 
             * Foo
             * {
             *  int* a;
             * }
             * 
             * Foo** f;
             * f, *f, (**f).a can be null
             * 
             * int*[] a;
             * a can be null
             * a[i] is always unsure
             * 
             * Expressions can be set or not, exposed or not
             * 
             * new expressions set and not exposed.
             * Lvalues are looked up in the map, if the base is in the map. If the base or the lvalue isn't in the map, assume not set and exposed
             * simple invokes are always not set and exposed
             * 
             * root not set => child not set
             * root exposed => child exposed
             * 
             */

            private interface PointerType
            {
                
            }

            private class LocalDeclPointer : PointerType
            {
                public AALocalDecl LocalDecl;

                public LocalDeclPointer(AALocalDecl localDecl)
                {
                    this.LocalDecl = localDecl;
                }
            }

            private class PointerPointer : PointerType
            {
            }

            //private List<List<PointerType>> nullPointers = new List<List<PointerType>>();
            private List<List<PointerType>> setPointers = new List<List<PointerType>>();
            private List<List<PointerType>> exposedPointers = new List<List<PointerType>>();
            private List<List<List<PointerType>>> pointerGroups = new List<List<List<PointerType>>>();
            private List<List<PointerType>> generatedPointers = new List<List<PointerType>>();
            private List<PointerType> currentPointer = new List<PointerType>();
            private bool isSet;
            private bool isExposed;

            
            private bool Contains(List<List<PointerType>> list, List<PointerType> pointer)
            {
                foreach (List<PointerType> p in list)
                {
                    if (p.Count != pointer.Count)
                        continue;
                    bool match = true;
                    for (int i = 0; i < pointer.Count; i++)
                    {
                        if (p[i].GetType() != pointer[i].GetType())
                        {
                            match = false;
                            break;
                        }
                        if (p[i] is LocalDeclPointer && ((LocalDeclPointer)p[i]).LocalDecl != ((LocalDeclPointer)pointer[i]).LocalDecl)
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                        return true;
                }
                return false;
            }

            private List<PointerType> MakePointer(List<PointerType> pointer)
            {
                foreach (List<PointerType> generatedPointer in generatedPointers)
                {
                    if (pointer.Count != generatedPointer.Count)
                        continue;
                    bool match = true;
                    for (int i = 0; i < pointer.Count; i++)
                    {
                        if (pointer[i].GetType() != generatedPointer[i].GetType())
                        {
                            match = false;
                            break;
                        }
                        if (pointer[i] is LocalDeclPointer && ((LocalDeclPointer)pointer[i]) != ((LocalDeclPointer)generatedPointer[i]))
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                        return generatedPointer;
                }
                generatedPointers.Add(pointer);
                return pointer;
            }

            private List<List<PointerType>> GetPrefixes(List<PointerType> pointer)
            {
                List<List<PointerType>> returner = new List<List<PointerType>>();
                foreach (List<PointerType> generatedPointer in generatedPointers)
                {
                    if (pointer.Count > generatedPointer.Count)
                        continue;
                    bool match = true;
                    for (int i = 0; i < pointer.Count; i++)
                    {
                        if (pointer[i].GetType() != generatedPointer[i].GetType())
                        {
                            match = false;
                            break;
                        }
                        if (pointer[i] is LocalDeclPointer && ((LocalDeclPointer)pointer[i]) != ((LocalDeclPointer)generatedPointer[i]))
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                        returner.Add(generatedPointer);
                }
                return returner;
            }

            public override void InAMethodDecl(AMethodDecl node)
            {
                //nullPointers.Clear();
                setPointers.Clear();
                exposedPointers.Clear();
                generatedPointers.Clear();

                //Paremeter pointers are exposed
                foreach (AALocalDecl formal in node.GetFormals())
                {
                    foreach (List<PointerType> pointer in GetAllPointerTypes(formal, formal.GetType()))
                    {
                        exposedPointers.Add(MakePointer(pointer));
                    }
                }
            }
            /*
             * str
             * {
             *  int* i;
             *  }
             *  
             * str* foo;
             * 
             * localDecl foo
             * localDecl foo, pointer, localdecl i
             * 
             * 
             * */
            private List<List<PointerType>> GetAllPointerTypes(AALocalDecl localDecl, PType type, bool wasPointer = false)
            {
                List<List<PointerType>> returner = new List<List<PointerType>>();
                if (type is APointerType)
                {
                    returner.Add(type == localDecl.GetType() && !wasPointer
                                     ? new List<PointerType>() {new LocalDeclPointer(localDecl)}
                                     : new List<PointerType>() {new PointerPointer()});
                    foreach (List<PointerType> pointer in GetAllPointerTypes(localDecl, ((APointerType)type).GetType(), true))
                    {
                        pointer.Insert(0, type == localDecl.GetType() && !wasPointer
                                                ? new LocalDeclPointer(localDecl)
                                                : (PointerType) new PointerPointer());
                        returner.Add(pointer);
                    }
                }
                else if (type is ANamedType && Util.IsBulkCopy(type))
                {
                    AStructDecl structDecl = data.StructTypeLinks[(ANamedType)type];
                    foreach (AALocalDecl local in structDecl.GetLocals())
                    {
                        List<List<PointerType>> returnedStuff = GetAllPointerTypes(local, local.GetType());
                        foreach (List<PointerType> pointers in returnedStuff)
                        {
                            pointers.Insert(0, wasPointer ? (PointerType)new PointerPointer() : new LocalDeclPointer(localDecl));
                            returner.Add(pointers);
                        }
                    }
                }
                return returner;
            }


            public override void CaseAALocalDecl(AALocalDecl node)
            {
                /*foreach (List<PointerType> pointerType in GetAllPointerTypes(node, node.GetType()))
                {
                    MakePointer(pointerType);
                }*/

                //Parameter
                if (Util.GetAncestor<AMethodDecl>(node) == null)
                    return;

                {
                    /*currentPointer.Clear();
                    node.GetLvalue().Apply(this);
                    List<PointerType> leftSide = MakePointer(currentPointer);*/
                    if (node.GetInit() == null)
                    {
                        isSet = false;
                        isExposed = false;
                    }
                    else
                    {
                        currentPointer.Clear();
                        node.GetInit().Apply(this);
                    }
                    //List<PointerType> rightSide = MakePointer(currentPointer);
                    foreach (List<PointerType> prefix in GetAllPointerTypes(node, node.GetType()))
                    {
                        //if prefix is root - set to isSet / isExposed
                        //if prefix is child - set isSet only if false, and isExposed only if true.
                        bool isRoot = prefix.Count == 1;

                        List<PointerType> aPrefix = MakePointer(prefix);
                        if (isSet != Contains(setPointers, aPrefix))
                        {
                            if (isSet)
                            {
                                if (isRoot)
                                    setPointers.Add(aPrefix);
                            }
                            else
                                setPointers.Remove(aPrefix);
                        }

                        if (isExposed != Contains(exposedPointers, aPrefix))
                        {
                            if (isSet)
                                setPointers.Add(aPrefix);
                            else if (isRoot)
                                setPointers.Remove(aPrefix);
                        }
                    }
                }
            }

            public override void CaseAAssignmentExp(AAssignmentExp node)
            {
                /*if (!(data.ExpTypes[node] is APointerType))
                {
                    base.CaseAAssignmentExp(node);
                    return;
                }*/
                

                {
                    currentPointer.Clear();
                    node.GetLvalue().Apply(this);
                    List<PointerType> leftSide = MakePointer(currentPointer);

                    currentPointer.Clear();
                    node.GetExp().Apply(this);
                    //List<PointerType> rightSide = MakePointer(currentPointer);
                    if (leftSide.Count > 0)
                        foreach (List<PointerType> prefix in GetPrefixes(leftSide))
                        {
                            bool isRoot = prefix == leftSide;

                            if (isSet != Contains(setPointers, prefix))
                            {
                                if (isSet)
                                {
                                    if (isRoot)
                                        setPointers.Add(prefix);
                                }
                                else
                                    setPointers.Remove(prefix);
                            }

                            if (isExposed != Contains(exposedPointers, prefix))
                            {
                                if (isSet)
                                    setPointers.Add(prefix);
                                else if (isRoot)
                                    setPointers.Remove(prefix);
                            }
                        }

                    
                }
              
            }

            public override void CaseANewExp(ANewExp node)
            {
                isSet = true;
                isExposed = false;
                currentPointer.Clear();
            }
            
            public override void CaseASimpleInvokeExp(ASimpleInvokeExp node)
            {
                //If any pointers are passed to the method, they are now exposed
                foreach (PExp arg in node.GetArgs())
                {
                    if (arg is ALvalueExp)
                    {
                        currentPointer.Clear();
                        arg.Apply(this);
                        if (currentPointer.Count > 0)
                        {
                            //Each pointer that is a prefix 
                            foreach (List<PointerType> pointer in GetPrefixes(currentPointer))
                            {
                                if (!Contains(exposedPointers, pointer))
                                    exposedPointers.Add(pointer);
                            }
                        }
                    }
                }

                AMethodDecl method = data.SimpleMethodLinks[node];
                if (method.GetName().Text == "Wait" || data.Libraries.Methods.Contains(method))
                    foreach (List<PointerType> exposedPointer in exposedPointers)
                    {
                        setPointers.Remove(exposedPointer);
                    }

                OutASimpleInvokeExp(node);
            }

            public override void OutASimpleInvokeExp(ASimpleInvokeExp node)
            {
                isSet = false;
                isExposed = false;
                currentPointer.Clear();
            }

            public override void CaseAPointerLvalue(APointerLvalue node)
            {
                //Build the list
                currentPointer.Clear();
                base.CaseAPointerLvalue(node);

                //Todo: insert runtime check here
                //if (currentPointer.Count == 0 || !setPointers.Contains(MakePointer(currentPointer)))
                if (!isSet)
                {
                    PStm pStm = Util.GetAncestor<PStm>(node);
                    if (pStm != null)
                    {
                        AABlock pBlock = (AABlock) pStm.Parent();
                        /*
                         *  if (<pointer> == null)
                         *  {
                         *      UIDisplayMessage(PlayerGroupAll, messageAreaDebug, StringToText(<filename>[<lineNr>:<pos>] + " null pointer exception"));
                         *      int i = 1 / 0;
                         *  }
                         */
                        AASourceFile currentSourceFile = Util.GetAncestor<AASourceFile>(node);

                        node.GetBase().Apply(new MoveMethodDeclsOut("pointerVar", data));
                        PExp pointer = Util.MakeClone(node.GetBase(), data);
                        ABinopExp cond = new ABinopExp(pointer, new AEqBinop(new TEq("==")), new ANullExp());
                        AABlock ifBlock = new AABlock();
                        ASimpleInvokeExp playerGroupAllInvoke = new ASimpleInvokeExp(new TIdentifier("PlayerGroupAll"), new ArrayList());
                        AFieldLvalue messageAreaDebugLink = new AFieldLvalue(new TIdentifier("c_messageAreaDebug"));
                        ALvalueExp messageAreaDebugLinkExp = new ALvalueExp(messageAreaDebugLink);
                        ASimpleInvokeExp stringToTextInvoke = new ASimpleInvokeExp(new TIdentifier("StringToText"),
                                                                                   new ArrayList()
                                                                                       {
                                                                                           new AStringConstExp(
                                                                                               new TStringLiteral("\"" +
                                                                                                                  currentSourceFile
                                                                                                                      .
                                                                                                                      GetName
                                                                                                                      ()
                                                                                                                      .
                                                                                                                      Text +
                                                                                                                  "[" +
                                                                                                                  node.
                                                                                                                      GetTokens
                                                                                                                      ()
                                                                                                                      .
                                                                                                                      Line +
                                                                                                                  "," +
                                                                                                                  node.
                                                                                                                      GetTokens
                                                                                                                      ()
                                                                                                                      .
                                                                                                                      Pos +
                                                                                                                  "]: Null pointer exception\""))
                                                                                       });
                        ASimpleInvokeExp displayMessageInvoke = new ASimpleInvokeExp(
                            new TIdentifier("UIDisplayMessage"),
                            new ArrayList() {playerGroupAllInvoke, messageAreaDebugLinkExp, stringToTextInvoke});
                        ifBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), displayMessageInvoke));

                        ABinopExp iDeclInit = new ABinopExp(new AIntConstExp(new TIntegerLiteral("1")),
                                                            new ADivideBinop(new TDiv("/")),
                                                            new AIntConstExp(new TIntegerLiteral("0")));
                        AALocalDecl iDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, new ANamedType(new TIdentifier("int"), null), new TIdentifier("i"), iDeclInit);
                        ifBlock.GetStatements().Add(new ALocalDeclStm(new TSemicolon(";"), iDecl));

                        pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm),
                                                      new AIfThenStm(new TLParen("("), cond,
                                                                     new ABlockStm(new TLBrace("{"), ifBlock)));

                        data.Locals[ifBlock] = new List<AALocalDecl>(){iDecl};
                        data.ExpTypes[cond.GetRight()] = new ANullType();
                        data.ExpTypes[cond] = new ANamedType(new TIdentifier("bool"), null);
                        data.ExpTypes[playerGroupAllInvoke] = new ANamedType(new TIdentifier("playergroup"), null);
                        data.ExpTypes[messageAreaDebugLinkExp] =
                            data.LvalueTypes[messageAreaDebugLink] =
                            data.ExpTypes[iDeclInit] =
                            data.ExpTypes[iDeclInit.GetLeft()] =
                            data.ExpTypes[iDeclInit.GetRight()] = new ANamedType(new TIdentifier("int"), null);
                        data.ExpTypes[stringToTextInvoke] = new ANamedType(new TIdentifier("text"), null);
                        data.ExpTypes[(PExp) stringToTextInvoke.GetArgs()[0]] = new ANamedType(new TIdentifier("string"), null);
                        data.ExpTypes[displayMessageInvoke] = new AVoidType(new TVoid("void"));

                        data.SimpleMethodLinks[playerGroupAllInvoke] =
                            data.Libraries.Methods.Find(m => m.GetName().Text == playerGroupAllInvoke.GetName().Text);
                        data.SimpleMethodLinks[displayMessageInvoke] =
                            data.Libraries.Methods.Find(m => m.GetName().Text == displayMessageInvoke.GetName().Text);
                        data.SimpleMethodLinks[stringToTextInvoke] =
                            data.Libraries.Methods.Find(m => m.GetName().Text == stringToTextInvoke.GetName().Text);
                        data.FieldLinks[messageAreaDebugLink] =
                            data.Libraries.Fields.Find(f => f.GetName().Text == messageAreaDebugLink.GetName().Text);

                        if (currentPointer.Count > 0)
                        {
                            setPointers.Add(MakePointer(currentPointer));
                        }
                    }
                }

                currentPointer.Add(new PointerPointer());
                currentPointer = MakePointer(currentPointer);
                isSet = Contains(setPointers, currentPointer);
                isExposed = Contains(exposedPointers, currentPointer);

                //If the currentPointer is in null pointers, report error.. and then again - we might not reach this statement - warning, and runtime check
                //If the currentPointer is not in setPointers, insert runtime check
            }

            public override void CaseAArrayLvalue(AArrayLvalue node)
            {
                base.CaseAArrayLvalue(node);
                currentPointer.Clear();//After an array index, it's always unsure - can't analyze what the index will be at runtime
                isSet = false;
                isExposed = true;
            }

            public override void CaseALocalLvalue(ALocalLvalue node)
            {
                currentPointer.Add(new LocalDeclPointer(data.LocalLinks[node]));
                currentPointer = MakePointer(currentPointer);
                isSet = Contains(setPointers, currentPointer);
                isExposed = Contains(exposedPointers, currentPointer);
            }

            public override void OutAStructLvalue(AStructLvalue node)
            {
                if (currentPointer.Count > 0)
                {
                    currentPointer.Add(new LocalDeclPointer(data.StructFieldLinks[node]));
                    currentPointer = MakePointer(currentPointer);
                    isSet = Contains(setPointers, currentPointer);
                    isExposed = Contains(exposedPointers, currentPointer);
                }
            }

        }

        //Fix pointers
        private class Phase2 : DepthFirstAdapter
        {
            private SharedData data;
            private static AMethodDecl newObjectMethod;
            private static AMethodDecl newArrayMethod;
            private static AMethodDecl deleteObjectMethod;
            private static AMethodDecl deleteArrayMethod;
            private static AMethodDecl simpleResizeArrayMethod;
            private static Dictionary<AStructDecl, AMethodDecl> resizeArrayMethods = new Dictionary<AStructDecl, AMethodDecl>();
            //private static AMethodDecl power2Method;

            internal struct GlobalStructVars
            {
                public AFieldDecl Array;
                public AFieldDecl Used;
                public AFieldDecl Index;
                public AFieldDecl IdentifierArray;
                public AFieldDecl IdentifierNext;

                public GlobalStructVars(AFieldDecl array, AFieldDecl used, AFieldDecl index, AFieldDecl identiferArray, AFieldDecl identifierNext)
                {
                    Array = array;
                    Used = used;
                    Index = index;
                    IdentifierArray = identiferArray;
                    IdentifierNext = identifierNext;
                }
            }
            private static Dictionary<AStructDecl, AMethodDecl> compareNullStructMethod = new Dictionary<AStructDecl, AMethodDecl>();
            private static Dictionary<AStructDecl, AMethodDecl> deleteStructMethod = new Dictionary<AStructDecl, AMethodDecl>();
            private static Dictionary<AStructDecl, AMethodDecl> createStructMethod = new Dictionary<AStructDecl, AMethodDecl>();
            private static Dictionary<AStructDecl, GlobalStructVars> structFields = new Dictionary<AStructDecl, GlobalStructVars>();
            private static Dictionary<AEnrichmentDecl, AMethodDecl> compareNullEnrichmentMethod = new Dictionary<AEnrichmentDecl, AMethodDecl>();
            private static Dictionary<AEnrichmentDecl, AMethodDecl> deleteEnrichmentMethod = new Dictionary<AEnrichmentDecl, AMethodDecl>();
            private static Dictionary<AEnrichmentDecl, AMethodDecl> createEnrichmentMethod = new Dictionary<AEnrichmentDecl, AMethodDecl>();
            private static Dictionary<AEnrichmentDecl, GlobalStructVars> EnrichmentFields = new Dictionary<AEnrichmentDecl, GlobalStructVars>();
            private static AMethodDecl generalCompareNullMethod;
            /*private List<ASimpleInvokeExp> newObjectInvokes = new List<ASimpleInvokeExp>();
            private List<ASimpleInvokeExp> newArrayInvokes = new List<ASimpleInvokeExp>();
            private List<ASimpleInvokeExp> deleteObjectInvokes = new List<ASimpleInvokeExp>();
            private List<ASimpleInvokeExp> deleteArrayInvokes = new List<ASimpleInvokeExp>();
            private Dictionary<AStructDecl, List<ASimpleInvokeExp>> deleteStructInvokes = new Dictionary<AStructDecl, List<ASimpleInvokeExp>>();*/
            private PExp nameExp;
            private bool getData;
            private bool hadPointer;
            private static List<PType> intPointersWithCmp;

            public Phase2(SharedData data, List<PType> IntPointersWithCmp)
            {
                intPointersWithCmp = IntPointersWithCmp;

                this.data = data;
                newObjectMethod = null;
                newArrayMethod = null;
                deleteObjectMethod = null;
                deleteArrayMethod = null;
                //power2Method = null;
                deleteStructMethod.Clear();
                createStructMethod.Clear();
                structFields.Clear();

                simpleResizeArrayMethod = null;
                resizeArrayMethods.Clear();

                compareNullStructMethod.Clear();
                compareNullEnrichmentMethod.Clear();
                generalCompareNullMethod = null;
            }


            
            public static AMethodDecl CreateNullCheckMethod(Node node, PType type, SharedData data)
            {
                if (Util.IsIntPointer(node, type, data))
                {
                    if (type is ANamedType && data.StructTypeLinks.ContainsKey((ANamedType) type))
                    {
                        AStructDecl decl = data.StructTypeLinks[(ANamedType) type];

                        if (compareNullStructMethod.ContainsKey(decl))
                            return compareNullStructMethod[decl];

                        AMethodDecl method = CreateNullCheckMethodP(node, decl.GetIntDim(),
                                                                                decl.GetName().Text,
                                                                                CreateStructFields(node, decl, data),
                                                                                data);
                        compareNullStructMethod[decl] = method;
                        return method;
                    }
                    //It must be an enriched int pointer
                    {
                        AEnrichmentDecl decl = data.EnrichmentTypeLinks[type];
                        if (compareNullEnrichmentMethod.ContainsKey(decl))
                            return compareNullEnrichmentMethod[decl];


                        AMethodDecl method = CreateNullCheckMethodP(node, decl.GetIntDim(),
                                                                               Util.TypeToIdentifierString(type),
                                                                               CreateEnrichmentFields(node, decl, data),
                                                                               data);
                        compareNullEnrichmentMethod[decl] = method;
                        return method;
                    }
                }
                else
                {
                    return CreateGeneralNullCheckMethod(node, data);
                }
            }

            private static AMethodDecl CreateGeneralNullCheckMethod(Node node, SharedData data)
            {
                if (generalCompareNullMethod != null)
                    return generalCompareNullMethod;

                /*
                 *  bool IsNull(string pointer)
                 *  {
                 *      if (pointer == null)
                 *      {
                 *          return true;
                 *      }
                 *      return !DataTableGetBool(true, pointer + "\\Exists");
                 *  }
                 */

                AALocalDecl pointerDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                          new ANamedType(new TIdentifier("string"), null),
                                                          new TIdentifier("pointer"), null);
                
                ALocalLvalue pointerRef1 = new ALocalLvalue(new TIdentifier("pointer"));
                ALocalLvalue pointerRef2 = new ALocalLvalue(new TIdentifier("pointer"));
                ALvalueExp pointerRef1Exp = new ALvalueExp(pointerRef1);
                ALvalueExp pointerRef2Exp = new ALvalueExp(pointerRef2);
                
                ANullExp nullExp = new ANullExp();

                ABooleanConstExp boolConst1 = new ABooleanConstExp(new ATrueBool());
                ABooleanConstExp boolConst2 = new ABooleanConstExp(new ATrueBool());

                AStringConstExp stringConst = new AStringConstExp(new TStringLiteral("\"\\\\Exists\""));

                ABinopExp binop1 = new ABinopExp(pointerRef1Exp, new AEqBinop(new TEq("==")), nullExp);
                ABinopExp binop2 = new ABinopExp(pointerRef2Exp, new APlusBinop(new TPlus("+")), stringConst);

                ASimpleInvokeExp dataTableGetBoolCall = new ASimpleInvokeExp(new TIdentifier("DataTableGetBool"), new ArrayList(){boolConst2, binop2});
                AUnopExp unopExp = new AUnopExp(new AComplementUnop(new TComplement("!")), dataTableGetBoolCall);

                generalCompareNullMethod = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null,
                                                           new ANamedType(new TIdentifier("bool"), null),
                                                           new TIdentifier("IsNull"), new ArrayList() {pointerDecl},
                                                           new AABlock(
                                                               new ArrayList()
                                                                   {
                                                                       new AIfThenStm(new TLParen("("), binop1,
                                                                                      new ABlockStm(new TLBrace("{"),
                                                                                                    new AABlock(
                                                                                                        new ArrayList()
                                                                                                            {
                                                                                                                new AValueReturnStm
                                                                                                                    (new TReturn
                                                                                                                         ("return"),
                                                                                                                     boolConst1)
                                                                                                            },
                                                                                                        new TRBrace("}")))),
                                                                       new AValueReturnStm(new TReturn("return"),
                                                                                           unopExp)
                                                                   }, new TRBrace("}")));

                AASourceFile sourceFile = Util.GetAncestor<AASourceFile>(node);
                sourceFile.GetDecl().Add(generalCompareNullMethod);

                data.LocalLinks[pointerRef1] =
                    data.LocalLinks[pointerRef2] = pointerDecl;

                data.LvalueTypes[pointerRef1] =
                    data.LvalueTypes[pointerRef2] =
                    data.ExpTypes[pointerRef1Exp] =
                    data.ExpTypes[pointerRef2Exp] =
                    data.ExpTypes[stringConst] =
                    data.ExpTypes[binop2] = new ANamedType(new TIdentifier("string"), null);

                data.ExpTypes[binop1] =
                    data.ExpTypes[boolConst1] =
                    data.ExpTypes[boolConst2] =
                    data.ExpTypes[dataTableGetBoolCall] =
                    data.ExpTypes[unopExp] = new ANamedType(new TIdentifier("string"), null);

                foreach (AMethodDecl methodDecl in data.Libraries.Methods)
                {
                    if (methodDecl.GetName().Text == dataTableGetBoolCall.GetName().Text)
                    {
                        data.SimpleMethodLinks[dataTableGetBoolCall] = methodDecl;
                        break;
                    }
                }
                return generalCompareNullMethod;
            }

            private static AMethodDecl CreateNullCheckMethodP(Node node, TIntegerLiteral intLiteral, string prefix, GlobalStructVars vars, SharedData data)
            {
                /*
                 * 
                            <<usedBits := floor(log2(42))+1>>
                            <<bitsLeft := 31 - usedBits>>
                            <<biggestIdentifier := 2^(bitsLeft + 1) - 1>> 
                 * 
                 *  bool prefix_IsNull(int pointer)
                 *  {
                 *      int identifier;
                 *      if (pointer == 0)
                 *      {
                 *          return true;
                 *      }
                 *      identifier = pointer & biggestIdentifier;
                 *      pointer = pointer >> bitsLeft;
                 *      return (Str_used[pointer / 31] & (1 << (pointer % 31))) == 0 || identifierArray[pointer] != identifier;
                 *  }
                 */
                int usedLimit = int.Parse(intLiteral.Text);
                int usedBits = usedLimit == 0 ? 0 : ((int)Math.Floor(Math.Log(usedLimit, 2)) + 1);
                int bitsLeft = 31 - usedBits;
                int biggestIdentifier = (1 << (bitsLeft + 1)) - 1;

                AALocalDecl pointerDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                          new ANamedType(new TIdentifier("int"), null),
                                                          new TIdentifier("pointer"), null);
                AALocalDecl identifierDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                             new ANamedType(new TIdentifier("int"), null),
                                                             new TIdentifier("identifier"), null);

                ALocalLvalue pointerRef1 = new ALocalLvalue(new TIdentifier("pointer"));
                ALocalLvalue pointerRef2 = new ALocalLvalue(new TIdentifier("pointer"));
                ALocalLvalue pointerRef3 = new ALocalLvalue(new TIdentifier("pointer"));
                ALocalLvalue pointerRef4 = new ALocalLvalue(new TIdentifier("pointer"));
                ALocalLvalue pointerRef5 = new ALocalLvalue(new TIdentifier("pointer"));
                ALocalLvalue pointerRef6 = new ALocalLvalue(new TIdentifier("pointer"));
                ALocalLvalue pointerRef7 = new ALocalLvalue(new TIdentifier("pointer"));
                ALvalueExp pointerRef1Exp = new ALvalueExp(pointerRef1);
                ALvalueExp pointerRef2Exp = new ALvalueExp(pointerRef2);
                ALvalueExp pointerRef4Exp = new ALvalueExp(pointerRef4);
                ALvalueExp pointerRef5Exp = new ALvalueExp(pointerRef5);
                ALvalueExp pointerRef6Exp = new ALvalueExp(pointerRef6);
                ALvalueExp pointerRef7Exp = new ALvalueExp(pointerRef7);

                ALocalLvalue identifierRef1 = new ALocalLvalue(new TIdentifier("identifier"));
                ALocalLvalue identifierRef2 = new ALocalLvalue(new TIdentifier("identifier"));
                ALvalueExp identifierRef2Exp = new ALvalueExp(identifierRef2);

                AFieldLvalue usedRef = new AFieldLvalue(new TIdentifier("used"));
                ALvalueExp usedRefExp = new ALvalueExp(usedRef);

                AFieldLvalue identifierArrayRef = new AFieldLvalue(new TIdentifier("identifierArray"));
                ALvalueExp identifierArrayRefExp = new ALvalueExp(identifierArrayRef);

                AIntConstExp intConst1 = new AIntConstExp(new TIntegerLiteral("0"));
                AIntConstExp intConst2 = new AIntConstExp(new TIntegerLiteral(biggestIdentifier.ToString()));
                AIntConstExp intConst3 = new AIntConstExp(new TIntegerLiteral(bitsLeft.ToString()));
                AIntConstExp intConst4 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst5 = new AIntConstExp(new TIntegerLiteral("1"));
                AIntConstExp intConst6 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst7 = new AIntConstExp(new TIntegerLiteral("0"));

                ABinopExp binop4 = new ABinopExp(pointerRef5Exp, new ADivideBinop(new TDiv("/")), intConst4);

                AArrayLvalue arrayLvalue1 = new AArrayLvalue(new TLBracket("["), usedRefExp, binop4);
                AArrayLvalue arrayLvalue2 = new AArrayLvalue(new TLBracket("["), identifierArrayRefExp, pointerRef7Exp);
                ALvalueExp arrayLvalue1Exp = new ALvalueExp(arrayLvalue1);
                ALvalueExp arrayLvalue2Exp = new ALvalueExp(arrayLvalue2);

                ABinopExp binop1 = new ABinopExp(pointerRef1Exp, new AEqBinop(new TEq("==")), intConst1);
                ABinopExp binop2 = new ABinopExp(pointerRef2Exp, new AAndBinop(new TAnd("&")), intConst2);
                ABinopExp binop3 = new ABinopExp(pointerRef4Exp, new ARBitShiftBinop(new TRBitShift(">>")), intConst3);
                ABinopExp binop5 = new ABinopExp(pointerRef6Exp, new AModuloBinop(new TMod("%")), intConst6);
                ABinopExp binop6 = new ABinopExp(intConst5, new ALBitShiftBinop(new TLBitShift("<<")), binop5);
                ABinopExp binop7 = new ABinopExp(arrayLvalue1Exp, new AAndBinop(new TAnd("&")), binop6);
                ABinopExp binop8 = new ABinopExp(binop7, new AEqBinop(new TEq("==")), intConst7);
                ABinopExp binop9 = new ABinopExp(arrayLvalue2Exp, new ANeBinop(new TNeq("!=")), identifierRef2Exp);
                ABinopExp binop10 = new ABinopExp(binop8, new ALazyOrBinop(new TOrOr("||")), binop9);

                AAssignmentExp assignment1 = new AAssignmentExp(new TAssign("="), identifierRef1, binop2);
                AAssignmentExp assignment2 = new AAssignmentExp(new TAssign("="), pointerRef3, binop3);

                ABooleanConstExp boolConst = new ABooleanConstExp(new ATrueBool());

                AMethodDecl method = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null,
                                                     new ANamedType(new TIdentifier("bool"), null),
                                                     new TIdentifier(prefix + "_IsNull"), new ArrayList() {pointerDecl},
                                                     new AABlock(
                                                         new ArrayList()
                                                             {
                                                                 new ALocalDeclStm(new TSemicolon(";"), identifierDecl),
                                                                 new AIfThenStm(new TLParen("("), binop1,
                                                                                new ABlockStm(new TLBrace("{"),
                                                                                              new AABlock(
                                                                                                  new ArrayList()
                                                                                                      {
                                                                                                          new AValueReturnStm
                                                                                                              (new TReturn(
                                                                                                                   "return"),
                                                                                                               boolConst)
                                                                                                      },
                                                                                                  new TRBrace("}")))),
                                                                 new AExpStm(new TSemicolon(";"), assignment1),
                                                                 new AExpStm(new TSemicolon(";"), assignment2),
                                                                 new AValueReturnStm(new TReturn("return"), binop10)
                                                             },
                                                         new TRBrace("}")));

                AASourceFile sourceFile = Util.GetAncestor<AASourceFile>(node);
                sourceFile.GetDecl().Add(method);

                data.LocalLinks[pointerRef1] =
                    data.LocalLinks[pointerRef2] =
                    data.LocalLinks[pointerRef3] =
                    data.LocalLinks[pointerRef4] =
                    data.LocalLinks[pointerRef5] =
                    data.LocalLinks[pointerRef6] =
                    data.LocalLinks[pointerRef7] = pointerDecl;

                data.LocalLinks[identifierRef1] =
                    data.LocalLinks[identifierRef2] = identifierDecl;

                data.FieldLinks[usedRef] = vars.Used;
                data.FieldLinks[identifierArrayRef] = vars.IdentifierArray;

                data.LvalueTypes[pointerRef1] =
                    data.LvalueTypes[pointerRef2] =
                    data.LvalueTypes[pointerRef3] =
                    data.LvalueTypes[pointerRef4] =
                    data.LvalueTypes[pointerRef5] =
                    data.LvalueTypes[pointerRef6] =
                    data.LvalueTypes[pointerRef7] =
                    data.ExpTypes[pointerRef1Exp] =
                    data.ExpTypes[pointerRef2Exp] =
                    data.ExpTypes[pointerRef4Exp] =
                    data.ExpTypes[pointerRef5Exp] =
                    data.ExpTypes[pointerRef6Exp] =
                    data.ExpTypes[pointerRef7Exp] =
                    data.LvalueTypes[identifierRef1] =
                    data.LvalueTypes[identifierRef2] =
                    data.ExpTypes[identifierRef2Exp] =
                    data.ExpTypes[intConst1] =
                    data.ExpTypes[intConst2] =
                    data.ExpTypes[intConst3] =
                    data.ExpTypes[intConst4] =
                    data.ExpTypes[intConst5] =
                    data.ExpTypes[intConst6] =
                    data.ExpTypes[intConst7] =
                    data.ExpTypes[intConst1] =
                    data.ExpTypes[binop2] =
                    data.ExpTypes[binop3] =
                    data.ExpTypes[binop4] =
                    data.ExpTypes[binop5] =
                    data.ExpTypes[binop6] =
                    data.ExpTypes[binop7] =
                    data.LvalueTypes[arrayLvalue1] =
                    data.LvalueTypes[arrayLvalue2] =
                    data.ExpTypes[arrayLvalue1Exp] =
                    data.ExpTypes[arrayLvalue2Exp] =
                    data.ExpTypes[assignment1] =
                    data.ExpTypes[assignment2] = new ANamedType(new TIdentifier("int"), null);

                data.ExpTypes[binop1] =
                    data.ExpTypes[binop8] =
                    data.ExpTypes[binop9] =
                    data.ExpTypes[binop10] =
                    data.ExpTypes[boolConst] = new ANamedType(new TIdentifier("bool"), null);

                data.LvalueTypes[usedRef] =
                    data.LvalueTypes[identifierArrayRef] =
                    data.ExpTypes[usedRefExp] =
                    data.ExpTypes[identifierArrayRefExp] = vars.IdentifierArray.GetType();



                return method;
            }


            public static AMethodDecl CreateNewObjectMethod(Node node, SharedData data)
            {
                if (newObjectMethod != null)
                    return newObjectMethod;
                /*  Insert
                        string CreateNewObject()
                        {
                            //Get next item nr
                            int itemNr = DataTableGetInt(true, "Objects\\Count");
                            itemNr = itemNr + 1;
                            DataTableSetInt(true, "Objects\\Count", itemNr);
    
                            DataTableSetBool(true, "Objects\\" + IntToString(itemNr) + "\\Exists", true);
                            return "Objects\\" + IntToString(itemNr);
                        }
                     */
                AABlock methodBlock = new AABlock();
                AMethodDecl method = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null,
                                                     new ANamedType(new TIdentifier("string"), null),
                                                     new TIdentifier("CreateNewObject"), new ArrayList(),
                                                     methodBlock);

                ASimpleInvokeExp dataTableGetIntInvoke = new ASimpleInvokeExp(new TIdentifier("DataTableGetInt"), new ArrayList());
                ABooleanConstExp trueConst1 = new ABooleanConstExp(new ATrueBool());
                dataTableGetIntInvoke.GetArgs().Add(trueConst1);
                AStringConstExp stringConst1 = new AStringConstExp(new TStringLiteral("\"Objects\\\\Count\""));
                dataTableGetIntInvoke.GetArgs().Add(stringConst1);
                AALocalDecl itemNrDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, new ANamedType(new TIdentifier("int"), null), new TIdentifier("itemNr"), dataTableGetIntInvoke);
                methodBlock.GetStatements().Add(new ALocalDeclStm(new TSemicolon(";"), itemNrDecl));

                ALocalLvalue itemNrLink1 = new ALocalLvalue(new TIdentifier("itemNr"));
                ALocalLvalue itemNrLink2 = new ALocalLvalue(new TIdentifier("itemNr"));
                ALvalueExp itemNrLink2Exp = new ALvalueExp(itemNrLink2);
                AIntConstExp intConst = new AIntConstExp(new TIntegerLiteral("1"));
                ABinopExp binop1 = new ABinopExp(itemNrLink2Exp, new APlusBinop(new TPlus("+")), intConst);
                AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), itemNrLink1, binop1);
                methodBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment));

                ASimpleInvokeExp dataTableSetIntInvoke = new ASimpleInvokeExp(new TIdentifier("DataTableSetInt"), new ArrayList());
                ABooleanConstExp trueConst2 = new ABooleanConstExp(new ATrueBool());
                dataTableSetIntInvoke.GetArgs().Add(trueConst2);
                AStringConstExp stringConst2 = new AStringConstExp(new TStringLiteral("\"Objects\\\\Count\""));
                dataTableSetIntInvoke.GetArgs().Add(stringConst2);
                ALocalLvalue itemNrLink3 = new ALocalLvalue(new TIdentifier("itemNr"));
                ALvalueExp itemNrLink3Exp = new ALvalueExp(itemNrLink3);
                dataTableSetIntInvoke.GetArgs().Add(itemNrLink3Exp);
                methodBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), dataTableSetIntInvoke));

                ASimpleInvokeExp dataTableSetBoolInvoke = new ASimpleInvokeExp(new TIdentifier("DataTableSetBool"), new ArrayList());
                ABooleanConstExp trueConst3 = new ABooleanConstExp(new ATrueBool());
                dataTableSetBoolInvoke.GetArgs().Add(trueConst3);
                AStringConstExp stringConst3 = new AStringConstExp(new TStringLiteral("\"Objects\\\\\""));
                ASimpleInvokeExp intToStringInvoke1 = new ASimpleInvokeExp(new TIdentifier("IntToString"), new ArrayList());
                ALocalLvalue itemNrLink4 = new ALocalLvalue(new TIdentifier("itemNr"));
                ALvalueExp itemNrLink4Exp = new ALvalueExp(itemNrLink4);
                intToStringInvoke1.GetArgs().Add(itemNrLink4Exp);
                ABinopExp binop2 = new ABinopExp(stringConst3, new APlusBinop(new TPlus("+")), intToStringInvoke1);
                AStringConstExp stringConst4 = new AStringConstExp(new TStringLiteral("\"\\\\Exists\""));
                ABinopExp binop3 = new ABinopExp(binop2, new APlusBinop(new TPlus("+")), stringConst4);
                dataTableSetBoolInvoke.GetArgs().Add(binop3);
                ABooleanConstExp trueConst4 = new ABooleanConstExp(new ATrueBool());
                dataTableSetBoolInvoke.GetArgs().Add(trueConst4);
                methodBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), dataTableSetBoolInvoke));

                AStringConstExp stringConst5 = new AStringConstExp(new TStringLiteral("\"Objects\\\\\""));
                ASimpleInvokeExp intToStringInvoke2 = new ASimpleInvokeExp(new TIdentifier("IntToString"), new ArrayList());
                ALocalLvalue itemNrLink5 = new ALocalLvalue(new TIdentifier("itemNr"));
                ALvalueExp itemNrLink5Exp = new ALvalueExp(itemNrLink5);
                intToStringInvoke2.GetArgs().Add(itemNrLink5Exp);
                ABinopExp binop4 = new ABinopExp(stringConst5, new APlusBinop(new TPlus("+")), intToStringInvoke2);
                methodBlock.GetStatements().Add(new AValueReturnStm(new TReturn("return"), binop4));

                AASourceFile sourceFile = Util.GetAncestor<AASourceFile>(node);
                sourceFile.GetDecl().Add(method);


                data.Locals[methodBlock] = new List<AALocalDecl>() { itemNrDecl };
                data.LocalLinks[itemNrLink1] =
                    data.LocalLinks[itemNrLink2] =
                    data.LocalLinks[itemNrLink3] =
                    data.LocalLinks[itemNrLink4] =
                    data.LocalLinks[itemNrLink5] = itemNrDecl;


                data.SimpleMethodLinks[dataTableSetIntInvoke] =
                    data.Libraries.Methods.First(m => m.GetName().Text == dataTableSetIntInvoke.GetName().Text);
                data.SimpleMethodLinks[dataTableGetIntInvoke] =
                    data.Libraries.Methods.First(m => m.GetName().Text == dataTableGetIntInvoke.GetName().Text);
                data.SimpleMethodLinks[dataTableSetBoolInvoke] =
                    data.Libraries.Methods.First(m => m.GetName().Text == dataTableSetBoolInvoke.GetName().Text);
                data.SimpleMethodLinks[intToStringInvoke1] =
                    data.SimpleMethodLinks[intToStringInvoke2] =
                    data.Libraries.Methods.First(m => m.GetName().Text == intToStringInvoke1.GetName().Text);

                data.ExpTypes[stringConst1] =
                    data.ExpTypes[stringConst2] =
                    data.ExpTypes[stringConst3] =
                    data.ExpTypes[stringConst4] =
                    data.ExpTypes[stringConst5] =
                    data.ExpTypes[binop2] =
                    data.ExpTypes[binop3] =
                    data.ExpTypes[binop4] =
                    data.ExpTypes[intToStringInvoke1] =
                    data.ExpTypes[intToStringInvoke2] = new ANamedType(new TIdentifier("string"), null);

                data.ExpTypes[trueConst1] =
                    data.ExpTypes[trueConst2] =
                    data.ExpTypes[trueConst3] =
                    data.ExpTypes[trueConst4] = new ANamedType(new TIdentifier("bool"), null);

                data.LvalueTypes[itemNrLink1] =
                    data.LvalueTypes[itemNrLink2] =
                    data.LvalueTypes[itemNrLink3] =
                    data.LvalueTypes[itemNrLink4] =
                    data.LvalueTypes[itemNrLink5] =
                    data.ExpTypes[itemNrLink2Exp] =
                    data.ExpTypes[itemNrLink3Exp] =
                    data.ExpTypes[itemNrLink4Exp] =
                    data.ExpTypes[itemNrLink5Exp] =
                    data.ExpTypes[binop1] =
                    data.ExpTypes[assignment] =
                    data.ExpTypes[dataTableGetIntInvoke] = new ANamedType(new TIdentifier("int"), null);

                data.ExpTypes[dataTableSetBoolInvoke] =
                    data.ExpTypes[dataTableSetIntInvoke] = new AVoidType(new TVoid("void"));




                newObjectMethod = method;
                return newObjectMethod;
            }

            //+20
            public static GlobalStructVars CreateStructFields(Node node, AStructDecl structDecl, SharedData data)
            {
                if (structFields.ContainsKey(structDecl))
                    return structFields[structDecl];

                ANamedType structType = new ANamedType(new TIdentifier(structDecl.GetName().Text), null);
                data.StructTypeLinks[structType] = structDecl;

                structFields[structDecl] = CreatePointerFields(node, structDecl.GetIntDim(), structDecl.GetName().Text,
                                                               structType, data, structDecl.GetLocals().Count > 0);
                return structFields[structDecl];
                /*
                AASourceFile file = Util.GetAncestor<AASourceFile>(node);

                ANamedType structType = new ANamedType(new TIdentifier(structDecl.GetName().Text), null);
                data.StructTypeLinks[structType] = structDecl;
                AFieldDecl array = new AFieldDecl(new APublicVisibilityModifier(), null, null,
                                                  new AArrayTempType(new TLBracket("["), structType,
                                                                     new AIntConstExp(
                                                                         new TIntegerLiteral(structDecl.GetIntDim().Text)),
                                                                     new TIntegerLiteral(structDecl.GetIntDim().Text)),
                                                                     new TIdentifier(structDecl.GetName().Text + "_array", data.LineCounts[file] + 20, 0), 
                                                                     null);
                int length = (int) Math.Ceiling(float.Parse(structDecl.GetIntDim().Text)/31);
                AFieldDecl used = new AFieldDecl(new APublicVisibilityModifier(), null, null,
                                                  new AArrayTempType(new TLBracket("["), new ANamedType(new TIdentifier("int"), null),
                                                                     new AIntConstExp(
                                                                         new TIntegerLiteral(length.ToString())),
                                                                     new TIntegerLiteral(length.ToString())),
                                                                     new TIdentifier(structDecl.GetName().Text + "_used", data.LineCounts[file] + 20, 0),
                                                                     null);
                AFieldDecl index = new AFieldDecl(new APublicVisibilityModifier(), null, null,  new ANamedType(new TIdentifier("int"), null),
                                                                     new TIdentifier(structDecl.GetName().Text + "_index", data.LineCounts[file] + 20, 0),
                                                                     null);
                file.GetDecl().Add(array);
                file.GetDecl().Add(used);
                file.GetDecl().Add(index);

                data.ExpTypes[((AArrayTempType) array.GetType()).GetDimention()] =
                    data.ExpTypes[((AArrayTempType) used.GetType()).GetDimention()] =
                    new ANamedType(new TIdentifier("int"), null);

                data.Fields.Add(new SharedData.DeclItem<AFieldDecl>(file, array));
                data.Fields.Add(new SharedData.DeclItem<AFieldDecl>(file, used));
                data.Fields.Add(new SharedData.DeclItem<AFieldDecl>(file, index));

                structFields[structDecl] = new GlobalStructVars(array, used, index);
                return structFields[structDecl];*/
            }

            public static GlobalStructVars CreateEnrichmentFields(Node node, AEnrichmentDecl enrichmentDecl, SharedData data)
            {
                if (EnrichmentFields.ContainsKey(enrichmentDecl))
                    return EnrichmentFields[enrichmentDecl];

                EnrichmentFields[enrichmentDecl] = CreatePointerFields(node, enrichmentDecl.GetIntDim(),
                                                                       Util.TypeToIdentifierString(
                                                                           enrichmentDecl.GetType()),
                                                                       Util.MakeClone(enrichmentDecl.GetType(), data), data);
                return EnrichmentFields[enrichmentDecl];
            }

            //Removed check on top, and get type
            public static GlobalStructVars CreatePointerFields(Node node, TIntegerLiteral literal, string prefix, PType type, SharedData data, bool createArray = true)
            {
                AASourceFile file = Util.GetAncestor<AASourceFile>(node);
                
                AFieldDecl array = new AFieldDecl(new APublicVisibilityModifier(), null, null,
                                                  new AArrayTempType(new TLBracket("["), type,
                                                                     new AIntConstExp(
                                                                         new TIntegerLiteral(literal.Text)),
                                                                     new TIntegerLiteral(literal.Text)),
                                                                     new TIdentifier(prefix + "_array", data.LineCounts[file] + 20, 0),
                                                                     null);
                int length = (int)Math.Ceiling(float.Parse(literal.Text) / 31);
                AFieldDecl used = new AFieldDecl(new APublicVisibilityModifier(), null, null,
                                                  new AArrayTempType(new TLBracket("["), new ANamedType(new TIdentifier("int"), null),
                                                                     new AIntConstExp(
                                                                         new TIntegerLiteral(length.ToString())),
                                                                     new TIntegerLiteral(length.ToString())),
                                                                     new TIdentifier(prefix + "_used", data.LineCounts[file] + 20, 0),
                                                                     null);
                AFieldDecl index = new AFieldDecl(new APublicVisibilityModifier(), null, null, new ANamedType(new TIdentifier("int"), null),
                                                                     new TIdentifier(prefix + "_index", data.LineCounts[file] + 20, 0),
                                                                     null);
                bool addIdentiferArray = false;
                foreach (PType t in intPointersWithCmp)
                {
                    if (Util.TypesEqual(type, t, data))
                    {
                        addIdentiferArray = true;
                        break;
                    }
                }
                AFieldDecl identiferCount = null;
                AFieldDecl identiferArray = null;
                if (addIdentiferArray)
                {
                    identiferArray = new AFieldDecl(new APublicVisibilityModifier(), null, null,
                                                 new AArrayTempType(new TLBracket("["), new ANamedType(new TIdentifier("int"), null),
                                                                    new AIntConstExp(
                                                                        new TIntegerLiteral(literal.Text)),
                                                                    new TIntegerLiteral(literal.Text)),
                                                                    new TIdentifier(prefix + "_identifierArray", data.LineCounts[file] + 20, 0),
                                                                    null);
                    identiferCount = new AFieldDecl(new APublicVisibilityModifier(), null, null, new ANamedType(new TIdentifier("int"), null),
                                                                         new TIdentifier(prefix + "_identiferNext", data.LineCounts[file] + 20, 0),
                                                                         new AIntConstExp(new TIntegerLiteral("1")));
                    file.GetDecl().Add(identiferArray);
                    file.GetDecl().Add(identiferCount);

                    data.Fields.Add(new SharedData.DeclItem<AFieldDecl>(file, identiferArray));
                    data.Fields.Add(new SharedData.DeclItem<AFieldDecl>(file, identiferCount));

                    data.ExpTypes[((AArrayTempType)identiferArray.GetType()).GetDimention()] =
                        data.ExpTypes[identiferCount.GetInit()] =
                        new ANamedType(new TIdentifier("int"), null);
                }
                if (createArray)
                    file.GetDecl().Add(array);
                file.GetDecl().Add(used);
                file.GetDecl().Add(index);

                data.ExpTypes[((AArrayTempType)array.GetType()).GetDimention()] =
                    data.ExpTypes[((AArrayTempType)used.GetType()).GetDimention()] =
                    new ANamedType(new TIdentifier("int"), null);

                if (createArray)
                    data.Fields.Add(new SharedData.DeclItem<AFieldDecl>(file, array));
                data.Fields.Add(new SharedData.DeclItem<AFieldDecl>(file, used));
                data.Fields.Add(new SharedData.DeclItem<AFieldDecl>(file, index));

                return new GlobalStructVars(createArray ? array : null, used, index, identiferArray, identiferCount);
            }

            //+19
            /*public static AMethodDecl CreatePower2Method(Node node, SharedData data)
            {
                if (power2Method != null)
                    return power2Method;

                /*  
                    int Power2(int i)
                    {
	                    int ret = 1;
	                    while (i > 0)
	                    {
		                    ret = ret * 2;
		                    i = i - 1;
	                    }
	                    return ret;
                    }
                 *//*
                AASourceFile file = Util.GetAncestor<AASourceFile>(node);

                AIntConstExp intConst1 = new AIntConstExp(new TIntegerLiteral("1"));
                AIntConstExp intConst2 = new AIntConstExp(new TIntegerLiteral("0"));
                AIntConstExp intConst3 = new AIntConstExp(new TIntegerLiteral("2"));
                AIntConstExp intConst4 = new AIntConstExp(new TIntegerLiteral("1"));

                AALocalDecl iDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, new ANamedType(new TIdentifier("int"), null),
                                                    new TIdentifier("i"), null);
                ALocalLvalue iRef1 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef2 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef3 = new ALocalLvalue(new TIdentifier("i"));
                ALvalueExp iRef1Exp = new ALvalueExp(iRef1);
                ALvalueExp iRef3Exp = new ALvalueExp(iRef3);

                AALocalDecl retDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, new ANamedType(new TIdentifier("int"), null),
                                                    new TIdentifier("ret"), intConst1);
                ALocalLvalue retRef1 = new ALocalLvalue(new TIdentifier("ret"));
                ALocalLvalue retRef2 = new ALocalLvalue(new TIdentifier("ret"));
                ALocalLvalue retRef3 = new ALocalLvalue(new TIdentifier("ret"));
                ALvalueExp retRef2Exp = new ALvalueExp(retRef2);
                ALvalueExp retRef3Exp = new ALvalueExp(retRef3);

                ABinopExp binop1 = new ABinopExp(iRef1Exp, new AGtBinop(new TGt(">")), intConst2);
                ABinopExp binop2 = new ABinopExp(retRef2Exp, new ATimesBinop(new TStar("*")), intConst3);
                ABinopExp binop3 = new ABinopExp(iRef3Exp, new AMinusBinop(new TMinus("-")), intConst4);

                AAssignmentExp assignment1 = new AAssignmentExp(new TAssign("="), retRef1, binop2);
                AAssignmentExp assignment2 = new AAssignmentExp(new TAssign("="), iRef2, binop3);



                power2Method = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null,
                                                     new ANamedType(new TIdentifier("int"), null),
                                                     new TIdentifier("Power2", data.LineCounts[file] + 19, 0), 
                                                     new ArrayList() {iDecl},
                                                     new AABlock(
                                                         new ArrayList()
                                                             {
                                                                 new ALocalDeclStm(new TSemicolon(";"), retDecl),
                                                                 new AWhileStm(new TLParen("("), binop1,
                                                                               new ABlockStm(new TLBrace("{"),
                                                                                             new AABlock(
                                                                                                 new ArrayList()
                                                                                                     {
                                                                                                         new AExpStm(
                                                                                                             new TSemicolon
                                                                                                                 (";"),
                                                                                                             assignment1),
                                                                                                         new AExpStm(
                                                                                                             new TSemicolon
                                                                                                                 (";"),
                                                                                                             assignment2)
                                                                                                     },
                                                                                                 new TRBrace("}")))),
                                                                 new AValueReturnStm(new TReturn("return"), retRef3Exp)
                                                             },
                                                         new TRBrace("}")));

                file.GetDecl().Add(power2Method);
                data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(file, power2Method));


                data.LocalLinks[iRef1] =
                    data.LocalLinks[iRef2] =
                    data.LocalLinks[iRef3] = iDecl;
                data.LocalLinks[retRef1] =
                    data.LocalLinks[retRef2] =
                    data.LocalLinks[retRef3] = retDecl;
                data.ExpTypes[intConst1] =
                    data.ExpTypes[intConst2] =
                    data.ExpTypes[intConst3] =
                    data.ExpTypes[intConst4] =
                    data.LvalueTypes[iRef1] =
                    data.LvalueTypes[iRef2] =
                    data.LvalueTypes[iRef3] =
                    data.ExpTypes[iRef1Exp] =
                    data.ExpTypes[iRef3Exp] =
                    data.LvalueTypes[retRef1] =
                    data.LvalueTypes[retRef2] =
                    data.LvalueTypes[retRef3] =
                    data.ExpTypes[retRef2Exp] =
                    data.ExpTypes[retRef3Exp] =
                    data.ExpTypes[binop2] =
                    data.ExpTypes[binop3] =
                    data.ExpTypes[assignment1] =
                    data.ExpTypes[assignment2] = new ANamedType(new TIdentifier("int"), null);
                data.ExpTypes[binop1] = new ANamedType(new TIdentifier("bool"), null);


                return power2Method;
            }*/

            //+18
            public static AMethodDecl CreateNewObjectMethod(Node node, AStructDecl structDecl, SharedData data)
            {
                /*if (structDecl.GetIntDim() == null)
                    return CreateNewObjectMethod(node, data);*/

                if (createStructMethod.ContainsKey(structDecl))
                    return createStructMethod[structDecl];

                createStructMethod[structDecl] = CreateNewObjectMethodP(node, structDecl.GetIntDim(), structDecl.GetName().Text,
                                       CreateStructFields(node, structDecl, data), data);

                return createStructMethod[structDecl];

                #region Comment

                /*
                    int CreateStr()
                    {
                        int i = Str_index;
                        while (Str_used[i / 31] & Power2(i % 31))
                        {
                            i = i + 1;
                            if (i >= 42)
                            {
                                i = 0;
                            }
                            if (i == Str_index)
                            {
                                UIDisplayMessage(PlayerGroupAll(), c_messageAreaDebug, StringToText("Error: Unable to allocate more than 42 dynamic Str types"));
                                IntToString(1/0);
                            }
                        }
                        Str_used[i / 31] = Str_used[i / 31] + Power2(i % 31);
                        Str_index = i;
                        return i;    
                    }
                 */

                /*AASourceFile file = Util.GetAncestor<AASourceFile>(node);
                GlobalStructVars vars = CreateStructFields(node, structDecl, data);
                
                AIntConstExp intConst1 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst2 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst3 = new AIntConstExp(new TIntegerLiteral("1"));
                AIntConstExp intConst4 = new AIntConstExp(new TIntegerLiteral(structDecl.GetIntDim().Text));
                AIntConstExp intConst5 = new AIntConstExp(new TIntegerLiteral("0"));
                AIntConstExp intConst6 = new AIntConstExp(new TIntegerLiteral("1"));
                AIntConstExp intConst7 = new AIntConstExp(new TIntegerLiteral("0"));
                AIntConstExp intConst8 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst9 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst10 = new AIntConstExp(new TIntegerLiteral("31"));

                AFieldLvalue strIndexRef1 = new AFieldLvalue(new TIdentifier(vars.Index.GetName().Text));
                AFieldLvalue strIndexRef2 = new AFieldLvalue(new TIdentifier(vars.Index.GetName().Text));
                AFieldLvalue strIndexRef3 = new AFieldLvalue(new TIdentifier(vars.Index.GetName().Text));
                ALvalueExp strIndexRef1Exp = new ALvalueExp(strIndexRef1);
                ALvalueExp strIndexRef2Exp = new ALvalueExp(strIndexRef2);

                AFieldLvalue strUsedRef1 = new AFieldLvalue(new TIdentifier(vars.Used.GetName().Text));
                AFieldLvalue strUsedRef2 = new AFieldLvalue(new TIdentifier(vars.Used.GetName().Text));
                AFieldLvalue strUsedRef3 = new AFieldLvalue(new TIdentifier(vars.Used.GetName().Text));
                ALvalueExp strUsedRef1Exp = new ALvalueExp(strUsedRef1);
                ALvalueExp strUsedRef2Exp = new ALvalueExp(strUsedRef2);
                ALvalueExp strUsedRef3Exp = new ALvalueExp(strUsedRef3);

                AALocalDecl iDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, new ANamedType(new TIdentifier("int"), null),
                                                    new TIdentifier("i"), strIndexRef1Exp);
                ALocalLvalue iRef1 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef2 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef3 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef4 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef5 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef6 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef7 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef8 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef9 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef10 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef11 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef12 = new ALocalLvalue(new TIdentifier("i"));
                ALvalueExp iRef1Exp = new ALvalueExp(iRef1);
                ALvalueExp iRef2Exp = new ALvalueExp(iRef2);
                ALvalueExp iRef4Exp = new ALvalueExp(iRef4);
                ALvalueExp iRef5Exp = new ALvalueExp(iRef5);
                ALvalueExp iRef7Exp = new ALvalueExp(iRef7);
                ALvalueExp iRef8Exp = new ALvalueExp(iRef8);
                ALvalueExp iRef9Exp = new ALvalueExp(iRef9);
                ALvalueExp iRef10Exp = new ALvalueExp(iRef10);
                ALvalueExp iRef11Exp = new ALvalueExp(iRef11);
                ALvalueExp iRef12Exp = new ALvalueExp(iRef12);

                ABinopExp binop1 = new ABinopExp(iRef1Exp, new ADivideBinop(new TDiv("/")), intConst1);
                ABinopExp binop2 = new ABinopExp(iRef2Exp, new AModuloBinop(new TMod("%")), intConst2);
                ABinopExp binop3 = new ABinopExp(null, new AAndBinop(new TAnd("&")), null);
                ABinopExp binop4 = new ABinopExp(iRef4Exp, new APlusBinop(new TPlus("+")), intConst3);
                ABinopExp binop5 = new ABinopExp(iRef5Exp, new AGeBinop(new TGteq(">=")), intConst4);
                ABinopExp binop6 = new ABinopExp(iRef7Exp, new AEqBinop(new TEq("==")), strIndexRef2Exp);
                ABinopExp binop7 = new ABinopExp(intConst6, new ADivideBinop(new TDiv("/")), intConst7);
                ABinopExp binop8 = new ABinopExp(iRef8Exp, new ADivideBinop(new TDiv("/")), intConst8);
                ABinopExp binop9 = new ABinopExp(iRef9Exp, new ADivideBinop(new TDiv("/")), intConst9);
                ABinopExp binop10 = new ABinopExp(iRef10Exp, new AModuloBinop(new TMod("%")), intConst10);
                ABinopExp binop11 = new ABinopExp(null, new APlusBinop(new TPlus("+")), null);

                ASimpleInvokeExp power2Invoke1 = new ASimpleInvokeExp(new TIdentifier("Power2"), new ArrayList() { binop2 });
                ASimpleInvokeExp power2Invoke2 = new ASimpleInvokeExp(new TIdentifier("Power2"), new ArrayList() { binop10 });
                binop3.SetRight(power2Invoke1);
                binop11.SetRight(power2Invoke2);

                AArrayLvalue arrayIndex1 = new AArrayLvalue(new TLBracket("["), strUsedRef1Exp, binop1);
                AArrayLvalue arrayIndex2 = new AArrayLvalue(new TLBracket("["), strUsedRef2Exp, binop8);
                AArrayLvalue arrayIndex3 = new AArrayLvalue(new TLBracket("["), strUsedRef3Exp, binop9);
                ALvalueExp arrayIndex1Exp = new ALvalueExp(arrayIndex1);
                ALvalueExp arrayIndex3Exp = new ALvalueExp(arrayIndex3);
                binop3.SetLeft(arrayIndex1Exp);
                binop11.SetLeft(arrayIndex3Exp);

                AAssignmentExp assignement1 = new AAssignmentExp(new TAssign("="), iRef3, binop4);
                AAssignmentExp assignement2 = new AAssignmentExp(new TAssign("="), iRef6, intConst5);
                AAssignmentExp assignement3 = new AAssignmentExp(new TAssign("="), arrayIndex2, binop11);
                AAssignmentExp assignement4 = new AAssignmentExp(new TAssign("="), strIndexRef3, iRef11Exp);

                ASimpleInvokeExp playerGroupAllInvoke = new ASimpleInvokeExp(new TIdentifier("PlayerGroupAll"), new ArrayList());
                AFieldLvalue messageAreaDebugRef = new AFieldLvalue(new TIdentifier("c_messageAreaDebug"));
                ALvalueExp messageAreaDebugRefExp = new ALvalueExp(messageAreaDebugRef);
                AStringConstExp stringConst =
                    new AStringConstExp(
                        new TStringLiteral("\"Galaxy++ Error: Unable to allocate more than " + structDecl.GetIntDim().Text +
                                           " dynamic " + structDecl.GetName().Text + " types.\""));
                ASimpleInvokeExp stringToTextInvoke = new ASimpleInvokeExp(new TIdentifier("StringToText"), new ArrayList(){stringConst});
                ASimpleInvokeExp displayMessageInvoke = new ASimpleInvokeExp(new TIdentifier("UIDisplayMessage"),
                                                                             new ArrayList()
                                                                                 {
                                                                                     playerGroupAllInvoke,
                                                                                     messageAreaDebugRefExp,
                                                                                     stringToTextInvoke
                                                                                 });
                ASimpleInvokeExp intToStringInvoke = new ASimpleInvokeExp(new TIdentifier("IntToString"),
                                                                          new ArrayList() {binop7});


                createStructMethod[structDecl] = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null,
                                                                 new ANamedType(new TIdentifier("int"), null),
                                                                 new TIdentifier("Create" + structDecl.GetName().Text, data.LineCounts[file] + 18, 0),
                                                                 new ArrayList(),
                                                                 new AABlock(
                                                                     new ArrayList()
                                                                         {
                                                                             new ALocalDeclStm(new TSemicolon(";"), iDecl),
                                                                             new AWhileStm(new TLParen("("), binop3,
                                                                                           new ABlockStm(new TLBrace("{"),
                                                                                                         new AABlock(
                                                                                                             new ArrayList()
                                                                                                                 {
                                                                                                                     new AExpStm
                                                                                                                         (new TSemicolon
                                                                                                                              (";"),
                                                                                                                          assignement1),
                                                                                                                     new AIfThenStm
                                                                                                                         (new TLParen
                                                                                                                              ("("),
                                                                                                                          binop5,
                                                                                                                          new ABlockStm
                                                                                                                              (new TLBrace
                                                                                                                                   ("{"),
                                                                                                                               new AABlock
                                                                                                                                   (new ArrayList
                                                                                                                                        ()
                                                                                                                                        {
                                                                                                                                            new AExpStm
                                                                                                                                                (new TSemicolon
                                                                                                                                                     (";"),
                                                                                                                                                 assignement2)
                                                                                                                                        },
                                                                                                                                    new TRBrace
                                                                                                                                        ("}")))),
                                                                                                                     new AIfThenStm
                                                                                                                         (new TLParen
                                                                                                                              ("("),
                                                                                                                          binop6,
                                                                                                                          new ABlockStm
                                                                                                                              (new TLBrace
                                                                                                                                   ("{"),
                                                                                                                               new AABlock
                                                                                                                                   (new ArrayList
                                                                                                                                        ()
                                                                                                                                        {
                                                                                                                                            new AExpStm
                                                                                                                                                (new TSemicolon
                                                                                                                                                     (";"),
                                                                                                                                                 displayMessageInvoke),
                                                                                                                                            new AExpStm
                                                                                                                                                (new TSemicolon
                                                                                                                                                     (";"),
                                                                                                                                                 intToStringInvoke)
                                                                                                                                        },
                                                                                                                                    new TRBrace
                                                                                                                                        ("}"))))
                                                                                                                 },
                                                                                                             new TRBrace(
                                                                                                                 "}")))),
                                                                             new AExpStm(new TSemicolon(";"), assignement3),
                                                                             new AExpStm(new TSemicolon(";"), assignement4),
                                                                             new AValueReturnStm(new TReturn("return"),
                                                                                                 iRef12Exp)
                                                                         },
                                                                     new TRBrace("}")));//66 lines! YEAH! you can see what that statement did

                file.GetDecl().Add(createStructMethod[structDecl]);

                data.LocalLinks[iRef1] =
                    data.LocalLinks[iRef2] =
                    data.LocalLinks[iRef3] =
                    data.LocalLinks[iRef4] =
                    data.LocalLinks[iRef5] =
                    data.LocalLinks[iRef6] =
                    data.LocalLinks[iRef7] =
                    data.LocalLinks[iRef8] =
                    data.LocalLinks[iRef9] =
                    data.LocalLinks[iRef10] =
                    data.LocalLinks[iRef11] =
                    data.LocalLinks[iRef12] = iDecl;
                data.FieldLinks[strUsedRef1] =
                    data.FieldLinks[strUsedRef2] =
                    data.FieldLinks[strUsedRef3] = vars.Used;
                data.FieldLinks[strIndexRef1] =
                    data.FieldLinks[strIndexRef2] =
                    data.FieldLinks[strIndexRef3] = vars.Index;
                data.SimpleMethodLinks[power2Invoke1] =
                    data.SimpleMethodLinks[power2Invoke2] = CreatePower2Method(node, data);
                data.FieldLinks[messageAreaDebugRef] =
                    data.Libraries.Fields.First(f => f.GetName().Text == messageAreaDebugRef.GetName().Text);
                data.SimpleMethodLinks[displayMessageInvoke] =
                    data.Libraries.Methods.First(m => m.GetName().Text == displayMessageInvoke.GetName().Text);
                data.SimpleMethodLinks[playerGroupAllInvoke] =
                    data.Libraries.Methods.First(m => m.GetName().Text == playerGroupAllInvoke.GetName().Text);
                data.SimpleMethodLinks[stringToTextInvoke] =
                    data.Libraries.Methods.First(m => m.GetName().Text == stringToTextInvoke.GetName().Text);
                data.SimpleMethodLinks[intToStringInvoke] =
                    data.Libraries.Methods.First(m => m.GetName().Text == intToStringInvoke.GetName().Text);

                data.ExpTypes[intConst1] =
                    data.ExpTypes[intConst2] =
                    data.ExpTypes[intConst3] =
                    data.ExpTypes[intConst4] =
                    data.ExpTypes[intConst5] =
                    data.ExpTypes[intConst6] =
                    data.ExpTypes[intConst7] =
                    data.ExpTypes[intConst8] =
                    data.ExpTypes[intConst9] =
                    data.ExpTypes[intConst10] =
                    data.LvalueTypes[strIndexRef1] =
                    data.LvalueTypes[strIndexRef2] =
                    data.LvalueTypes[strIndexRef3] =
                    data.ExpTypes[strIndexRef1Exp] =
                    data.ExpTypes[strIndexRef2Exp] =
                    data.LvalueTypes[iRef1] =
                    data.LvalueTypes[iRef2] =
                    data.LvalueTypes[iRef3] =
                    data.LvalueTypes[iRef4] =
                    data.LvalueTypes[iRef5] =
                    data.LvalueTypes[iRef6] =
                    data.LvalueTypes[iRef7] =
                    data.LvalueTypes[iRef8] =
                    data.LvalueTypes[iRef9] =
                    data.LvalueTypes[iRef10] =
                    data.LvalueTypes[iRef11] =
                    data.LvalueTypes[iRef12] =
                    data.ExpTypes[iRef1Exp] =
                    data.ExpTypes[iRef2Exp] =
                    data.ExpTypes[iRef4Exp] =
                    data.ExpTypes[iRef5Exp] =
                    data.ExpTypes[iRef7Exp] =
                    data.ExpTypes[iRef8Exp] =
                    data.ExpTypes[iRef9Exp] =
                    data.ExpTypes[iRef10Exp] =
                    data.ExpTypes[iRef11Exp] =
                    data.ExpTypes[iRef12Exp] =
                    data.LvalueTypes[arrayIndex1] =
                    data.LvalueTypes[arrayIndex2] =
                    data.LvalueTypes[arrayIndex3] =
                    data.ExpTypes[arrayIndex1Exp] =
                    data.ExpTypes[arrayIndex3Exp] =
                    data.ExpTypes[binop1] =
                    data.ExpTypes[binop2] =
                    data.ExpTypes[binop3] =
                    data.ExpTypes[binop4] =
                    data.ExpTypes[binop7] =
                    data.ExpTypes[binop8] =
                    data.ExpTypes[binop9] =
                    data.ExpTypes[binop10] =
                    data.ExpTypes[binop11] =
                    data.ExpTypes[power2Invoke1] =
                    data.ExpTypes[power2Invoke2] =
                    data.ExpTypes[intToStringInvoke] =
                    data.ExpTypes[assignement1] =
                    data.ExpTypes[assignement2] =
                    data.ExpTypes[assignement3] =
                    data.ExpTypes[assignement4] = 
                    data.LvalueTypes[messageAreaDebugRef] = 
                    data.ExpTypes[messageAreaDebugRefExp] = new ANamedType(new TIdentifier("int"), null);

                data.LvalueTypes[strUsedRef1] =
                    data.LvalueTypes[strUsedRef2] =
                    data.LvalueTypes[strUsedRef3] =
                    data.ExpTypes[strUsedRef1Exp] =
                    data.ExpTypes[strUsedRef2Exp] =
                    data.ExpTypes[strUsedRef3Exp] = vars.Used.GetType();

                data.ExpTypes[binop5] =
                    data.ExpTypes[binop6] = new ANamedType(new TIdentifier("int"), null);

                data.ExpTypes[stringConst] =
                    data.ExpTypes[intToStringInvoke] = new ANamedType(new TIdentifier("string"), null);

                data.ExpTypes[stringToTextInvoke] = new ANamedType(new TIdentifier("text"), null);

                data.ExpTypes[playerGroupAllInvoke] = new ANamedType(new TIdentifier("playergroup"), null);

                data.ExpTypes[displayMessageInvoke] = new AVoidType(new TVoid("void"));

                return createStructMethod[structDecl];*/

                #endregion
            }

            public static AMethodDecl CreateNewObjectMethod(Node node, AEnrichmentDecl enrichmentDecl, SharedData data)
            {

                if (createEnrichmentMethod.ContainsKey(enrichmentDecl))
                    return createEnrichmentMethod[enrichmentDecl];

                createEnrichmentMethod[enrichmentDecl] = CreateNewObjectMethodP(node, enrichmentDecl.GetIntDim(),
                                                                        Util.TypeToIdentifierString(enrichmentDecl.GetType()),
                                                                        CreateEnrichmentFields(node, enrichmentDecl, data), data);

                return createEnrichmentMethod[enrichmentDecl];
            }

            private static AMethodDecl CreateNewObjectMethodP(Node node, TIntegerLiteral intLiteral, string prefix, GlobalStructVars vars, SharedData data)
            {
                if (intLiteral == null)
                    return CreateNewObjectMethod(node, data);

                //if (createStructMethod.ContainsKey(structDecl))
                //    return createStructMethod[structDecl];

                /*
                    int CreateStr()
                    {
                        int i = Str_index;
                        while (Str_used[i / 31] & 1 << (i % 31))
                        {
                            i = i + 1;
                            if (i >= 42)
                            {
                                i = 0;
                            }
                            if (i == Str_index)
                            {
                                UIDisplayMessage(PlayerGroupAll(), c_messageAreaDebug, StringToText("Error: Unable to allocate more than 42 dynamic Str types"));
                                IntToString(1/0);
                            }
                        }
                        Str_used[i / 31] = Str_used[i / 31] + Power2(i % 31);
                        Str_index = i;
                        <<if it is being compared with null at any point in time>>
                            <<usedBits := floor(log2(42))+1>>
                            <<bitsLeft := 31 - usedBits>>
                            <<biggestIdentifier := 2^(bitsLeft + 1) - 1>> 
                            identifierArray[i] = identifierNext;
                            i = (i << bitsLeft) + identifierNext;
                            identifierNext = identifierNext%biggestIdentifier + 1;
                        return i;    
                    }
                 */
                

                AASourceFile file = Util.GetAncestor<AASourceFile>(node);

                AIntConstExp intConst1 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst2 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst3 = new AIntConstExp(new TIntegerLiteral("1"));
                AIntConstExp intConst4 = new AIntConstExp(new TIntegerLiteral(intLiteral.Text));
                AIntConstExp intConst5 = new AIntConstExp(new TIntegerLiteral("0"));
                AIntConstExp intConst6 = new AIntConstExp(new TIntegerLiteral("1"));
                AIntConstExp intConst7 = new AIntConstExp(new TIntegerLiteral("0"));
                AIntConstExp intConst8 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst9 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst10 = new AIntConstExp(new TIntegerLiteral("31"));

                AIntConstExp intConst11 = new AIntConstExp(new TIntegerLiteral("1"));
                AIntConstExp intConst12 = new AIntConstExp(new TIntegerLiteral("1"));

                AFieldLvalue strIndexRef1 = new AFieldLvalue(new TIdentifier(vars.Index.GetName().Text));
                AFieldLvalue strIndexRef2 = new AFieldLvalue(new TIdentifier(vars.Index.GetName().Text));
                AFieldLvalue strIndexRef3 = new AFieldLvalue(new TIdentifier(vars.Index.GetName().Text));
                ALvalueExp strIndexRef1Exp = new ALvalueExp(strIndexRef1);
                ALvalueExp strIndexRef2Exp = new ALvalueExp(strIndexRef2);

                AFieldLvalue strUsedRef1 = new AFieldLvalue(new TIdentifier(vars.Used.GetName().Text));
                AFieldLvalue strUsedRef2 = new AFieldLvalue(new TIdentifier(vars.Used.GetName().Text));
                AFieldLvalue strUsedRef3 = new AFieldLvalue(new TIdentifier(vars.Used.GetName().Text));
                ALvalueExp strUsedRef1Exp = new ALvalueExp(strUsedRef1);
                ALvalueExp strUsedRef2Exp = new ALvalueExp(strUsedRef2);
                ALvalueExp strUsedRef3Exp = new ALvalueExp(strUsedRef3);

                AALocalDecl iDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, new ANamedType(new TIdentifier("int"), null),
                                                    new TIdentifier("i"), strIndexRef1Exp);
                ALocalLvalue iRef1 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef2 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef3 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef4 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef5 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef6 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef7 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef8 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef9 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef10 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef11 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef12 = new ALocalLvalue(new TIdentifier("i"));
                ALvalueExp iRef1Exp = new ALvalueExp(iRef1);
                ALvalueExp iRef2Exp = new ALvalueExp(iRef2);
                ALvalueExp iRef4Exp = new ALvalueExp(iRef4);
                ALvalueExp iRef5Exp = new ALvalueExp(iRef5);
                ALvalueExp iRef7Exp = new ALvalueExp(iRef7);
                ALvalueExp iRef8Exp = new ALvalueExp(iRef8);
                ALvalueExp iRef9Exp = new ALvalueExp(iRef9);
                ALvalueExp iRef10Exp = new ALvalueExp(iRef10);
                ALvalueExp iRef11Exp = new ALvalueExp(iRef11);
                ALvalueExp iRef12Exp = new ALvalueExp(iRef12);

                ABinopExp binop1 = new ABinopExp(iRef1Exp, new ADivideBinop(new TDiv("/")), intConst1);
                ABinopExp binop2 = new ABinopExp(iRef2Exp, new AModuloBinop(new TMod("%")), intConst2);
                ABinopExp binop3 = new ABinopExp(null, new AAndBinop(new TAnd("&")), null);
                ABinopExp binop4 = new ABinopExp(iRef4Exp, new APlusBinop(new TPlus("+")), intConst3);
                ABinopExp binop5 = new ABinopExp(iRef5Exp, new AGeBinop(new TGteq(">=")), intConst4);
                ABinopExp binop6 = new ABinopExp(iRef7Exp, new AEqBinop(new TEq("==")), strIndexRef2Exp);
                ABinopExp binop7 = new ABinopExp(intConst6, new ADivideBinop(new TDiv("/")), intConst7);
                ABinopExp binop8 = new ABinopExp(iRef8Exp, new ADivideBinop(new TDiv("/")), intConst8);
                ABinopExp binop9 = new ABinopExp(iRef9Exp, new ADivideBinop(new TDiv("/")), intConst9);
                ABinopExp binop10 = new ABinopExp(iRef10Exp, new AModuloBinop(new TMod("%")), intConst10);
                ABinopExp binop11 = new ABinopExp(null, new APlusBinop(new TPlus("+")), null);

                ABinopExp binop12 = new ABinopExp(intConst11, new ALBitShiftBinop(new TLBitShift("<<")), binop2);
                ABinopExp binop13 = new ABinopExp(intConst12, new ALBitShiftBinop(new TLBitShift("<<")), binop10);
                binop3.SetRight(binop12);
                binop11.SetRight(binop13);

                //ASimpleInvokeExp power2Invoke1 = new ASimpleInvokeExp(new TIdentifier("Power2"), new ArrayList() { binop2 });
                //ASimpleInvokeExp power2Invoke2 = new ASimpleInvokeExp(new TIdentifier("Power2"), new ArrayList() { binop10 });
                //binop3.SetRight(power2Invoke1);
                //binop11.SetRight(power2Invoke2);

                AArrayLvalue arrayIndex1 = new AArrayLvalue(new TLBracket("["), strUsedRef1Exp, binop1);
                AArrayLvalue arrayIndex2 = new AArrayLvalue(new TLBracket("["), strUsedRef2Exp, binop8);
                AArrayLvalue arrayIndex3 = new AArrayLvalue(new TLBracket("["), strUsedRef3Exp, binop9);
                ALvalueExp arrayIndex1Exp = new ALvalueExp(arrayIndex1);
                ALvalueExp arrayIndex3Exp = new ALvalueExp(arrayIndex3);
                binop3.SetLeft(arrayIndex1Exp);
                binop11.SetLeft(arrayIndex3Exp);

                AAssignmentExp assignement1 = new AAssignmentExp(new TAssign("="), iRef3, binop4);
                AAssignmentExp assignement2 = new AAssignmentExp(new TAssign("="), iRef6, intConst5);
                AAssignmentExp assignement3 = new AAssignmentExp(new TAssign("="), arrayIndex2, binop11);
                AAssignmentExp assignement4 = new AAssignmentExp(new TAssign("="), strIndexRef3, iRef11Exp);

                ASimpleInvokeExp playerGroupAllInvoke = new ASimpleInvokeExp(new TIdentifier("PlayerGroupAll"), new ArrayList());
                AFieldLvalue messageAreaDebugRef = new AFieldLvalue(new TIdentifier("c_messageAreaDebug"));
                ALvalueExp messageAreaDebugRefExp = new ALvalueExp(messageAreaDebugRef);
                AStringConstExp stringConst =
                    new AStringConstExp(
                        new TStringLiteral("\"Galaxy++ Error: Unable to allocate more than " + intLiteral.Text +
                                           " dynamic " + prefix + " types.\""));
                ASimpleInvokeExp stringToTextInvoke = new ASimpleInvokeExp(new TIdentifier("StringToText"), new ArrayList() { stringConst });
                ASimpleInvokeExp displayMessageInvoke = new ASimpleInvokeExp(new TIdentifier("UIDisplayMessage"),
                                                                             new ArrayList()
                                                                                 {
                                                                                     playerGroupAllInvoke,
                                                                                     messageAreaDebugRefExp,
                                                                                     stringToTextInvoke
                                                                                 });
                ASimpleInvokeExp intToStringInvoke = new ASimpleInvokeExp(new TIdentifier("IntToString"),
                                                                          new ArrayList() { binop7 });

                AABlock methodBlock = new AABlock(
                    new ArrayList()
                        {
                            new ALocalDeclStm(new TSemicolon(";"), iDecl),
                            new AWhileStm(new TLParen("("), binop3,
                                          new ABlockStm(new TLBrace("{"),
                                                        new AABlock(
                                                            new ArrayList()
                                                                {
                                                                    new AExpStm
                                                                        (new TSemicolon
                                                                             (";"),
                                                                         assignement1),
                                                                    new AIfThenStm
                                                                        (new TLParen
                                                                             ("("),
                                                                         binop5,
                                                                         new ABlockStm
                                                                             (new TLBrace
                                                                                  ("{"),
                                                                              new AABlock
                                                                                  (new ArrayList
                                                                                       ()
                                                                                       {
                                                                                           new AExpStm
                                                                                               (new TSemicolon
                                                                                                    (";"),
                                                                                                assignement2)
                                                                                       },
                                                                                   new TRBrace
                                                                                       ("}")))),
                                                                    new AIfThenStm
                                                                        (new TLParen
                                                                             ("("),
                                                                         binop6,
                                                                         new ABlockStm
                                                                             (new TLBrace
                                                                                  ("{"),
                                                                              new AABlock
                                                                                  (new ArrayList
                                                                                       ()
                                                                                       {
                                                                                           new AExpStm
                                                                                               (new TSemicolon
                                                                                                    (";"),
                                                                                                displayMessageInvoke),
                                                                                           new AExpStm
                                                                                               (new TSemicolon
                                                                                                    (";"),
                                                                                                intToStringInvoke)
                                                                                       },
                                                                                   new TRBrace
                                                                                       ("}"))))
                                                                },
                                                            new TRBrace(
                                                                "}")))),
                            new AExpStm(new TSemicolon(";"), assignement3),
                            new AExpStm(new TSemicolon(";"), assignement4)
                        },
                    new TRBrace("}"));

                if (vars.IdentifierArray != null)
                {
                    /*
                        <<if it is being compared with null at any point in time>>
                            <<usedBits := floor(log2(42))+1>>
                            <<bitsLeft := 31 - usedBits>>
                            <<biggestIdentifier := 2^(bitsLeft + 1) - 1>> 
                            identifierArray[i] = identifierNext;
                            i = (i << bitsLeft) + identifierNext;
                            identifierNext = identifierNext%biggestIdentifier + 1;
                    */
                    int usedLimit = int.Parse(intLiteral.Text);
                    int usedBits = usedLimit == 0 ? 0 : ((int)Math.Floor(Math.Log(usedLimit, 2)) + 1);
                    int bitsLeft = 31 - usedBits;
                    int biggestIdentifier = (1 << (bitsLeft + 1)) - 1;



                    AIntConstExp bitsLeftConst = new AIntConstExp(new TIntegerLiteral(bitsLeft.ToString()));
                    AIntConstExp biggestIdentifierConst = new AIntConstExp(new TIntegerLiteral(biggestIdentifier.ToString()));
                    AIntConstExp oneIntConst = new AIntConstExp(new TIntegerLiteral("1"));
                    ALocalLvalue secondIRef1 = new ALocalLvalue(new TIdentifier("i"));
                    ALocalLvalue secondIRef2 = new ALocalLvalue(new TIdentifier("i"));
                    ALocalLvalue secondIRef3 = new ALocalLvalue(new TIdentifier("i"));
                    ALvalueExp secondIRef2Exp = new ALvalueExp(secondIRef2);
                    ALvalueExp secondIRef3Exp = new ALvalueExp(secondIRef3);
                    AFieldLvalue identierNExtRef1 = new AFieldLvalue(new TIdentifier("identiferNext"));
                    AFieldLvalue identierNExtRef2 = new AFieldLvalue(new TIdentifier("identiferNext"));
                    AFieldLvalue identierNExtRef3 = new AFieldLvalue(new TIdentifier("identiferNext"));
                    AFieldLvalue identierNExtRef4 = new AFieldLvalue(new TIdentifier("identiferNext"));
                    ALvalueExp identierNExtRef1Exp = new ALvalueExp(identierNExtRef1);
                    ALvalueExp identierNExtRef3Exp = new ALvalueExp(identierNExtRef3);
                    ALvalueExp identierNExtRef4Exp = new ALvalueExp(identierNExtRef4);
                    AFieldLvalue identifierArrayRef = new AFieldLvalue(new TIdentifier("identifierArray"));
                    ALvalueExp identifierArrayRefExp = new ALvalueExp(identifierArrayRef);

                    AArrayLvalue arrayLvalue = new AArrayLvalue(new TLBracket("["), identifierArrayRefExp, secondIRef3Exp);

                    AAssignmentExp secondAssignment3 = new AAssignmentExp(new TAssign("="), arrayLvalue, identierNExtRef4Exp);

                    methodBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), secondAssignment3));


                    ABinopExp secondBinop1 = new ABinopExp(secondIRef2Exp, new ALBitShiftBinop(new TLBitShift("<<")), bitsLeftConst);
                    ABinopExp secondBinop2 = new ABinopExp(secondBinop1, new APlusBinop(new TPlus("+")), identierNExtRef1Exp);

                    AAssignmentExp secondAssignment1 = new AAssignmentExp(new TAssign("="), secondIRef1, secondBinop2);

                    methodBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), secondAssignment1));

                    ABinopExp secondBinop3 = new ABinopExp(identierNExtRef3Exp, new AModuloBinop(new TMod("%")), biggestIdentifierConst);
                    ABinopExp secondBinop4 = new ABinopExp(secondBinop3, new APlusBinop(new TPlus("+")), oneIntConst);

                    AAssignmentExp secondAssignment2 = new AAssignmentExp(new TAssign("="), identierNExtRef2, secondBinop4);

                    methodBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), secondAssignment2));

                    data.LvalueTypes[secondIRef1] =
                        data.LvalueTypes[secondIRef2] =
                        data.ExpTypes[bitsLeftConst] =
                        data.ExpTypes[biggestIdentifierConst] =
                        data.ExpTypes[oneIntConst] =
                        data.ExpTypes[secondIRef2Exp] =
                        data.LvalueTypes[identierNExtRef1] =
                        data.LvalueTypes[identierNExtRef2] =
                        data.LvalueTypes[identierNExtRef3] =
                        data.ExpTypes[identierNExtRef1Exp] =
                        data.ExpTypes[identierNExtRef3Exp] =
                        data.ExpTypes[secondBinop1] =
                        data.ExpTypes[secondBinop2] =
                        data.ExpTypes[secondAssignment1] =
                        data.ExpTypes[secondBinop3] =
                        data.ExpTypes[secondBinop4] =
                        data.ExpTypes[secondAssignment2] =
                        data.LvalueTypes[secondIRef3] =
                        data.LvalueTypes[identierNExtRef4] =
                        data.ExpTypes[secondIRef3Exp] =
                        data.ExpTypes[identierNExtRef4Exp] =
                        data.LvalueTypes[arrayLvalue] =
                        data.ExpTypes[secondAssignment3] = new ANamedType(new TIdentifier("int"), null);

                    data.LvalueTypes[identifierArrayRef] =
                        data.ExpTypes[identifierArrayRefExp] = vars.IdentifierArray.GetType();

                    data.LocalLinks[secondIRef1] =
                        data.LocalLinks[secondIRef2] =
                        data.LocalLinks[secondIRef3] = iDecl;

                    data.FieldLinks[identierNExtRef1] =
                        data.FieldLinks[identierNExtRef2] =
                        data.FieldLinks[identierNExtRef3] =
                        data.FieldLinks[identierNExtRef4] = vars.IdentifierNext;

                    data.FieldLinks[identifierArrayRef] = vars.IdentifierArray;
                }

                methodBlock.GetStatements().Add(new AValueReturnStm(new TReturn("return"), iRef12Exp));

                AMethodDecl method = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null,
                                                                 new ANamedType(new TIdentifier("int"), null),
                                                                 new TIdentifier("Create" + prefix, data.LineCounts[file] + 18, 0),
                                                                 new ArrayList(),
                                                                 methodBlock);

                file.GetDecl().Add(method);
                data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(file, method));

                data.LocalLinks[iRef1] =
                    data.LocalLinks[iRef2] =
                    data.LocalLinks[iRef3] =
                    data.LocalLinks[iRef4] =
                    data.LocalLinks[iRef5] =
                    data.LocalLinks[iRef6] =
                    data.LocalLinks[iRef7] =
                    data.LocalLinks[iRef8] =
                    data.LocalLinks[iRef9] =
                    data.LocalLinks[iRef10] =
                    data.LocalLinks[iRef11] =
                    data.LocalLinks[iRef12] = iDecl;
                data.FieldLinks[strUsedRef1] =
                    data.FieldLinks[strUsedRef2] =
                    data.FieldLinks[strUsedRef3] = vars.Used;
                data.FieldLinks[strIndexRef1] =
                    data.FieldLinks[strIndexRef2] =
                    data.FieldLinks[strIndexRef3] = vars.Index;
                //data.SimpleMethodLinks[power2Invoke1] =
                //    data.SimpleMethodLinks[power2Invoke2] = CreatePower2Method(node, data);
                data.FieldLinks[messageAreaDebugRef] =
                    data.Libraries.Fields.First(f => f.GetName().Text == messageAreaDebugRef.GetName().Text);
                data.SimpleMethodLinks[displayMessageInvoke] =
                    data.Libraries.Methods.First(m => m.GetName().Text == displayMessageInvoke.GetName().Text);
                data.SimpleMethodLinks[playerGroupAllInvoke] =
                    data.Libraries.Methods.First(m => m.GetName().Text == playerGroupAllInvoke.GetName().Text);
                data.SimpleMethodLinks[stringToTextInvoke] =
                    data.Libraries.Methods.First(m => m.GetName().Text == stringToTextInvoke.GetName().Text);
                data.SimpleMethodLinks[intToStringInvoke] =
                    data.Libraries.Methods.First(m => m.GetName().Text == intToStringInvoke.GetName().Text);

                data.ExpTypes[intConst1] =
                    data.ExpTypes[intConst2] =
                    data.ExpTypes[intConst3] =
                    data.ExpTypes[intConst4] =
                    data.ExpTypes[intConst5] =
                    data.ExpTypes[intConst6] =
                    data.ExpTypes[intConst7] =
                    data.ExpTypes[intConst8] =
                    data.ExpTypes[intConst9] =
                    data.ExpTypes[intConst10] =
                    data.ExpTypes[intConst11] =
                    data.ExpTypes[intConst12] =
                    data.LvalueTypes[strIndexRef1] =
                    data.LvalueTypes[strIndexRef2] =
                    data.LvalueTypes[strIndexRef3] =
                    data.ExpTypes[strIndexRef1Exp] =
                    data.ExpTypes[strIndexRef2Exp] =
                    data.LvalueTypes[iRef1] =
                    data.LvalueTypes[iRef2] =
                    data.LvalueTypes[iRef3] =
                    data.LvalueTypes[iRef4] =
                    data.LvalueTypes[iRef5] =
                    data.LvalueTypes[iRef6] =
                    data.LvalueTypes[iRef7] =
                    data.LvalueTypes[iRef8] =
                    data.LvalueTypes[iRef9] =
                    data.LvalueTypes[iRef10] =
                    data.LvalueTypes[iRef11] =
                    data.LvalueTypes[iRef12] =
                    data.ExpTypes[iRef1Exp] =
                    data.ExpTypes[iRef2Exp] =
                    data.ExpTypes[iRef4Exp] =
                    data.ExpTypes[iRef5Exp] =
                    data.ExpTypes[iRef7Exp] =
                    data.ExpTypes[iRef8Exp] =
                    data.ExpTypes[iRef9Exp] =
                    data.ExpTypes[iRef10Exp] =
                    data.ExpTypes[iRef11Exp] =
                    data.ExpTypes[iRef12Exp] =
                    data.LvalueTypes[arrayIndex1] =
                    data.LvalueTypes[arrayIndex2] =
                    data.LvalueTypes[arrayIndex3] =
                    data.ExpTypes[arrayIndex1Exp] =
                    data.ExpTypes[arrayIndex3Exp] =
                    data.ExpTypes[binop1] =
                    data.ExpTypes[binop2] =
                    data.ExpTypes[binop3] =
                    data.ExpTypes[binop4] =
                    data.ExpTypes[binop7] =
                    data.ExpTypes[binop8] =
                    data.ExpTypes[binop9] =
                    data.ExpTypes[binop10] =
                    data.ExpTypes[binop11] =
                    data.ExpTypes[binop12] =
                    data.ExpTypes[binop13] =
                    //data.ExpTypes[power2Invoke1] =
                    //data.ExpTypes[power2Invoke2] =
                    data.ExpTypes[intToStringInvoke] =
                    data.ExpTypes[assignement1] =
                    data.ExpTypes[assignement2] =
                    data.ExpTypes[assignement3] =
                    data.ExpTypes[assignement4] =
                    data.LvalueTypes[messageAreaDebugRef] =
                    data.ExpTypes[messageAreaDebugRefExp] = new ANamedType(new TIdentifier("int"), null);

                data.LvalueTypes[strUsedRef1] =
                    data.LvalueTypes[strUsedRef2] =
                    data.LvalueTypes[strUsedRef3] =
                    data.ExpTypes[strUsedRef1Exp] =
                    data.ExpTypes[strUsedRef2Exp] =
                    data.ExpTypes[strUsedRef3Exp] = vars.Used.GetType();

                data.ExpTypes[binop5] =
                    data.ExpTypes[binop6] = new ANamedType(new TIdentifier("int"), null);

                data.ExpTypes[stringConst] =
                    data.ExpTypes[intToStringInvoke] = new ANamedType(new TIdentifier("string"), null);

                data.ExpTypes[stringToTextInvoke] = new ANamedType(new TIdentifier("text"), null);

                data.ExpTypes[playerGroupAllInvoke] = new ANamedType(new TIdentifier("playergroup"), null);

                data.ExpTypes[displayMessageInvoke] = new AVoidType(new TVoid("void"));

                return method;
            }



            public static AMethodDecl CreateNewArrayMethod(Node node, SharedData data)
            {
                if (newArrayMethod != null)
                    return newArrayMethod;
                if (newObjectMethod == null)
                    CreateNewObjectMethod(node, data);
                /* Insert
                     *  string CreateNewArray(int length)
                        {
                            string id = CreateNewObject();
    
                            DataTableSetInt(true, id + "\\Length", length);
                            return id;
                        }
                     */
                AALocalDecl lengthDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, new ANamedType(new TIdentifier("int"), null), new TIdentifier("length"), null);
                AABlock methodBlock = new AABlock();
                AMethodDecl method = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null,
                                                     new ANamedType(new TIdentifier("string"), null),
                                                     new TIdentifier("CreateNewArray"), new ArrayList() { lengthDecl },
                                                     methodBlock);

                ASimpleInvokeExp createNewObjectInvoke = new ASimpleInvokeExp(new TIdentifier("CreateNewObject"), new ArrayList());
                AALocalDecl idDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, new ANamedType(new TIdentifier("string"), null), new TIdentifier("id"), createNewObjectInvoke);
                methodBlock.GetStatements().Add(new ALocalDeclStm(new TSemicolon(";"), idDecl));

                ASimpleInvokeExp dataTableSetIntInvoke = new ASimpleInvokeExp(new TIdentifier("DataTableSetInt"), new ArrayList());
                ABooleanConstExp trueConst = new ABooleanConstExp(new ATrueBool());
                dataTableSetIntInvoke.GetArgs().Add(trueConst);
                ALocalLvalue idLink1 = new ALocalLvalue(new TIdentifier("id"));
                ALvalueExp idLink1Exp = new ALvalueExp(idLink1);
                AStringConstExp lengthText = new AStringConstExp(new TStringLiteral("\"\\\\Length\""));
                ABinopExp binopExp = new ABinopExp(idLink1Exp, new APlusBinop(new TPlus("+")), lengthText);
                dataTableSetIntInvoke.GetArgs().Add(binopExp);
                ALocalLvalue lenghtLink = new ALocalLvalue(new TIdentifier("length"));
                ALvalueExp lengthLinkExp = new ALvalueExp(lenghtLink);
                dataTableSetIntInvoke.GetArgs().Add(lengthLinkExp);
                methodBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), dataTableSetIntInvoke));

                ALocalLvalue idLink2 = new ALocalLvalue(new TIdentifier("id"));
                ALvalueExp idLink2Exp = new ALvalueExp(idLink2);
                methodBlock.GetStatements().Add(new AValueReturnStm(new TReturn("return"), idLink2Exp));

                AASourceFile sourceFile = Util.GetAncestor<AASourceFile>(node);
                sourceFile.GetDecl().Add(method);
                data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(sourceFile, method));


                data.Locals[methodBlock] = new List<AALocalDecl> { lengthDecl, idDecl };
                data.LocalLinks[lenghtLink] = lengthDecl;
                data.LocalLinks[idLink1] = idDecl;
                data.LocalLinks[idLink2] = idDecl;

                data.SimpleMethodLinks[dataTableSetIntInvoke] =
                    data.Libraries.Methods.First(m => m.GetName().Text == dataTableSetIntInvoke.GetName().Text);

                data.ExpTypes[createNewObjectInvoke] =
                    data.ExpTypes[lengthText] =
                    data.LvalueTypes[idLink1] =
                    data.LvalueTypes[idLink2] =
                    data.ExpTypes[idLink1Exp] =
                    data.ExpTypes[idLink2Exp] =
                    data.ExpTypes[binopExp] = new ANamedType(new TIdentifier("string"), null);

                data.ExpTypes[trueConst] = new ANamedType(new TIdentifier("bool"), null);

                data.LvalueTypes[lenghtLink] = data.ExpTypes[lengthLinkExp] = new ANamedType(new TIdentifier("int"), null);

                data.ExpTypes[dataTableSetIntInvoke] = new AVoidType(new TVoid("void"));

                data.SimpleMethodLinks[createNewObjectInvoke] = newObjectMethod;

                newArrayMethod = method;
                return newArrayMethod;
            }

            public static AMethodDecl CreateDeleteObjectMethod(Node node, SharedData data)
            {
                if (deleteObjectMethod != null)
                    return deleteObjectMethod;
                /*Insert
                 *  void DeleteObject(string id)
                    {
                        DataTableValueRemove(true, id + "\\Exists");
                        DataTableValueRemove(true, id);
                    }
                 */

                AALocalDecl idDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, new ANamedType(new TIdentifier("string"), null), new TIdentifier("id"), null);
                AABlock block = new AABlock();
                AMethodDecl method = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null, new AVoidType(new TVoid("void")),
                                                     new TIdentifier("DeleteObject"), new ArrayList() {idDecl}, block);

                ABooleanConstExp trueConst1 = new ABooleanConstExp(new ATrueBool());
                ALocalLvalue idLink1 = new ALocalLvalue(new TIdentifier("id"));
                ALvalueExp idLink1Exp = new ALvalueExp(idLink1);
                AStringConstExp stringConst = new AStringConstExp(new TStringLiteral("\"\\\\Exists\""));
                ABinopExp binop = new ABinopExp(idLink1Exp, new APlusBinop(new TPlus("+")), stringConst);
                ASimpleInvokeExp dataTableValueRemove1 = new ASimpleInvokeExp(new TIdentifier("DataTableValueRemove"), new ArrayList() { trueConst1, binop });
                block.GetStatements().Add(new AExpStm(new TSemicolon(";"), dataTableValueRemove1));

                ABooleanConstExp trueConst2 = new ABooleanConstExp(new ATrueBool());
                ALocalLvalue idLink2 = new ALocalLvalue(new TIdentifier("id"));
                ALvalueExp idLink2Exp = new ALvalueExp(idLink2);
                ASimpleInvokeExp dataTableValueRemove2 = new ASimpleInvokeExp(new TIdentifier("DataTableValueRemove"), new ArrayList() { trueConst2, idLink2Exp });
                block.GetStatements().Add(new AExpStm(new TSemicolon(";"), dataTableValueRemove2));

                AASourceFile sourceFile = Util.GetAncestor<AASourceFile>(node);
                sourceFile.GetDecl().Add(method);
                data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(sourceFile, method));


                data.Locals[block] = new List<AALocalDecl> { idDecl };
                data.LocalLinks[idLink1] = idDecl;
                data.LocalLinks[idLink2] = idDecl;

                data.SimpleMethodLinks[dataTableValueRemove1] =
                data.SimpleMethodLinks[dataTableValueRemove2] =
                    data.Libraries.Methods.First(m => m.GetName().Text == dataTableValueRemove1.GetName().Text);

                data.ExpTypes[stringConst] =
                    data.LvalueTypes[idLink1] =
                    data.LvalueTypes[idLink2] =
                    data.ExpTypes[idLink1Exp] =
                    data.ExpTypes[idLink2Exp] =
                    data.ExpTypes[binop] = new ANamedType(new TIdentifier("string"), null);

                data.ExpTypes[trueConst1] =
                    data.ExpTypes[trueConst2] = new ANamedType(new TIdentifier("bool"), null);


                data.ExpTypes[dataTableValueRemove1] =
                    data.ExpTypes[dataTableValueRemove2] = new AVoidType(new TVoid("void"));


                deleteObjectMethod = method;
                return deleteObjectMethod;
            }

            public static AMethodDecl CreateResizeArrayMethod(Node node, PType baseType, SharedData data)
            {
                //Search already created stuff, and return that if present.
                //All non struct baseTypes get the simple version

                ANamedType aBaseType = null;
                AStructDecl structDecl = null;
                if (Util.IsBulkCopy(baseType))
                {
                    aBaseType = (ANamedType)baseType;
                    structDecl = data.StructTypeLinks[aBaseType];

                    if (resizeArrayMethods.ContainsKey(structDecl))
                        return resizeArrayMethods[structDecl];
                }
                else
                    if (simpleResizeArrayMethod != null)
                        return simpleResizeArrayMethod;

                //Create
                /*
                    void Resize(int newSize, string id)
                    {
                        int oldSize = DataTableGetInt(true, id + "\\Length");
                        while (oldSize > newSize)
                        {
                            //Delete everything up to now
                            oldSize = oldSize - 1;
                            DeleteStruct<name>(id + "[" + IntToString(oldSize) + "]");//If struct type
                            DataTableValueRemove(true, id + "[" + IntToString(oldSize) + "]");
                        }
                        DataTableSetInt(true, id + "\\Length", newSize);
                    }
                 */
                AALocalDecl newSizeFormal = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                            new ANamedType(new TIdentifier("int"), null),
                                                            new TIdentifier("newSize"), null);
                AALocalDecl idFormal = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                            new ANamedType(new TIdentifier("string"), null),
                                                            new TIdentifier("id"), null);

                AABlock methodBlock = new AABlock(new ArrayList(), new TRBrace("}"));

                //int oldSize = DataTableGetInt(true, id + "\\Length");
                ABooleanConstExp boolConstExp1 = new ABooleanConstExp(new ATrueBool());
                AStringConstExp stringConstExp1 = new AStringConstExp(new TStringLiteral("\"\\\\Length\""));
                ALocalLvalue idRef1 = new ALocalLvalue(new TIdentifier("id"));
                ALvalueExp idRef1Exp = new ALvalueExp(idRef1);
                ABinopExp binop1 = new ABinopExp(idRef1Exp, new APlusBinop(new TPlus("+")), stringConstExp1);
                ASimpleInvokeExp getSizeInvoke = new ASimpleInvokeExp(new TIdentifier("DataTableGetInt"), new ArrayList(){boolConstExp1, binop1});
                AALocalDecl oldSizeDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                            new ANamedType(new TIdentifier("int"), null),
                                                            new TIdentifier("oldSize"), getSizeInvoke);
                methodBlock.GetStatements().Add(new ALocalDeclStm(new TSemicolon(";"), oldSizeDecl));

                //while (oldSize > newSize)
                ALocalLvalue oldSizeRef1 = new ALocalLvalue(new TIdentifier("oldSize"));
                ALvalueExp oldSizeRef1Exp = new ALvalueExp(oldSizeRef1);
                ALocalLvalue newSizeRef1 = new ALocalLvalue(new TIdentifier("newSize"));
                ALvalueExp newSizeRef1Exp = new ALvalueExp(newSizeRef1);
                ABinopExp binop2 = new ABinopExp(oldSizeRef1Exp, new AGtBinop(new TGt(">")), newSizeRef1Exp);
                AABlock whileBlock = new AABlock(new ArrayList(), new TRBrace("}"));
                methodBlock.GetStatements().Add(new AWhileStm(new TLParen("("), binop2,
                                                               new ABlockStm(new TLBrace("{"), whileBlock)));

                //oldSize = oldSize - 1;
                ALocalLvalue oldSizeRef2 = new ALocalLvalue(new TIdentifier("oldSize"));
                ALocalLvalue oldSizeRef3 = new ALocalLvalue(new TIdentifier("oldSize"));
                ALvalueExp oldSizeRef3Exp = new ALvalueExp(oldSizeRef3);
                AIntConstExp intConst1 = new AIntConstExp(new TIntegerLiteral("1"));
                ABinopExp binop3 = new ABinopExp(oldSizeRef3Exp, new AMinusBinop(new TMinus("-")), intConst1);
                AAssignmentExp assignment1 = new AAssignmentExp(new TAssign("="), oldSizeRef2, binop3);
                whileBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment1));

                //DeleteStruct<name>(id + "[" + IntToString(oldSize) + "]");//If struct type
                ALocalLvalue idRef2 = null;
                ALvalueExp idRef2Exp = null;
                AStringConstExp stringConstExp2 = null;
                AStringConstExp stringConstExp3 = null;
                ALocalLvalue oldSizeRef4 = null;
                ALvalueExp oldSizeRef4Exp = null;
                ASimpleInvokeExp intToString1 = null;
                ABinopExp binop4 = null;
                ABinopExp binop5 = null;
                ABinopExp binop6 = null;
                ASimpleInvokeExp deleteStructInvoke = null;
                if (aBaseType != null)
                {
                    idRef2 = new ALocalLvalue(new TIdentifier("id"));
                    idRef2Exp = new ALvalueExp(idRef2);
                    stringConstExp2 = new AStringConstExp(new TStringLiteral("\"[\""));
                    stringConstExp3 = new AStringConstExp(new TStringLiteral("\"]\""));
                    oldSizeRef4 = new ALocalLvalue(new TIdentifier("oldSize"));
                    oldSizeRef4Exp = new ALvalueExp(oldSizeRef4);
                    intToString1 = new ASimpleInvokeExp(new TIdentifier("IntToString"), new ArrayList() { oldSizeRef4Exp });
                    binop4 = new ABinopExp(intToString1, new APlusBinop(new TPlus("+")), stringConstExp3);
                    binop5 = new ABinopExp(stringConstExp2, new APlusBinop(new TPlus("+")), binop4);
                    binop6 = new ABinopExp(idRef2Exp, new APlusBinop(new TPlus("+")), binop5);
                    deleteStructInvoke =
                        new ASimpleInvokeExp(new TIdentifier("DeleteStruct" + aBaseType.AsIdentifierString()),
                                             new ArrayList() { binop6 });
                    whileBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), deleteStructInvoke));
                }

                //DataTableValueRemove(true, id + "[" + IntToString(oldSize) + "]");
                ABooleanConstExp boolConstExp2 = new ABooleanConstExp(new ATrueBool());
                ALocalLvalue idRef3 = new ALocalLvalue(new TIdentifier("id"));
                ALvalueExp idRef3Exp = new ALvalueExp(idRef3);
                AStringConstExp stringConstExp4 = new AStringConstExp(new TStringLiteral("\"[\""));
                AStringConstExp stringConstExp5 = new AStringConstExp(new TStringLiteral("\"]\""));
                ALocalLvalue oldSizeRef5 = new ALocalLvalue(new TIdentifier("oldSize"));
                ALvalueExp oldSizeRef5Exp = new ALvalueExp(oldSizeRef5);
                ASimpleInvokeExp intToString2 = new ASimpleInvokeExp(new TIdentifier("IntToString"), new ArrayList(){oldSizeRef5Exp});
                ABinopExp binop7 = new ABinopExp(intToString2, new APlusBinop(new TPlus("+")), stringConstExp5);
                ABinopExp binop8 = new ABinopExp(stringConstExp4, new APlusBinop(new TPlus("+")), binop7);
                ABinopExp binop9 = new ABinopExp(idRef3Exp, new APlusBinop(new TPlus("+")), binop8);
                ASimpleInvokeExp dataTableRemove = new ASimpleInvokeExp(new TIdentifier("DataTableValueRemove"),
                                                                        new ArrayList() {boolConstExp2, binop9});
                whileBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), dataTableRemove));

                //DataTableSetInt(true, id + "\\Length", newSize);
                ABooleanConstExp boolConstExp3 = new ABooleanConstExp(new ATrueBool());
                AStringConstExp stringConstExp6 = new AStringConstExp(new TStringLiteral("\"\\\\Length\""));
                ALocalLvalue idRef4 = new ALocalLvalue(new TIdentifier("id"));
                ALvalueExp idRef4Exp = new ALvalueExp(idRef4);
                ABinopExp binop10 = new ABinopExp(idRef4Exp, new APlusBinop(new TPlus("+")), stringConstExp6);
                ALocalLvalue newSizeRef2 = new ALocalLvalue(new TIdentifier("newSize"));
                ALvalueExp newSizeRef2Exp = new ALvalueExp(newSizeRef2);
                ASimpleInvokeExp setSizeInvoke = new ASimpleInvokeExp(new TIdentifier("DataTableSetInt"),
                                                                      new ArrayList()
                                                                          {boolConstExp3, binop10, newSizeRef2Exp});
                methodBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), setSizeInvoke));

                AMethodDecl method = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null,
                                                         new AVoidType(new TVoid("void")),
                                                         new TIdentifier("ResizeArray"),
                                                         new ArrayList() { newSizeFormal, idFormal }, methodBlock);
                AASourceFile sourceFile = Util.GetAncestor<AASourceFile>(node);
                sourceFile.GetDecl().Add(method);
                data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(sourceFile, method));

                data.LvalueTypes[oldSizeRef1] =
                    data.LvalueTypes[oldSizeRef2] =
                    data.LvalueTypes[oldSizeRef3] =
                    //data.LvalueTypes[oldSizeRef4] =
                    data.LvalueTypes[oldSizeRef5] =
                    data.ExpTypes[oldSizeRef1Exp] =
                    data.ExpTypes[oldSizeRef3Exp] =
                    //data.ExpTypes[oldSizeRef4Exp] =
                    data.ExpTypes[oldSizeRef5Exp] =
                    data.LvalueTypes[newSizeRef1] =
                    data.LvalueTypes[newSizeRef2] =
                    data.ExpTypes[newSizeRef1Exp] =
                    data.ExpTypes[newSizeRef2Exp] =
                    data.ExpTypes[binop3] =
                    data.ExpTypes[assignment1] =
                    data.ExpTypes[getSizeInvoke] = new ANamedType(new TIdentifier("int"), null);
                data.LvalueTypes[idRef1] =
                    //data.LvalueTypes[idRef2] =
                    data.LvalueTypes[idRef3] =
                    data.LvalueTypes[idRef4] =
                    data.ExpTypes[idRef1Exp] =
                    //data.ExpTypes[idRef2Exp] =
                    data.ExpTypes[idRef3Exp] =
                    data.ExpTypes[idRef4Exp] =
                    data.ExpTypes[stringConstExp1] =
                    //data.ExpTypes[stringConstExp2] =
                    //data.ExpTypes[stringConstExp3] =
                    data.ExpTypes[stringConstExp4] =
                    data.ExpTypes[stringConstExp5] =
                    data.ExpTypes[stringConstExp6] =
                    data.ExpTypes[binop1] =
                    //data.ExpTypes[binop4] =
                    //data.ExpTypes[binop5] =
                    //data.ExpTypes[binop6] =
                    data.ExpTypes[binop7] =
                    data.ExpTypes[binop8] =
                    data.ExpTypes[binop9] =
                    data.ExpTypes[binop10] =
                    //data.ExpTypes[intToString1] =
                    data.ExpTypes[intToString2] = new ANamedType(new TIdentifier("string"), null);
                data.ExpTypes[dataTableRemove] =
                    //data.ExpTypes[deleteStructInvoke] =
                    data.ExpTypes[setSizeInvoke] = new AVoidType(new TVoid("void"));
                data.ExpTypes[boolConstExp1] =
                    data.ExpTypes[boolConstExp2] =
                    data.ExpTypes[boolConstExp3] =
                    data.ExpTypes[binop2] = new ANamedType(new TIdentifier("bool"), null);

                data.LocalLinks[oldSizeRef1] =
                    data.LocalLinks[oldSizeRef2] =
                    data.LocalLinks[oldSizeRef3] =
                    data.LocalLinks[oldSizeRef5] = oldSizeDecl;
                data.LocalLinks[newSizeRef1] =
                    data.LocalLinks[newSizeRef2] = newSizeFormal;
                data.LocalLinks[idRef1] =
                    data.LocalLinks[idRef3] =
                    data.LocalLinks[idRef4] = idFormal;

                data.SimpleMethodLinks[getSizeInvoke] =
                    data.Libraries.Methods.First(m => m.GetName().Text == getSizeInvoke.GetName().Text);
                data.SimpleMethodLinks[setSizeInvoke] =
                    data.Libraries.Methods.First(m => m.GetName().Text == setSizeInvoke.GetName().Text);
                data.SimpleMethodLinks[dataTableRemove] =
                    data.Libraries.Methods.First(m => m.GetName().Text == dataTableRemove.GetName().Text);
                data.SimpleMethodLinks[intToString2] =
                    data.Libraries.Methods.First(m => m.GetName().Text == intToString2.GetName().Text);

                if (aBaseType == null)
                    return simpleResizeArrayMethod = method;


                data.LvalueTypes[oldSizeRef4] =
                    data.ExpTypes[oldSizeRef4Exp] = new ANamedType(new TIdentifier("int"), null);
                data.LvalueTypes[idRef2] =
                    data.ExpTypes[idRef2Exp] =
                    data.ExpTypes[stringConstExp2] =
                    data.ExpTypes[stringConstExp3] =
                    data.ExpTypes[binop4] =
                    data.ExpTypes[binop5] =
                    data.ExpTypes[binop6] =
                    data.ExpTypes[intToString1] = new ANamedType(new TIdentifier("string"), null);
                data.ExpTypes[deleteStructInvoke] = new AVoidType(new TVoid("void"));

                data.LocalLinks[oldSizeRef4] = oldSizeDecl;
                data.LocalLinks[idRef2] = idFormal;

                data.SimpleMethodLinks[deleteStructInvoke] = CreateDeleteStructMethod(node, structDecl, data);
                data.SimpleMethodLinks[intToString1] = data.SimpleMethodLinks[intToString2];
                method.SetName(new TIdentifier("Resize" + aBaseType.AsIdentifierString() + "Array"));
                resizeArrayMethods.Add(structDecl, method);
                return method;
            }

            public static AMethodDecl CreateDeleteArrayMethod(Node node, SharedData data)
            {
                if (deleteArrayMethod != null)
                    return deleteArrayMethod;
                /*  void DeleteArray(string id)
                    {
                        int length = DataTableGetInt(true, id + "\\Length");
                        while (length > 0)
                        {
                            length = length - 1;
                            DataTableValueRemove(true, id + "[" + IntToString(length) + "]");
                        }
                        DataTableValueRemove(true, id + "\\Length");
                        DataTableValueRemove(true, id + "\\Exists");
                    }
                 */

                AALocalDecl idDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, new ANamedType(new TIdentifier("string"), null), new TIdentifier("id"), null);
                AABlock block = new AABlock();
                AMethodDecl method = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null, new AVoidType(new TVoid("void")),
                                                     new TIdentifier("DeleteArray"), new ArrayList() { idDecl }, block);

                ALocalLvalue idLink1 = new ALocalLvalue(new TIdentifier("id"));
                ALvalueExp idLink1Exp = new ALvalueExp(idLink1);
                ABinopExp binopExp1 = new ABinopExp(idLink1Exp, new APlusBinop(new TPlus("+")), new AStringConstExp(new TStringLiteral("\"\\\\Length\"")));
                ABooleanConstExp trueConst1 = new ABooleanConstExp(new ATrueBool());
                ASimpleInvokeExp dataTableGetIntInvoke = new ASimpleInvokeExp(new TIdentifier("DataTableGetInt"), new ArrayList{trueConst1, binopExp1});
                AALocalDecl lengthDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, new ANamedType(new TIdentifier("int"), null), new TIdentifier("length"), dataTableGetIntInvoke);
                block.GetStatements().Add(new ALocalDeclStm(new TSemicolon(";"), lengthDecl));

                ALocalLvalue lengthLink1 = new ALocalLvalue(new TIdentifier("length"));
                ALvalueExp lengthLink1Exp = new ALvalueExp(lengthLink1);
                ABinopExp binopExp2 = new ABinopExp(lengthLink1Exp, new AGtBinop(new TGt(">")), new AIntConstExp(new TIntegerLiteral("0")));
                AABlock whileBlock = new AABlock();
                block.GetStatements().Add(new AWhileStm(new TLParen("("), binopExp2,
                                                        new ABlockStm(new TLBrace("{"), whileBlock)));

                ALocalLvalue lengthLink2 = new ALocalLvalue(new TIdentifier("length"));
                ALvalueExp lengthLink2Exp = new ALvalueExp(lengthLink2);
                ALocalLvalue lengthLink3 = new ALocalLvalue(new TIdentifier("length"));
                ABinopExp binopExp3 = new ABinopExp(lengthLink2Exp, new AMinusBinop(new TMinus("-")), new AIntConstExp(new TIntegerLiteral("1")));
                AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), lengthLink3, binopExp3);
                whileBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), assignment));

                ABooleanConstExp trueConst2 = new ABooleanConstExp(new ATrueBool());
                ALocalLvalue idLink2 = new ALocalLvalue(new TIdentifier("id"));
                ALvalueExp idLink2Exp = new ALvalueExp(idLink2);
                ALocalLvalue lengthLink4 = new ALocalLvalue(new TIdentifier("length"));
                ALvalueExp lengthLink4Exp = new ALvalueExp(lengthLink4);
                ASimpleInvokeExp intToStringInvoke = new ASimpleInvokeExp(new TIdentifier("IntToString"), new ArrayList(){lengthLink4Exp});
                ABinopExp binopExp4 = new ABinopExp(idLink2Exp, new APlusBinop(new TPlus("+")), new AStringConstExp(new TStringLiteral("\"[\"")));
                ABinopExp binopExp5 = new ABinopExp(binopExp4, new APlusBinop(new TPlus("+")), intToStringInvoke);
                ABinopExp binopExp6 = new ABinopExp(binopExp5, new APlusBinop(new TPlus("+")), new AStringConstExp(new TStringLiteral("\"]\"")));
                ASimpleInvokeExp dataTableRemoveInvoke1 = new ASimpleInvokeExp(new TIdentifier("DataTableValueRemove"), new ArrayList(){trueConst2, binopExp6});
                whileBlock.GetStatements().Add(new AExpStm(new TSemicolon(";"), dataTableRemoveInvoke1));

                ABooleanConstExp trueConst3 = new ABooleanConstExp(new ATrueBool());
                ALocalLvalue idLink3 = new ALocalLvalue(new TIdentifier("id"));
                ALvalueExp idLink3Exp = new ALvalueExp(idLink3);
                ABinopExp binopExp7 = new ABinopExp(idLink3Exp, new APlusBinop(new TPlus("+")), new AStringConstExp(new TStringLiteral("\"\\\\Length\"")));
                ASimpleInvokeExp dataTableRemoveInvoke2 = new ASimpleInvokeExp(new TIdentifier("DataTableValueRemove"), new ArrayList() { trueConst3, binopExp7 });
                block.GetStatements().Add(new AExpStm(new TSemicolon(";"), dataTableRemoveInvoke2));

                ABooleanConstExp trueConst4 = new ABooleanConstExp(new ATrueBool());
                ALocalLvalue idLink4 = new ALocalLvalue(new TIdentifier("id"));
                ALvalueExp idLink4Exp = new ALvalueExp(idLink4);
                ABinopExp binopExp8 = new ABinopExp(idLink4Exp, new APlusBinop(new TPlus("+")), new AStringConstExp(new TStringLiteral("\"\\\\Exists\"")));
                ASimpleInvokeExp dataTableRemoveInvoke3 = new ASimpleInvokeExp(new TIdentifier("DataTableValueRemove"), new ArrayList() { trueConst4, binopExp8 });
                block.GetStatements().Add(new AExpStm(new TSemicolon(";"), dataTableRemoveInvoke3));

                AASourceFile sourceFile = Util.GetAncestor<AASourceFile>(node);
                sourceFile.GetDecl().Add(method);
                data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(sourceFile, method));


                data.Locals[block] = new List<AALocalDecl> { idDecl, lengthDecl };
                data.LocalLinks[idLink1] =
                    data.LocalLinks[idLink2] =
                    data.LocalLinks[idLink3] =
                    data.LocalLinks[idLink4] = idDecl;
                data.LocalLinks[lengthLink1] =
                    data.LocalLinks[lengthLink2] =
                    data.LocalLinks[lengthLink3] =
                    data.LocalLinks[lengthLink4] = lengthDecl;


                data.SimpleMethodLinks[dataTableRemoveInvoke1] =
                data.SimpleMethodLinks[dataTableRemoveInvoke2] =
                data.SimpleMethodLinks[dataTableRemoveInvoke3] =
                    data.Libraries.Methods.First(m => m.GetName().Text == dataTableRemoveInvoke1.GetName().Text);
                data.SimpleMethodLinks[dataTableGetIntInvoke] =
                    data.Libraries.Methods.First(m => m.GetName().Text == dataTableGetIntInvoke.GetName().Text);
                data.SimpleMethodLinks[intToStringInvoke] =
                    data.Libraries.Methods.First(m => m.GetName().Text == intToStringInvoke.GetName().Text);

                data.LvalueTypes[idLink1] =
                    data.LvalueTypes[idLink2] =
                    data.LvalueTypes[idLink3] =
                    data.LvalueTypes[idLink4] =
                    data.ExpTypes[idLink1Exp] =
                    data.ExpTypes[idLink2Exp] =
                    data.ExpTypes[idLink3Exp] =
                    data.ExpTypes[idLink4Exp] =
                    data.ExpTypes[binopExp1] =
                    data.ExpTypes[binopExp4] =
                    data.ExpTypes[binopExp5] =
                    data.ExpTypes[binopExp6] =
                    data.ExpTypes[binopExp7] =
                    data.ExpTypes[binopExp8] =
                    data.ExpTypes[binopExp1.GetRight()] =
                    data.ExpTypes[binopExp4.GetRight()] =
                    data.ExpTypes[binopExp6.GetRight()] =
                    data.ExpTypes[binopExp7.GetRight()] =
                    data.ExpTypes[binopExp8.GetRight()] =
                    data.ExpTypes[intToStringInvoke] = new ANamedType(new TIdentifier("string"), null);



                data.ExpTypes[trueConst1] =
                    data.ExpTypes[trueConst2] =
                    data.ExpTypes[trueConst3] =
                    data.ExpTypes[trueConst4] =
                    data.ExpTypes[binopExp2] = new ANamedType(new TIdentifier("bool"), null);

                data.LvalueTypes[lengthLink1] =
                    data.LvalueTypes[lengthLink2] =
                    data.LvalueTypes[lengthLink3] =
                    data.LvalueTypes[lengthLink4] =
                    data.ExpTypes[lengthLink1Exp] =
                    data.ExpTypes[lengthLink2Exp] =
                    data.ExpTypes[lengthLink4Exp] =
                    data.ExpTypes[binopExp3] =
                    data.ExpTypes[binopExp2.GetRight()] =
                    data.ExpTypes[binopExp3.GetRight()] =
                    data.ExpTypes[dataTableGetIntInvoke] =
                    data.ExpTypes[assignment] = new ANamedType(new TIdentifier("int"), null);



                data.ExpTypes[dataTableRemoveInvoke1] =
                    data.ExpTypes[dataTableRemoveInvoke2] =
                    data.ExpTypes[dataTableRemoveInvoke3] = new AVoidType(new TVoid("void"));


                deleteArrayMethod = method;
                return deleteArrayMethod;
            }

            public static AMethodDecl CreateDeleteStructMethod(Node node, AStructDecl structDecl, SharedData data)
            {
                if (deleteStructMethod.ContainsKey(structDecl))
                    return deleteStructMethod[structDecl];

                if (structDecl.GetIntDim() == null)
                    return CreateDeleteStructMethodDataTable(node, structDecl, data);
                return CreateDeleteStructMethodGlobalArray(node, structDecl, data);
            }

            public static AMethodDecl CreateDeleteEnrichmentMethod(Node node, AEnrichmentDecl enrichmentDecl, SharedData data)
            {
                if (deleteEnrichmentMethod.ContainsKey(enrichmentDecl))
                    return deleteEnrichmentMethod[enrichmentDecl];

                if (enrichmentDecl.GetIntDim() == null)
                    return CreateDeleteObjectMethod(node, data);
                return CreateDeleteEnrichmentMethodGlobalArray(node, enrichmentDecl, data);
            }

            public static AMethodDecl CreateDeleteEnrichmentMethodGlobalArray(Node node, AEnrichmentDecl enrichmentDecl, SharedData data)
            {
                deleteEnrichmentMethod[enrichmentDecl] = CreateDeleteStructMethodGlobalArrayP(node, enrichmentDecl.GetIntDim(),
                                                                                      Util.TypeToIdentifierString(enrichmentDecl.GetType()),
                                                                                      CreateEnrichmentFields(node,
                                                                                                         enrichmentDecl,
                                                                                                         data), data);
                return deleteEnrichmentMethod[enrichmentDecl];
            }

            public static AMethodDecl CreateDeleteStructMethodGlobalArray(Node node, AStructDecl structDecl, SharedData data)
            {
                deleteStructMethod[structDecl] = CreateDeleteStructMethodGlobalArrayP(node, structDecl.GetIntDim(),
                                                                                      structDecl.GetName().Text,
                                                                                      CreateStructFields(node,
                                                                                                         structDecl,
                                                                                                         data), data);
                return deleteStructMethod[structDecl];

                #region Comment

                /*
                GlobalStructVars vars = CreateStructFields(node, structDecl, data);
                AASourceFile file = Util.GetAncestor<AASourceFile>(node);

                AIntConstExp intConst1 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst2 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst3 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst4 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst5 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst6 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst7 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst8 = new AIntConstExp(new TIntegerLiteral("1"));
                AIntConstExp intConst9 = new AIntConstExp(new TIntegerLiteral("0"));
                AIntConstExp intConst10 = new AIntConstExp(new TIntegerLiteral((int.Parse(structDecl.GetIntDim().Text) - 1).ToString()));

                AFieldLvalue strUsedRef1 = new AFieldLvalue(new TIdentifier(vars.Used.GetName().Text));
                AFieldLvalue strUsedRef2 = new AFieldLvalue(new TIdentifier(vars.Used.GetName().Text));
                AFieldLvalue strUsedRef3 = new AFieldLvalue(new TIdentifier(vars.Used.GetName().Text));
                AFieldLvalue strUsedRef4 = new AFieldLvalue(new TIdentifier(vars.Used.GetName().Text));
                ALvalueExp strUsedRef1Exp = new ALvalueExp(strUsedRef1);
                ALvalueExp strUsedRef2Exp = new ALvalueExp(strUsedRef2);
                ALvalueExp strUsedRef3Exp = new ALvalueExp(strUsedRef3);
                ALvalueExp strUsedRef4Exp = new ALvalueExp(strUsedRef4);

                AFieldLvalue strIndexRef1 = new AFieldLvalue(new TIdentifier(vars.Index.GetName().Text));
                AFieldLvalue strIndexRef2 = new AFieldLvalue(new TIdentifier(vars.Index.GetName().Text));
                AFieldLvalue strIndexRef3 = new AFieldLvalue(new TIdentifier(vars.Index.GetName().Text));
                ALvalueExp strIndexRef1Exp = new ALvalueExp(strIndexRef1);
                ALvalueExp strIndexRef2Exp = new ALvalueExp(strIndexRef2);

                AALocalDecl iDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, new ANamedType(new TIdentifier("int"), null),
                                                    new TIdentifier("i"), null);
                ALocalLvalue iRef1 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef2 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef3 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef4 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef5 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef6 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef7 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef8 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef9 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef10 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef11 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef12 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef13 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef14 = new ALocalLvalue(new TIdentifier("i"));
                ALvalueExp iRef1Exp = new ALvalueExp(iRef1);
                ALvalueExp iRef2Exp = new ALvalueExp(iRef2);
                ALvalueExp iRef3Exp = new ALvalueExp(iRef3);
                ALvalueExp iRef4Exp = new ALvalueExp(iRef4);
                ALvalueExp iRef5Exp = new ALvalueExp(iRef5);
                ALvalueExp iRef6Exp = new ALvalueExp(iRef6);
                ALvalueExp iRef7Exp = new ALvalueExp(iRef7);
                ALvalueExp iRef8Exp = new ALvalueExp(iRef8);
                ALvalueExp iRef10Exp = new ALvalueExp(iRef10);
                ALvalueExp iRef11Exp = new ALvalueExp(iRef11);
                ALvalueExp iRef13Exp = new ALvalueExp(iRef13);
                ALvalueExp iRef14Exp = new ALvalueExp(iRef14);

                ABinopExp binop1 = new ABinopExp(iRef1Exp, new ADivideBinop(new TDiv("/")), intConst1);
                ABinopExp binop2 = new ABinopExp(iRef2Exp, new AModuloBinop(new TMod("%")), intConst2);
                ABinopExp binop3 = new ABinopExp(null, new AAndBinop(new TAnd("&")), null);
                ABinopExp binop4 = new ABinopExp(iRef3Exp, new ADivideBinop(new TDiv("/")), intConst3);
                ABinopExp binop5 = new ABinopExp(iRef4Exp, new ADivideBinop(new TDiv("/")), intConst4);
                ABinopExp binop6 = new ABinopExp(iRef5Exp, new AModuloBinop(new TMod("%")), intConst5);
                ABinopExp binop7 = new ABinopExp(null, new AMinusBinop(new TMinus("-")), null);
                ABinopExp binop8 = new ABinopExp(iRef6Exp, new AEqBinop(new TEq("==")), strIndexRef1Exp);
                ABinopExp binop9 = new ABinopExp(iRef7Exp, new ADivideBinop(new TDiv("/")), intConst6);
                ABinopExp binop10 = new ABinopExp(iRef8Exp, new AModuloBinop(new TMod("%")), intConst7);
                ABinopExp binop11 = new ABinopExp(null, new AAndBinop(new TAnd("&")), null);
                ABinopExp binop12 = new ABinopExp(iRef10Exp, new AMinusBinop(new TMinus("-")), intConst8);
                ABinopExp binop13 = new ABinopExp(iRef11Exp, new ALtBinop(new TLt("<")), intConst9);
                ABinopExp binop14 = new ABinopExp(iRef13Exp, new AEqBinop(new TEq("==")), strIndexRef2Exp);

                AArrayLvalue arrayIndex1 = new AArrayLvalue(new TLBracket("["), strUsedRef1Exp, binop1);
                AArrayLvalue arrayIndex2 = new AArrayLvalue(new TLBracket("["), strUsedRef2Exp, binop4);
                AArrayLvalue arrayIndex3 = new AArrayLvalue(new TLBracket("["), strUsedRef3Exp, binop5);
                AArrayLvalue arrayIndex4 = new AArrayLvalue(new TLBracket("["), strUsedRef4Exp, binop9);
                ALvalueExp arrayIndex1Exp = new ALvalueExp(arrayIndex1);
                ALvalueExp arrayIndex3Exp = new ALvalueExp(arrayIndex3);
                ALvalueExp arrayIndex4Exp = new ALvalueExp(arrayIndex4);
                binop3.SetLeft(arrayIndex1Exp);
                binop7.SetLeft(arrayIndex3Exp);
                binop11.SetLeft(arrayIndex4Exp);

                ASimpleInvokeExp power2Invoke1 = new ASimpleInvokeExp(new TIdentifier("Power2"), new ArrayList() { binop2 });
                ASimpleInvokeExp power2Invoke2 = new ASimpleInvokeExp(new TIdentifier("Power2"), new ArrayList() { binop6 });
                ASimpleInvokeExp power2Invoke3 = new ASimpleInvokeExp(new TIdentifier("Power2"), new ArrayList() { binop10 });
                binop3.SetRight(power2Invoke1);
                binop7.SetRight(power2Invoke2);
                binop11.SetRight(power2Invoke3);

                AParenExp paren1 = new AParenExp(binop3);
                AParenExp paren2 = new AParenExp(binop11);

                AUnopExp unop1 = new AUnopExp(new AComplementUnop(new TComplement("!")), paren1);
                AUnopExp unop2 = new AUnopExp(new AComplementUnop(new TComplement("!")), paren2);

                AAssignmentExp assignment1 = new AAssignmentExp(new TAssign("="), arrayIndex2, binop7);
                AAssignmentExp assignment2 = new AAssignmentExp(new TAssign("="), iRef9, binop12);
                AAssignmentExp assignment3 = new AAssignmentExp(new TAssign("="), iRef12, intConst10);
                AAssignmentExp assignment4 = new AAssignmentExp(new TAssign("="), strIndexRef3, iRef14Exp);

                deleteStructMethod[structDecl] = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null,
                                                                 new AVoidType(new TVoid("void")),
                                                                 new TIdentifier("Delete" + structDecl.GetName().Text, data.LineCounts[file] + 18, 0),
                                                                 new ArrayList() {iDecl},
                                                                 new AABlock(
                                                                     new ArrayList()
                                                                         {
                                                                             new AIfThenStm(new TLParen("("), unop1,
                                                                                            new ABlockStm(
                                                                                                new TLBrace("{"),
                                                                                                new AABlock(
                                                                                                    new ArrayList()
                                                                                                        {
                                                                                                            new AVoidReturnStm
                                                                                                                (new TReturn
                                                                                                                     ("return"))
                                                                                                        },
                                                                                                    new TRBrace("}")))),
                                                                             new AExpStm(new TSemicolon(";"),
                                                                                         assignment1),
                                                                             new AIfThenStm(new TLParen("("), binop8,
                                                                                            new ABlockStm(
                                                                                                new TLBrace("{"),
                                                                                                new AABlock(
                                                                                                    new ArrayList()
                                                                                                        {
                                                                                                            new AWhileStm
                                                                                                                (new TLParen
                                                                                                                     ("("),
                                                                                                                 unop2,
                                                                                                                 new ABlockStm
                                                                                                                     (new TLBrace
                                                                                                                          ("{"),
                                                                                                                      new AABlock
                                                                                                                          (new ArrayList
                                                                                                                               ()
                                                                                                                               {
                                                                                                                                   new AExpStm
                                                                                                                                       (new TSemicolon
                                                                                                                                            (";"),
                                                                                                                                        assignment2),
                                                                                                                                   new AIfThenStm
                                                                                                                                       (new TLParen
                                                                                                                                            ("("),
                                                                                                                                        binop13,
                                                                                                                                        new ABlockStm
                                                                                                                                            (new TLBrace
                                                                                                                                                 ("{"),
                                                                                                                                             new AABlock
                                                                                                                                                 (new ArrayList
                                                                                                                                                      ()
                                                                                                                                                      {
                                                                                                                                                          new AExpStm
                                                                                                                                                              (new TSemicolon
                                                                                                                                                                   (";"),
                                                                                                                                                               assignment3)
                                                                                                                                                      },
                                                                                                                                                  new TRBrace
                                                                                                                                                      ("}")))),
                                                                                                                                   new AIfThenStm
                                                                                                                                       (new TLParen
                                                                                                                                            ("("),
                                                                                                                                        binop14,
                                                                                                                                        new ABlockStm
                                                                                                                                            (new TLBrace
                                                                                                                                                 ("{"),
                                                                                                                                             new AABlock
                                                                                                                                                 (new ArrayList
                                                                                                                                                      ()
                                                                                                                                                      {
                                                                                                                                                          new ABreakStm
                                                                                                                                                              (new TBreak
                                                                                                                                                                   ("break"))
                                                                                                                                                      },
                                                                                                                                                  new TRBrace
                                                                                                                                                      ("}"))))
                                                                                                                               },
                                                                                                                           new TRBrace
                                                                                                                               ("}")))),
                                                                                                            new AExpStm(
                                                                                                                new TSemicolon
                                                                                                                    (";"),
                                                                                                                assignment4)
                                                                                                        },
                                                                                                    new TRBrace("}"))))
                                                                         },
                                                                      new TRBrace("}")));

                file.GetDecl().Add(deleteStructMethod[structDecl]);

                data.FieldLinks[strUsedRef1] =
                    data.FieldLinks[strUsedRef2] =
                    data.FieldLinks[strUsedRef3] =
                    data.FieldLinks[strUsedRef4] = vars.Used;
                data.FieldLinks[strIndexRef1] =
                    data.FieldLinks[strIndexRef2] =
                    data.FieldLinks[strIndexRef3] = vars.Index;
                data.LocalLinks[iRef1] =
                    data.LocalLinks[iRef2] =
                    data.LocalLinks[iRef3] =
                    data.LocalLinks[iRef4] =
                    data.LocalLinks[iRef5] =
                    data.LocalLinks[iRef6] =
                    data.LocalLinks[iRef7] =
                    data.LocalLinks[iRef8] =
                    data.LocalLinks[iRef9] =
                    data.LocalLinks[iRef10] =
                    data.LocalLinks[iRef11] =
                    data.LocalLinks[iRef12] =
                    data.LocalLinks[iRef13] =
                    data.LocalLinks[iRef14] = iDecl;
                data.SimpleMethodLinks[power2Invoke1] =
                    data.SimpleMethodLinks[power2Invoke2] =
                    data.SimpleMethodLinks[power2Invoke3] = CreatePower2Method(node, data);

                data.ExpTypes[intConst1] =
                    data.ExpTypes[intConst2] =
                    data.ExpTypes[intConst3] =
                    data.ExpTypes[intConst4] =
                    data.ExpTypes[intConst5] =
                    data.ExpTypes[intConst6] =
                    data.ExpTypes[intConst7] =
                    data.ExpTypes[intConst8] =
                    data.ExpTypes[intConst9] =
                    data.ExpTypes[intConst10] =
                    data.LvalueTypes[iRef1] =
                    data.LvalueTypes[iRef2] =
                    data.LvalueTypes[iRef3] =
                    data.LvalueTypes[iRef4] =
                    data.LvalueTypes[iRef5] =
                    data.LvalueTypes[iRef6] =
                    data.LvalueTypes[iRef7] =
                    data.LvalueTypes[iRef8] =
                    data.LvalueTypes[iRef9] =
                    data.LvalueTypes[iRef10] =
                    data.LvalueTypes[iRef11] =
                    data.LvalueTypes[iRef12] =
                    data.LvalueTypes[iRef13] =
                    data.LvalueTypes[iRef14] =
                    data.ExpTypes[iRef1Exp] =
                    data.ExpTypes[iRef2Exp] =
                    data.ExpTypes[iRef3Exp] =
                    data.ExpTypes[iRef4Exp] =
                    data.ExpTypes[iRef5Exp] =
                    data.ExpTypes[iRef6Exp] =
                    data.ExpTypes[iRef7Exp] =
                    data.ExpTypes[iRef8Exp] =
                    data.ExpTypes[iRef10Exp] =
                    data.ExpTypes[iRef11Exp] =
                    data.ExpTypes[iRef13Exp] =
                    data.ExpTypes[iRef14Exp] =
                    data.LvalueTypes[arrayIndex1] =
                    data.LvalueTypes[arrayIndex2] =
                    data.LvalueTypes[arrayIndex3] =
                    data.LvalueTypes[arrayIndex4] =
                    data.ExpTypes[arrayIndex1Exp] =
                    data.ExpTypes[arrayIndex3Exp] =
                    data.ExpTypes[arrayIndex4Exp] =
                    data.ExpTypes[binop1] =
                    data.ExpTypes[binop2] =
                    data.ExpTypes[binop3] =
                    data.ExpTypes[binop4] =
                    data.ExpTypes[binop5] =
                    data.ExpTypes[binop6] =
                    data.ExpTypes[binop7] =
                    data.ExpTypes[binop9] =
                    data.ExpTypes[binop10] =
                    data.ExpTypes[binop11] =
                    data.ExpTypes[binop12] =
                    data.ExpTypes[paren1] =
                    data.ExpTypes[paren2] =
                    data.ExpTypes[power2Invoke1] =
                    data.ExpTypes[power2Invoke2] =
                    data.ExpTypes[power2Invoke3] = new ANamedType(new TIdentifier("int"), null);

                data.ExpTypes[binop8] =
                    data.ExpTypes[binop13] =
                    data.ExpTypes[binop14] =
                    data.ExpTypes[unop1] =
                    data.ExpTypes[unop2] = new ANamedType(new TIdentifier("bool"), null);

                return deleteStructMethod[structDecl];*/

                #endregion
            }

            public static AMethodDecl CreateDeleteStructMethodGlobalArrayP(Node node, TIntegerLiteral intDim, string prefix, GlobalStructVars vars, SharedData data)
            {
                /*
                    void DeleteStr(int i)
                    {
                        if (!(Str_used[i / 31] & Power2(i % 31)))
                        {
                            return;
                        }
                        Str_used[i / 31] = Str_used[i / 31] - Power2(i % 31);
                        stack[freeCount] = i;
                        freeCount += 1;
                        /*if (i == Str_index)
                        {
                            while (!(Str_used[i / 31] & Power2(i % 31)))
                            {
                                i = i - 1;
                                if (i < 0)
                                {
                                    i = 41;
                                }
                                if (i == Str_index)
                                {
                                    //Everything is free
                                    break;
                                }
                            }
                            Str_index = i;
                        }* /
                    }
                 */

                AASourceFile file = Util.GetAncestor<AASourceFile>(node);

                AIntConstExp intConst1 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst2 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst3 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst4 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst5 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst6 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst7 = new AIntConstExp(new TIntegerLiteral("31"));
                AIntConstExp intConst8 = new AIntConstExp(new TIntegerLiteral("1"));
                AIntConstExp intConst9 = new AIntConstExp(new TIntegerLiteral("0"));
                AIntConstExp intConst10 = new AIntConstExp(new TIntegerLiteral((int.Parse(intDim.Text) - 1).ToString()));
                AIntConstExp intConst11 = new AIntConstExp(new TIntegerLiteral("1"));
                AIntConstExp intConst12 = new AIntConstExp(new TIntegerLiteral("1"));
                AIntConstExp intConst13 = new AIntConstExp(new TIntegerLiteral("1"));

                AFieldLvalue strUsedRef1 = new AFieldLvalue(new TIdentifier(vars.Used.GetName().Text));
                AFieldLvalue strUsedRef2 = new AFieldLvalue(new TIdentifier(vars.Used.GetName().Text));
                AFieldLvalue strUsedRef3 = new AFieldLvalue(new TIdentifier(vars.Used.GetName().Text));
                AFieldLvalue strUsedRef4 = new AFieldLvalue(new TIdentifier(vars.Used.GetName().Text));
                ALvalueExp strUsedRef1Exp = new ALvalueExp(strUsedRef1);
                ALvalueExp strUsedRef2Exp = new ALvalueExp(strUsedRef2);
                ALvalueExp strUsedRef3Exp = new ALvalueExp(strUsedRef3);
                ALvalueExp strUsedRef4Exp = new ALvalueExp(strUsedRef4);

                AFieldLvalue strIndexRef1 = new AFieldLvalue(new TIdentifier(vars.Index.GetName().Text));
                AFieldLvalue strIndexRef2 = new AFieldLvalue(new TIdentifier(vars.Index.GetName().Text));
                AFieldLvalue strIndexRef3 = new AFieldLvalue(new TIdentifier(vars.Index.GetName().Text));
                ALvalueExp strIndexRef1Exp = new ALvalueExp(strIndexRef1);
                ALvalueExp strIndexRef2Exp = new ALvalueExp(strIndexRef2);

                AALocalDecl iDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, new ANamedType(new TIdentifier("int"), null),
                                                    new TIdentifier("i"), null);
                ALocalLvalue iRef1 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef2 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef3 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef4 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef5 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef6 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef7 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef8 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef9 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef10 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef11 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef12 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef13 = new ALocalLvalue(new TIdentifier("i"));
                ALocalLvalue iRef14 = new ALocalLvalue(new TIdentifier("i"));
                ALvalueExp iRef1Exp = new ALvalueExp(iRef1);
                ALvalueExp iRef2Exp = new ALvalueExp(iRef2);
                ALvalueExp iRef3Exp = new ALvalueExp(iRef3);
                ALvalueExp iRef4Exp = new ALvalueExp(iRef4);
                ALvalueExp iRef5Exp = new ALvalueExp(iRef5);
                ALvalueExp iRef6Exp = new ALvalueExp(iRef6);
                ALvalueExp iRef7Exp = new ALvalueExp(iRef7);
                ALvalueExp iRef8Exp = new ALvalueExp(iRef8);
                ALvalueExp iRef10Exp = new ALvalueExp(iRef10);
                ALvalueExp iRef11Exp = new ALvalueExp(iRef11);
                ALvalueExp iRef13Exp = new ALvalueExp(iRef13);
                ALvalueExp iRef14Exp = new ALvalueExp(iRef14);

                ABinopExp binop1 = new ABinopExp(iRef1Exp, new ADivideBinop(new TDiv("/")), intConst1);
                ABinopExp binop2 = new ABinopExp(iRef2Exp, new AModuloBinop(new TMod("%")), intConst2);
                ABinopExp binop3 = new ABinopExp(null, new AAndBinop(new TAnd("&")), null);
                ABinopExp binop4 = new ABinopExp(iRef3Exp, new ADivideBinop(new TDiv("/")), intConst3);
                ABinopExp binop5 = new ABinopExp(iRef4Exp, new ADivideBinop(new TDiv("/")), intConst4);
                ABinopExp binop6 = new ABinopExp(iRef5Exp, new AModuloBinop(new TMod("%")), intConst5);
                ABinopExp binop7 = new ABinopExp(null, new AMinusBinop(new TMinus("-")), null);
                ABinopExp binop8 = new ABinopExp(iRef6Exp, new AEqBinop(new TEq("==")), strIndexRef1Exp);
                ABinopExp binop9 = new ABinopExp(iRef7Exp, new ADivideBinop(new TDiv("/")), intConst6);
                ABinopExp binop10 = new ABinopExp(iRef8Exp, new AModuloBinop(new TMod("%")), intConst7);
                ABinopExp binop11 = new ABinopExp(null, new AAndBinop(new TAnd("&")), null);
                ABinopExp binop12 = new ABinopExp(iRef10Exp, new AMinusBinop(new TMinus("-")), intConst8);
                ABinopExp binop13 = new ABinopExp(iRef11Exp, new ALtBinop(new TLt("<")), intConst9);
                ABinopExp binop14 = new ABinopExp(iRef13Exp, new AEqBinop(new TEq("==")), strIndexRef2Exp);

                AArrayLvalue arrayIndex1 = new AArrayLvalue(new TLBracket("["), strUsedRef1Exp, binop1);
                AArrayLvalue arrayIndex2 = new AArrayLvalue(new TLBracket("["), strUsedRef2Exp, binop4);
                AArrayLvalue arrayIndex3 = new AArrayLvalue(new TLBracket("["), strUsedRef3Exp, binop5);
                AArrayLvalue arrayIndex4 = new AArrayLvalue(new TLBracket("["), strUsedRef4Exp, binop9);
                ALvalueExp arrayIndex1Exp = new ALvalueExp(arrayIndex1);
                ALvalueExp arrayIndex3Exp = new ALvalueExp(arrayIndex3);
                ALvalueExp arrayIndex4Exp = new ALvalueExp(arrayIndex4);
                binop3.SetLeft(arrayIndex1Exp);
                binop7.SetLeft(arrayIndex3Exp);
                binop11.SetLeft(arrayIndex4Exp);


                ABinopExp binop15 = new ABinopExp(intConst11, new ALBitShiftBinop(new TLBitShift("<<")), binop2);
                ABinopExp binop16 = new ABinopExp(intConst12, new ALBitShiftBinop(new TLBitShift("<<")), binop6);
                ABinopExp binop17 = new ABinopExp(intConst13, new ALBitShiftBinop(new TLBitShift("<<")), binop10);
                binop3.SetRight(binop15);
                binop7.SetRight(binop16);
                binop11.SetRight(binop17);
                /*ASimpleInvokeExp power2Invoke1 = new ASimpleInvokeExp(new TIdentifier("Power2"), new ArrayList() { binop2 });
                ASimpleInvokeExp power2Invoke2 = new ASimpleInvokeExp(new TIdentifier("Power2"), new ArrayList() { binop6 });
                ASimpleInvokeExp power2Invoke3 = new ASimpleInvokeExp(new TIdentifier("Power2"), new ArrayList() { binop10 });
                binop3.SetRight(power2Invoke1);
                binop7.SetRight(power2Invoke2);
                binop11.SetRight(power2Invoke3);*/

                AParenExp paren1 = new AParenExp(binop3);
                AParenExp paren2 = new AParenExp(binop11);

                AUnopExp unop1 = new AUnopExp(new AComplementUnop(new TComplement("!")), paren1);
                AUnopExp unop2 = new AUnopExp(new AComplementUnop(new TComplement("!")), paren2);

                AAssignmentExp assignment1 = new AAssignmentExp(new TAssign("="), arrayIndex2, binop7);
                AAssignmentExp assignment2 = new AAssignmentExp(new TAssign("="), iRef9, binop12);
                AAssignmentExp assignment3 = new AAssignmentExp(new TAssign("="), iRef12, intConst10);
                AAssignmentExp assignment4 = new AAssignmentExp(new TAssign("="), strIndexRef3, iRef14Exp);

                AMethodDecl method = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null,
                                                                 new AVoidType(new TVoid("void")),
                                                                 new TIdentifier("Delete" + prefix, data.LineCounts[file] + 18, 0),
                                                                 new ArrayList() { iDecl },
                                                                 new AABlock(
                                                                     new ArrayList()
                                                                         {
                                                                             new AIfThenStm(new TLParen("("), unop1,
                                                                                            new ABlockStm(
                                                                                                new TLBrace("{"),
                                                                                                new AABlock(
                                                                                                    new ArrayList()
                                                                                                        {
                                                                                                            new AVoidReturnStm
                                                                                                                (new TReturn
                                                                                                                     ("return"))
                                                                                                        },
                                                                                                    new TRBrace("}")))),
                                                                             new AExpStm(new TSemicolon(";"),
                                                                                         assignment1),
                                                                             new AIfThenStm(new TLParen("("), binop8,
                                                                                            new ABlockStm(
                                                                                                new TLBrace("{"),
                                                                                                new AABlock(
                                                                                                    new ArrayList()
                                                                                                        {
                                                                                                            new AWhileStm
                                                                                                                (new TLParen
                                                                                                                     ("("),
                                                                                                                 unop2,
                                                                                                                 new ABlockStm
                                                                                                                     (new TLBrace
                                                                                                                          ("{"),
                                                                                                                      new AABlock
                                                                                                                          (new ArrayList
                                                                                                                               ()
                                                                                                                               {
                                                                                                                                   new AExpStm
                                                                                                                                       (new TSemicolon
                                                                                                                                            (";"),
                                                                                                                                        assignment2),
                                                                                                                                   new AIfThenStm
                                                                                                                                       (new TLParen
                                                                                                                                            ("("),
                                                                                                                                        binop13,
                                                                                                                                        new ABlockStm
                                                                                                                                            (new TLBrace
                                                                                                                                                 ("{"),
                                                                                                                                             new AABlock
                                                                                                                                                 (new ArrayList
                                                                                                                                                      ()
                                                                                                                                                      {
                                                                                                                                                          new AExpStm
                                                                                                                                                              (new TSemicolon
                                                                                                                                                                   (";"),
                                                                                                                                                               assignment3)
                                                                                                                                                      },
                                                                                                                                                  new TRBrace
                                                                                                                                                      ("}")))),
                                                                                                                                   new AIfThenStm
                                                                                                                                       (new TLParen
                                                                                                                                            ("("),
                                                                                                                                        binop14,
                                                                                                                                        new ABlockStm
                                                                                                                                            (new TLBrace
                                                                                                                                                 ("{"),
                                                                                                                                             new AABlock
                                                                                                                                                 (new ArrayList
                                                                                                                                                      ()
                                                                                                                                                      {
                                                                                                                                                          new ABreakStm
                                                                                                                                                              (new TBreak
                                                                                                                                                                   ("break"))
                                                                                                                                                      },
                                                                                                                                                  new TRBrace
                                                                                                                                                      ("}"))))
                                                                                                                               },
                                                                                                                           new TRBrace
                                                                                                                               ("}")))),
                                                                                                            new AExpStm(
                                                                                                                new TSemicolon
                                                                                                                    (";"),
                                                                                                                assignment4)
                                                                                                        },
                                                                                                    new TRBrace("}"))))
                                                                         },
                                                                      new TRBrace("}")));

                file.GetDecl().Add(method);
                data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(file, method));

                data.FieldLinks[strUsedRef1] =
                    data.FieldLinks[strUsedRef2] =
                    data.FieldLinks[strUsedRef3] =
                    data.FieldLinks[strUsedRef4] = vars.Used;
                data.FieldLinks[strIndexRef1] =
                    data.FieldLinks[strIndexRef2] =
                    data.FieldLinks[strIndexRef3] = vars.Index;
                data.LocalLinks[iRef1] =
                    data.LocalLinks[iRef2] =
                    data.LocalLinks[iRef3] =
                    data.LocalLinks[iRef4] =
                    data.LocalLinks[iRef5] =
                    data.LocalLinks[iRef6] =
                    data.LocalLinks[iRef7] =
                    data.LocalLinks[iRef8] =
                    data.LocalLinks[iRef9] =
                    data.LocalLinks[iRef10] =
                    data.LocalLinks[iRef11] =
                    data.LocalLinks[iRef12] =
                    data.LocalLinks[iRef13] =
                    data.LocalLinks[iRef14] = iDecl;
                //data.SimpleMethodLinks[power2Invoke1] =
                //    data.SimpleMethodLinks[power2Invoke2] =
                //    data.SimpleMethodLinks[power2Invoke3] = CreatePower2Method(node, data);

                data.ExpTypes[intConst1] =
                    data.ExpTypes[intConst2] =
                    data.ExpTypes[intConst3] =
                    data.ExpTypes[intConst4] =
                    data.ExpTypes[intConst5] =
                    data.ExpTypes[intConst6] =
                    data.ExpTypes[intConst7] =
                    data.ExpTypes[intConst8] =
                    data.ExpTypes[intConst9] =
                    data.ExpTypes[intConst10] =
                    data.ExpTypes[intConst11] =
                    data.ExpTypes[intConst12] =
                    data.ExpTypes[intConst13] =
                    data.LvalueTypes[iRef1] =
                    data.LvalueTypes[iRef2] =
                    data.LvalueTypes[iRef3] =
                    data.LvalueTypes[iRef4] =
                    data.LvalueTypes[iRef5] =
                    data.LvalueTypes[iRef6] =
                    data.LvalueTypes[iRef7] =
                    data.LvalueTypes[iRef8] =
                    data.LvalueTypes[iRef9] =
                    data.LvalueTypes[iRef10] =
                    data.LvalueTypes[iRef11] =
                    data.LvalueTypes[iRef12] =
                    data.LvalueTypes[iRef13] =
                    data.LvalueTypes[iRef14] =
                    data.ExpTypes[iRef1Exp] =
                    data.ExpTypes[iRef2Exp] =
                    data.ExpTypes[iRef3Exp] =
                    data.ExpTypes[iRef4Exp] =
                    data.ExpTypes[iRef5Exp] =
                    data.ExpTypes[iRef6Exp] =
                    data.ExpTypes[iRef7Exp] =
                    data.ExpTypes[iRef8Exp] =
                    data.ExpTypes[iRef10Exp] =
                    data.ExpTypes[iRef11Exp] =
                    data.ExpTypes[iRef13Exp] =
                    data.ExpTypes[iRef14Exp] =
                    data.LvalueTypes[arrayIndex1] =
                    data.LvalueTypes[arrayIndex2] =
                    data.LvalueTypes[arrayIndex3] =
                    data.LvalueTypes[arrayIndex4] =
                    data.ExpTypes[arrayIndex1Exp] =
                    data.ExpTypes[arrayIndex3Exp] =
                    data.ExpTypes[arrayIndex4Exp] =
                    data.ExpTypes[binop1] =
                    data.ExpTypes[binop2] =
                    data.ExpTypes[binop3] =
                    data.ExpTypes[binop4] =
                    data.ExpTypes[binop5] =
                    data.ExpTypes[binop6] =
                    data.ExpTypes[binop7] =
                    data.ExpTypes[binop9] =
                    data.ExpTypes[binop10] =
                    data.ExpTypes[binop11] =
                    data.ExpTypes[binop12] =
                    data.ExpTypes[binop15] =
                    data.ExpTypes[binop16] =
                    data.ExpTypes[binop17] =
                    data.ExpTypes[paren1] =
                    data.ExpTypes[paren2] =
                    data.ExpTypes[assignment1] =
                    data.ExpTypes[assignment2] =
                    data.ExpTypes[assignment3] =
                    data.ExpTypes[assignment4] =
                    /*data.ExpTypes[power2Invoke1] =
                    data.ExpTypes[power2Invoke2] =
                    data.ExpTypes[power2Invoke3] =*/ new ANamedType(new TIdentifier("int"), null);

                data.ExpTypes[binop8] =
                    data.ExpTypes[binop13] =
                    data.ExpTypes[binop14] =
                    data.ExpTypes[unop1] =
                    data.ExpTypes[unop2] = new ANamedType(new TIdentifier("bool"), null);

                return method;
            }

            public static AMethodDecl CreateDeleteStructMethodDataTable(Node node, AStructDecl structDecl, SharedData data)
            {

                AALocalDecl idDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, new ANamedType(new TIdentifier("string"), null), new TIdentifier("id"), null);
                AABlock block = new AABlock();
                AMethodDecl method = new AMethodDecl(new APublicVisibilityModifier(), null, null, null, null, null, new AVoidType(new TVoid("void")),
                                                     new TIdentifier("Delete" + structDecl.GetName().Text), new ArrayList() { idDecl }, block);

                ALocalLvalue idLink = new ALocalLvalue(new TIdentifier("id"));
                ALvalueExp idLinkExp = new ALvalueExp(idLink);


                data.Locals[block] = new List<AALocalDecl> { idDecl };
                data.LocalLinks[idLink] = idDecl;



                data.LvalueTypes[idLink] =
                    data.ExpTypes[idLinkExp] = new ANamedType(new TIdentifier("string"), null);

                ANamedType structType = new ANamedType(new TIdentifier(structDecl.GetName().Text), null);
                data.StructTypeLinks[structType] = structDecl;

                AddRemoves(idLinkExp, structType, block, data);


                AASourceFile sourceFile = Util.GetAncestor<AASourceFile>(node);
                sourceFile.GetDecl().Add(method);
                data.Methods.Add(new SharedData.DeclItem<AMethodDecl>(sourceFile, method));

                deleteStructMethod[structDecl] = method;
                return method;
            }



            private static void AddRemoves(PExp exp, PType type, AABlock block, SharedData data)
            {
                if (Util.IsBulkCopy(type))
                {
                    if (type is ANamedType)
                    {
                        ANamedType aType = (ANamedType)type;
                        AStructDecl structDecl = data.StructTypeLinks[aType];
                        foreach (AALocalDecl localDecl in structDecl.GetLocals())
                        {
                            ABinopExp newExp = new ABinopExp(exp, new APlusBinop(new TPlus("+")),
                                                     new AStringConstExp(
                                                         new TStringLiteral("\"." + localDecl.GetName().Text + "\"")));
                            data.ExpTypes[newExp] =
                                data.ExpTypes[newExp.GetRight()] =
                                new ANamedType(new TIdentifier("string"), null);

                            AddRemoves(newExp, localDecl.GetType(), block, data);
                        }
                    }
                    else
                    {//Is array type. Can Only be a constant array type
                        AArrayTempType aType = (AArrayTempType)type;
                        for (int i = 0; i < int.Parse(aType.GetIntDim().Text); i++)
                        {
                            ABinopExp newExp = new ABinopExp(exp, new APlusBinop(new TPlus("+")),
                                                     new AStringConstExp(
                                                         new TStringLiteral("\"[" + i + "]\"")));
                            data.ExpTypes[newExp] =
                                data.ExpTypes[newExp.GetRight()] =
                                new ANamedType(new TIdentifier("string"), null);

                            AddRemoves(newExp, aType.GetType(), block, data);
                        }

                    }
                }
                else
                {
                    exp = Util.MakeClone(exp, data);

                    ABooleanConstExp trueConst1 = new ABooleanConstExp(new ATrueBool());
                    ASimpleInvokeExp removeInvoke = new ASimpleInvokeExp(new TIdentifier("DataTableValueRemove"), new ArrayList() { trueConst1, exp });
                    block.GetStatements().Add(new AExpStm(new TSemicolon(";"), removeInvoke));

                    data.ExpTypes[trueConst1] = new ANamedType(new TIdentifier("bool"), null);

                    data.ExpTypes[removeInvoke] = new AVoidType(new TVoid("void"));

                    data.SimpleMethodLinks[removeInvoke] =
                        data.Libraries.Methods.First(m => m.GetName().Text == removeInvoke.GetName().Text);
                }
            }

            

            public override void CaseAPointerType(APointerType node)
            {
                //Convert to string or int type
                PType replacer;
                if (Util.IsIntPointer(node, node.GetType(), data))
                {
                    //Int type
                    replacer = new ANamedType(new TIdentifier("int"), null);
                }
                else
                {
                    //String type
                    replacer = new ANamedType(new TIdentifier("string"), null);
                }
                node.ReplaceBy(replacer);
                replacer.Apply(this);
            }

            public override void CaseANewExp(ANewExp node)
            {
                //Call new object or new array
                ASimpleInvokeExp invoke = new ASimpleInvokeExp(new TIdentifier("invoke"), new ArrayList());
                ANamedType pointerType = new ANamedType(new TIdentifier("string"), null);
                if (node.GetType() is AArrayTempType)
                {
                    if (newArrayMethod == null) CreateNewArrayMethod(node, data);

                    node.GetType().Apply(this);
                    AArrayTempType type = (AArrayTempType) node.GetType();
                    invoke.GetArgs().Add(Util.MakeClone(type.GetDimention(), data));
                    data.SimpleMethodLinks[invoke] = newArrayMethod;
                }
                else if (Util.IsIntPointer(node, node.GetType(), data))
                {
                    if (node.GetType() is ANamedType && data.StructTypeLinks.ContainsKey((ANamedType) node.GetType()))
                        data.SimpleMethodLinks[invoke] = CreateNewObjectMethod(node, data.StructTypeLinks[(ANamedType) node.GetType()],
                                                                               data);
                    else
                    {
                        data.SimpleMethodLinks[invoke] = CreateNewObjectMethod(node,
                                                                                data.EnrichmentTypeLinks[node.GetType()],
                                                                                data);
                    }
                    pointerType = new ANamedType(new TIdentifier("int"), null);
                }
                else
                {
                    if (newObjectMethod == null) CreateNewObjectMethod(node, data);

                    data.SimpleMethodLinks[invoke] = newObjectMethod;
                }
                node.ReplaceBy(invoke);
                data.ExpTypes[invoke] = pointerType;

                //Call initializer
                if (data.ConstructorLinks.ContainsKey(node))
                {
                    PStm pStm = Util.GetAncestor<PStm>(invoke);
                    AABlock pblock = (AABlock) pStm.Parent();

                    PLvalue lvalue;
                    ALvalueExp lvalueExp;
                    PStm stm;
                    AAssignmentExp assignment = null;
                    AALocalDecl localDecl = null;
                    if (invoke.Parent() is AAssignmentExp)
                    {
                        AAssignmentExp parent = (AAssignmentExp) invoke.Parent();
                        assignment = parent;
                        /*lvalue = parent.GetLvalue();
                        lvalue.Apply(new MoveMethodDeclsOut("pointerVar", data));
                        lvalue = Util.MakeClone(lvalue, data);*/
                    }
                    else if (invoke.Parent() is AALocalDecl)
                    {
                        AALocalDecl parent = (AALocalDecl) invoke.Parent();
                        localDecl = parent;
                        /*lvalue = new ALocalLvalue(new TIdentifier(parent.GetName().Text));

                        data.LocalLinks[(ALocalLvalue) lvalue] = parent;
                        data.LvalueTypes[lvalue] = parent.GetType();*/
                    }
                    else
                    {
                        //Move the new invocation out into a local decl, and use that
                        localDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                                Util.MakeClone(pointerType, data),
                                                                new TIdentifier("newVar"), null);
                        ALocalLvalue localLvalue = new ALocalLvalue(new TIdentifier("newVar"));
                        lvalueExp = new ALvalueExp(localLvalue);
                        invoke.ReplaceBy(lvalueExp);
                        localDecl.SetInit(invoke);
                        stm = new ALocalDeclStm(new TSemicolon(";"), localDecl);
                        pblock.GetStatements().Insert(pblock.GetStatements().IndexOf(pStm), stm);
                        pStm = stm;
                        //lvalue = new ALocalLvalue(new TIdentifier("newVar"));

                        //data.LocalLinks[(ALocalLvalue) lvalue] =
                            data.LocalLinks[localLvalue] = localDecl;
                        //data.LvalueTypes[lvalue] =
                            data.LvalueTypes[localLvalue] =
                            data.ExpTypes[lvalueExp] = localDecl.GetType();
                    }


                    ASimpleInvokeExp oldInvoke = invoke;
                    invoke = new ASimpleInvokeExp(new TIdentifier("renameMe"), new ArrayList());
                    while (node.GetArgs().Count > 0)
                    {
                        invoke.GetArgs().Add(node.GetArgs()[0]);
                    }
                    if (assignment != null)
                    {
                        assignment.SetExp(invoke);
                        invoke.GetArgs().Add(oldInvoke);
                    }
                    else
                    {
                        localDecl.SetInit(invoke);
                        invoke.GetArgs().Add(oldInvoke);
                    }
                    //lvalueExp = new ALvalueExp(lvalue);
                    //invoke.GetArgs().Add(lvalueExp);
                    //stm = new AExpStm(new TSemicolon(";"), invoke);
                    //pblock.GetStatements().Insert(pblock.GetStatements().IndexOf(pStm) + 1, stm);

                    //data.ExpTypes[lvalueExp] = data.LvalueTypes[lvalue];
                    data.SimpleMethodLinks[invoke] = data.ConstructorMap[data.ConstructorLinks[node]];
                    data.ExpTypes[invoke] = data.ConstructorMap[data.ConstructorLinks[node]].GetReturnType();
                    invoke.Apply(this);
                }
            }

            public override void CaseADeleteStm(ADeleteStm node)
            {
                List<Node> visitMeNext = new List<Node>();
                APointerType pointer = (APointerType) data.ExpTypes[node.GetExp()];


                //Call deconstructor if it exists
                {
                    AMethodDecl deconstructor = null;
                    if (pointer.GetType() is ANamedType &&
                        data.StructTypeLinks.ContainsKey((ANamedType) pointer.GetType()))
                    {
                        AStructDecl str = data.StructTypeLinks[(ANamedType) pointer.GetType()];

                        deconstructor =
                            data.DeconstructorMap[data.StructDeconstructor[str]];

                    }
                    else //Look for enrichment deconstructor
                    {
                        foreach (AEnrichmentDecl enrichment in data.Enrichments)
                        {
                            if (Util.TypesEqual(pointer.GetType(), enrichment.GetType(), data))
                            {
                                foreach (PDecl decl in enrichment.GetDecl())
                                {
                                    if (decl is ADeconstructorDecl)
                                    {
                                        deconstructor = data.DeconstructorMap[(ADeconstructorDecl) decl];
                                        break;
                                    }
                                }
                                if (deconstructor != null)
                                    break;
                            }
                        }
                    }
                    if (deconstructor != null)
                    {
                        /*
                         * Convert delete <exp>; to
                         * 
                         * var deleteVar = <exp>;
                         * Deconstructor(deleteVar);
                         * delete deleteVar;
                         */

                        

                        AALocalDecl deleteVarDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null,
                                                                    null,
                                                                    new ANamedType(
                                                                        new TIdentifier(Util.IsIntPointer(node,
                                                                                                          pointer.
                                                                                                              GetType(),
                                                                                                          data)
                                                                                            ? "int"
                                                                                            : "string"), null),
                                                                    new TIdentifier("deleteVar"), node.GetExp());
                        ALocalLvalue deleteVarRef = new ALocalLvalue(new TIdentifier("deleteVar"));
                        ALvalueExp deleteVarRefExp = new ALvalueExp(deleteVarRef);
                        node.SetExp(deleteVarRefExp);

                        AABlock pBlock = (AABlock) node.Parent();
                        pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(node),
                                                      new ALocalDeclStm(new TSemicolon(";"), deleteVarDecl));

                        data.LocalLinks[deleteVarRef] = deleteVarDecl;
                        data.LvalueTypes[deleteVarRef] =
                            data.ExpTypes[deleteVarRefExp] = data.ExpTypes[deleteVarDecl.GetInit()];

                        deleteVarRef = new ALocalLvalue(new TIdentifier("deleteVar"));
                        deleteVarRefExp = new ALvalueExp(deleteVarRef);



                        ASimpleInvokeExp invoke = new ASimpleInvokeExp(new TIdentifier(deconstructor.GetName().Text),
                                                                       new ArrayList() {deleteVarRefExp});
                        pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(node),
                                                      new AExpStm(new TSemicolon(";"), invoke));


                        data.LocalLinks[deleteVarRef] = deleteVarDecl;
                        data.LvalueTypes[deleteVarRef] =
                            data.ExpTypes[deleteVarRefExp] = data.ExpTypes[deleteVarDecl.GetInit()];
                        data.SimpleMethodLinks[invoke] = deconstructor;
                        data.ExpTypes[invoke] = deconstructor.GetReturnType();
                        visitMeNext.Add(deleteVarDecl);


                    }
                }


                if (pointer.GetType() is AArrayTempType || pointer.GetType() is ADynamicArrayType)
                {
                    //If struct array, delete all struct data
                    PType baseType;
                    if (pointer.GetType() is AArrayTempType)
                        baseType = ((AArrayTempType) pointer.GetType()).GetType();
                    else
                        baseType = ((ADynamicArrayType)pointer.GetType()).GetType();

                    PExp pointerString = node.GetExp();
                    if (Util.IsBulkCopy(baseType))
                    {
                        node.GetExp().Apply(new MoveMethodDeclsOut("pointerVar", data));
                        pointerString = node.GetExp();
                        ANamedType aBaseType = (ANamedType) baseType;
                        /* Add the following
                         * 
                         * string deleteMe = node.getExp(); <-- no
                         * int i = 0;
                         * while (i < array.length)
                         * {
                         *      DeleteStruct<name>(deleteMe + StringToInt(i));
                         *      i = i + 1;
                         * }
                         * 
                         */
                        //string deleteMe = node.getExp();
                        /*AALocalDecl deleteMeVar = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                                  new ANamedType(new TIdentifier("string"), null),
                                                                  new TIdentifier("deleteMe"), node.GetExp());*/
                        //pointerString = deleteMeVar;
                        //int i = 0;
                        AALocalDecl iVar = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                           new ANamedType(new TIdentifier("int"), null),
                                                           new TIdentifier("i"),
                                                           new AIntConstExp(new TIntegerLiteral("0")));
                        //i < array.length
                        ASimpleInvokeExp lenghCall = new ASimpleInvokeExp(new TIdentifier("DataTableGetInt"), new ArrayList());
                        lenghCall.GetArgs().Add(new ABooleanConstExp(new ATrueBool()));
                        //ALocalLvalue deleteMeUse1 = new ALocalLvalue(new TIdentifier("deleteMeVar"));
                        ABinopExp arrayLengthString = new ABinopExp(Util.MakeClone(pointerString, data),
                                                                    new APlusBinop(new TPlus("+")),
                                                                    new AStringConstExp(
                                                                        new TStringLiteral("\"\\\\Length\"")));
                        lenghCall.GetArgs().Add(arrayLengthString);
                        ALocalLvalue iUse1 = new ALocalLvalue(new TIdentifier("i"));
                        ABinopExp cond = new ABinopExp(new ALvalueExp(iUse1), new ALtBinop(new TLt("<")), lenghCall);

                        //DeleteStruct<name>(deleteMe + StringToInt(i));
                        //ALocalLvalue deleteMeUse2 = new ALocalLvalue(new TIdentifier("deleteMeVar"));
                        ALocalLvalue iUse2 = new ALocalLvalue(new TIdentifier("i"));
                        ASimpleInvokeExp intToString = new ASimpleInvokeExp(new TIdentifier("IntToString"), new ArrayList());
                        intToString.GetArgs().Add(new ALvalueExp(iUse2));
                        ABinopExp binopExp1 = new ABinopExp(Util.MakeClone(pointerString, data), new APlusBinop(new TPlus("+")), new AStringConstExp(new TStringLiteral("\"[\"")));
                        ABinopExp binopExp2 = new ABinopExp(binopExp1, new APlusBinop(new TPlus("+")), intToString);
                        ABinopExp binopExp3 = new ABinopExp(binopExp2, new APlusBinop(new TPlus("+")), new AStringConstExp(new TStringLiteral("\"]\"")));
                        ASimpleInvokeExp deleteStructInvoke = new ASimpleInvokeExp(new TIdentifier("DeleteStruct" + aBaseType.AsIdentifierString()), new ArrayList());
                        deleteStructInvoke.GetArgs().Add(binopExp3);
                        //i = i + 1;
                        ALocalLvalue iUse3 = new ALocalLvalue(new TIdentifier("i"));
                        ALocalLvalue iUse4 = new ALocalLvalue(new TIdentifier("i"));
                        ABinopExp binopExp = new ABinopExp(new ALvalueExp(iUse4), new APlusBinop(new TPlus("+")), new AIntConstExp(new TIntegerLiteral("1")));
                        AAssignmentExp assign = new AAssignmentExp(new TAssign("="), iUse3, binopExp);

                        //While (...){...}
                        AABlock innerWhile = new AABlock();
                        innerWhile.GetStatements().Add(new AExpStm(new TSemicolon(";"), deleteStructInvoke));
                        innerWhile.GetStatements().Add(new AExpStm(new TSemicolon(";"), assign));
                        AWhileStm whileStm = new AWhileStm(new TLParen("("), cond, new ABlockStm(new TLBrace("{"), innerWhile));

                        AABlock pBlock = (AABlock) node.Parent();
                        //pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(node), new ALocalDeclStm(new TSemicolon(";"), deleteMeVar));
                        pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(node), new ALocalDeclStm(new TSemicolon(";"), iVar));
                        pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(node), whileStm);
                        //visitMeNext.Add(deleteMeVar);
                        visitMeNext.Add(whileStm);

                        data.ExpTypes[iVar.GetInit()] =
                            data.LvalueTypes[iUse1] =
                            data.LvalueTypes[iUse2] =
                            data.LvalueTypes[iUse3] =
                            data.LvalueTypes[iUse4] =
                            data.ExpTypes[cond.GetLeft()] =
                            data.ExpTypes[lenghCall] =
                            data.ExpTypes[(PExp) intToString.GetArgs()[0]] =
                            data.ExpTypes[binopExp.GetLeft()] =
                            data.ExpTypes[binopExp.GetRight()] =
                            data.ExpTypes[binopExp] =
                            data.ExpTypes[assign] = new ANamedType(new TIdentifier("int"), null);
                        data.ExpTypes[(PExp) lenghCall.GetArgs()[0]] = 
                            data.ExpTypes[cond] = new ANamedType(new TIdentifier("bool"), null);
                        data.ExpTypes[lenghCall] =
                           // data.LvalueTypes[deleteMeUse1] =
                           // data.LvalueTypes[deleteMeUse2] =
                            data.ExpTypes[arrayLengthString.GetLeft()] =
                            data.ExpTypes[arrayLengthString.GetRight()] =
                            data.ExpTypes[arrayLengthString] =
                            data.ExpTypes[intToString] = 
                            data.ExpTypes[binopExp1] =
                            data.ExpTypes[binopExp1.GetLeft()] =
                            data.ExpTypes[binopExp1.GetRight()] =
                            data.ExpTypes[binopExp2] =
                            data.ExpTypes[binopExp3] =
                            data.ExpTypes[binopExp3.GetRight()] =
                            data.ExpTypes[lenghCall] = 
                            data.ExpTypes[lenghCall] = 
                            data.ExpTypes[lenghCall] = 
                            data.ExpTypes[lenghCall] = new ANamedType(new TIdentifier("string"), null);
                        data.ExpTypes[deleteStructInvoke] = new AVoidType(new TVoid("void"));

                        data.Locals[pBlock].Add(iVar);
                        //data.Locals[pBlock].Add(deleteMeVar);

                        data.LocalLinks[iUse1] =
                            data.LocalLinks[iUse2] =
                            data.LocalLinks[iUse3] =
                            data.LocalLinks[iUse4] = iVar;

                        //data.LocalLinks[deleteMeUse1] =
                        //    data.LocalLinks[deleteMeUse2] = deleteMeVar;

                        data.SimpleMethodLinks[lenghCall] =
                            data.Libraries.Methods.First(method => method.GetName().Text == lenghCall.GetName().Text);
                        data.SimpleMethodLinks[intToString] =
                            data.Libraries.Methods.First(method => method.GetName().Text == intToString.GetName().Text);

                        AStructDecl structDecl = data.StructTypeLinks[aBaseType];

                        if (!deleteStructMethod.ContainsKey(structDecl))
                            CreateDeleteStructMethod(node, structDecl, data);
                        data.SimpleMethodLinks[deleteStructInvoke] = deleteStructMethod[structDecl];
                    }

                    /*
                     * Convert delete <exp>
                     * to
                     * DeleteArray(<exp>);
                     * 
                     */ 

                    ASimpleInvokeExp invoke = new ASimpleInvokeExp(new TIdentifier("DeleteArray"), new ArrayList(){pointerString});
                    /*if (pointerString == null)
                    {
                        invoke.GetArgs().Add(node.GetExp());
                    }
                    else
                    {
                        ALocalLvalue local = new ALocalLvalue(new TIdentifier("pointerString"));
                        invoke.GetArgs().Add(new ALvalueExp(local));

                        data.LocalLinks[local] = pointerString;
                        data.LvalueTypes[local] =
                            data.ExpTypes[(PExp) invoke.GetArgs()[0]] = new ANamedType(new TIdentifier("string"), null);
                    }*/
                    data.ExpTypes[invoke] = new AVoidType(new TVoid("void"));
                    if (deleteArrayMethod == null)
                        CreateDeleteArrayMethod(node, data);
                    data.SimpleMethodLinks[invoke] = deleteArrayMethod;
                    visitMeNext.Add(invoke);

                    node.ReplaceBy(new AExpStm(new TSemicolon(";"), invoke));
                }
                else
                {
                    //Not array type
                    PExp pointerString = node.GetExp();
                    bool isIntPointer = Util.IsIntPointer(node, pointer.GetType(), data);
                    bool createdStructDelete = false;
                    if (Util.IsBulkCopy(pointer.GetType()))
                    {
                        node.GetExp().Apply(new MoveMethodDeclsOut("pointerVar", data));
                        pointerString = node.GetExp();

                        ANamedType aBaseType = (ANamedType) pointer.GetType();
                        /* Insert
                         * 
                         * string deleteMeVar = node.getExp();
                         * DeleteStruct<name>(deleteMeVar);
                         * 
                         */

                       /* AALocalDecl deleteMeVar = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null,
                                                                  new ANamedType(new TIdentifier("string"), null),
                                                                  new TIdentifier("deleteMe"), node.GetExp());*/
                        //pointerString = deleteMeVar;
                        //ALocalLvalue deleteMeUse = new ALocalLvalue(new TIdentifier("deleteMeVar"));

                        PExp deleteExp = Util.MakeClone(pointerString, data);
                        AStructDecl structDecl = data.StructTypeLinks[aBaseType];
                        if (isIntPointer)
                        {
                            GlobalStructVars vars = CreateStructFields(node, structDecl, data);
                            int allocateLimit = int.Parse(structDecl.GetIntDim().Text);
                            if (vars.IdentifierArray != null)
                            {
                                int usedBits = allocateLimit == 0
                                                   ? 0
                                                   : ((int) Math.Floor(Math.Log(allocateLimit, 2)) + 1);
                                int bitsLeft = 31 - usedBits;
                                int biggestIdentifier = (1 << (bitsLeft + 1)) - 1;

                                AIntConstExp bitsLeftConst = new AIntConstExp(new TIntegerLiteral(bitsLeft.ToString()));
                                deleteExp = new ABinopExp(deleteExp, new ARBitShiftBinop(new TRBitShift(">>")),
                                                          bitsLeftConst);

                                data.ExpTypes[bitsLeftConst] =
                                    data.ExpTypes[deleteExp] = new ANamedType(new TIdentifier("int"), null);
                            }
                        }

                        ASimpleInvokeExp deleteStructInvoke = new ASimpleInvokeExp(new TIdentifier("DeleteStruct" + aBaseType.AsIdentifierString()), new ArrayList());
                        deleteStructInvoke.GetArgs().Add(deleteExp);

                        AABlock pBlock = (AABlock)node.Parent();
                        //pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(node), new ALocalDeclStm(new TSemicolon(";"), deleteMeVar));
                        pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(node), new AExpStm(new TSemicolon(";"), deleteStructInvoke));
                        //visitMeNext.Add(deleteMeVar);
                        visitMeNext.Add(deleteStructInvoke);

                        /*data.ExpTypes[(PExp) deleteStructInvoke.GetArgs()[0]] =
                            data.LvalueTypes[deleteMeUse] = new ANamedType(new TIdentifier("string"), null);*/
                        data.ExpTypes[deleteStructInvoke] = new AVoidType(new TVoid("void"));

                        //data.Locals[pBlock].Add(deleteMeVar);


                        //data.LocalLinks[deleteMeUse] = deleteMeVar;


                        if (!deleteStructMethod.ContainsKey(structDecl))
                            CreateDeleteStructMethod(node, structDecl, data);
                        data.SimpleMethodLinks[deleteStructInvoke] = deleteStructMethod[structDecl];
                        createdStructDelete = true;
                    }
                    if (!isIntPointer)
                    {
                        /*
                         * Convert delete <exp>
                         * to
                         * DeleteSimple(<exp>);
                         * 
                         */

                        ASimpleInvokeExp invoke = new ASimpleInvokeExp(new TIdentifier("DeleteObject"),
                                                                       new ArrayList() {pointerString});
                        /*if (pointerString == null)
                        {
                            invoke.GetArgs().Add(node.GetExp());
                        }
                        else
                        {
                            ALocalLvalue local = new ALocalLvalue(new TIdentifier("pointerString"));
                            invoke.GetArgs().Add(new ALvalueExp(local));

                            data.LocalLinks[local] = pointerString;
                            data.LvalueTypes[local] =
                                data.ExpTypes[(PExp)invoke.GetArgs()[0]] = new ANamedType(new TIdentifier("string"), null);
                        }*/
                        data.ExpTypes[invoke] = new AVoidType(new TVoid("void"));
                        if (deleteObjectMethod == null)
                            CreateDeleteObjectMethod(node, data);
                        data.SimpleMethodLinks[invoke] = deleteObjectMethod;
                        visitMeNext.Add(invoke);

                        node.ReplaceBy(new AExpStm(new TSemicolon(";"), invoke));
                    }
                    else if (createdStructDelete)
                    {
                        node.Parent().RemoveChild(node);
                    }
                    else
                    {
                        //There is an enrichment

                        PExp deleteExp = pointerString;
                        AEnrichmentDecl enrichmentDecl = data.EnrichmentTypeLinks[pointer.GetType()];
                        if (isIntPointer)
                        {
                            GlobalStructVars vars = CreateEnrichmentFields(node, enrichmentDecl, data);
                            int allocateLimit = int.Parse(enrichmentDecl.GetIntDim().Text);
                            if (vars.IdentifierArray != null)
                            {
                                int usedBits = allocateLimit == 0
                                                   ? 0
                                                   : ((int)Math.Floor(Math.Log(allocateLimit, 2)) + 1);
                                int bitsLeft = 31 - usedBits;

                                AIntConstExp bitsLeftConst = new AIntConstExp(new TIntegerLiteral(bitsLeft.ToString()));
                                deleteExp = new ABinopExp(deleteExp, new ARBitShiftBinop(new TRBitShift(">>")),
                                                          bitsLeftConst);

                                data.ExpTypes[bitsLeftConst] =
                                    data.ExpTypes[deleteExp] = new ANamedType(new TIdentifier("int"), null);
                            }
                        }

                        ASimpleInvokeExp invoke = new ASimpleInvokeExp(new TIdentifier("DeleteObject"),
                                                                       new ArrayList() { deleteExp });

                        data.ExpTypes[invoke] = new AVoidType(new TVoid("void"));
                        data.SimpleMethodLinks[invoke] = CreateDeleteEnrichmentMethod(node, enrichmentDecl, data);
                        visitMeNext.Add(invoke);

                        node.ReplaceBy(new AExpStm(new TSemicolon(";"), invoke));
                
                    }
                }
                bool hadPointerPreviously = hadPointer;
                PExp previosNameExp = nameExp;
                for (int i = 0; i < visitMeNext.Count; i++)
                {
                    hadPointer = false;
                    nameExp = null;
                    visitMeNext[i].Apply(this);
                }
                hadPointer = hadPointerPreviously;
                nameExp = previosNameExp;
            }

            public override void CaseAPArrayLengthLvalue(APArrayLengthLvalue node)
            {
                base.CaseAPArrayLengthLvalue(node);
                ABinopExp binopExp = new ABinopExp(nameExp, new APlusBinop(new TPlus("+")), new AStringConstExp(new TStringLiteral("\"\\\\Length\"")));
                ABooleanConstExp trueConst = new ABooleanConstExp(new ATrueBool());
                ASimpleInvokeExp invoke = new ASimpleInvokeExp(new TIdentifier("DataTableGetInt"), new ArrayList(){trueConst, binopExp});
                ALvalueExp parent = (ALvalueExp) node.Parent();
                parent.ReplaceBy(invoke);

                data.ExpTypes[binopExp] =
                    data.ExpTypes[binopExp.GetRight()] = new ANamedType(new TIdentifier("string"), null);

                data.ExpTypes[trueConst] = new ANamedType(new TIdentifier("bool"), null);

                data.ExpTypes[invoke] = new ANamedType(new TIdentifier("int"), null);

                data.SimpleMethodLinks[invoke] =
                    data.Libraries.Methods.First(m => m.GetName().Text == invoke.GetName().Text);

                nameExp = null;
                hadPointer = false;
            }

            public override void CaseAArrayResizeExp(AArrayResizeExp node)
            {
                PType baseType = ((ADynamicArrayType)data.ExpTypes[node.GetBase()]).GetType();
                node.GetBase().Apply(this);
                AMethodDecl method = CreateResizeArrayMethod(node, baseType, data);
                ASimpleInvokeExp invoke = new ASimpleInvokeExp(new TIdentifier(method.GetName().Text), new ArrayList(){node.GetArg(), nameExp});
                node.ReplaceBy(invoke);
                data.ExpTypes[invoke] = new AVoidType(new TVoid("void"));
                data.SimpleMethodLinks[invoke] = method;
                nameExp = null;
                hadPointer = false;
                invoke.Apply(this);
            }


            public override void CaseAFieldDecl(AFieldDecl node)
            {
                PType type = node.GetType();
                if (node.GetInit() is ANullExp &&
                    type is APointerType &&
                    Util.IsIntPointer(node, ((APointerType)type).GetType(), data))
                {
                    AIntConstExp replacer = new AIntConstExp(new TIntegerLiteral("0"));
                    data.ExpTypes[replacer] = new ANamedType(new TIdentifier("int"), null);
                    node.GetInit().ReplaceBy(replacer);
                }

                base.CaseAFieldDecl(node);
            }

            public override void CaseAAssignmentExp(AAssignmentExp node)
            {

                if (node.GetExp() is ANewExp)
                {
                    node.GetExp().Apply(this);
                }

                bool rightIsDynamic = false;
                PExp rightString = null;
                bool rightWasSet = assignmentRightSideSet;
                assignmentRightSideSet = false;
                if (rightWasSet)
                {
                    rightIsDynamic = hadPointer;
                    rightString = nameExp;
                }
                

                hadPointer = false;
                nameExp = null;
                node.GetLvalue().Apply(this);
                bool leftIsDynamic = hadPointer;
                PExp leftString = nameExp;

                if (!rightWasSet)
                {
                    hadPointer = false;
                    nameExp = null;
                    node.GetExp().Apply(this);
                    rightIsDynamic = hadPointer;
                    rightString = nameExp;
                }

                PType type = data.ExpTypes[node];
                if (node.GetExp() is ANullExp &&
                            type is APointerType &&
                            Util.IsIntPointer(node, ((APointerType)type).GetType(), data))
                {
                    AIntConstExp replacer = new AIntConstExp(new TIntegerLiteral("0"));
                    data.ExpTypes[replacer] = new ANamedType(new TIdentifier("int"), null);
                    node.GetExp().ReplaceBy(replacer);
                }
                /* Cases to handle
                 * 
                 * left dynamic, right dynamic, type simple / left dynamic, right normal, type simple
                 * left dynamic, right dynamic, type bulk
                 * left dynamic, right simple, type bulk
                 * left simple, right dynamic, type bulk
                 * 
                 * 
                 */ 
                /*if (!Util.IsBulkCopy(type))
                {//left dynamic, right dynamic, type simple / left dynamic, right normal, type simple
                    if (leftIsDynamic)
                    {
                        string typeString = "string";
                        if (type is ANamedType)
                        {
                            typeString = ((ANamedType) type).GetName().Text;
                        }
                        PExp exp = CreateDynaicSetStm(typeString, leftString, node.GetExp());

                        node.ReplaceBy(exp);
                    }
                }
                else*/
                {
                    //throw new ParserException(null, "Not implemented");
                    if (type is ANamedType)
                    {
                        
                    }
                    else if (type is AArrayTempType || type is ADynamicArrayType)
                    {
                         
                    }
                    PStm pStm = Util.GetAncestor<PStm>(node);
                    AABlock pBlock = (AABlock) pStm.Parent();
                    int index = pBlock.GetStatements().IndexOf(pStm) + 1;
                    if (leftIsDynamic)
                    {
                        
                        

                        if (rightIsDynamic)
                        {//left dynamic, right dynamic, type bulk
                            MakeAssignmentBothDynamic(leftString, rightString, type, pBlock, ref index);
                            pStm.Parent().RemoveChild(pStm);
                        }
                        else
                        {//left dynamic, right simple, type bulk
                            MakeAssignmentLeftDynamic(leftString, node.GetExp(), type, pBlock, ref index);
                            pStm.Parent().RemoveChild(pStm);
                        }
                    }
                    else if (rightIsDynamic)
                    {//left simple, right dynamic, type bulk
                        MakeAssignmentRightDynamic(node.GetLvalue(), rightString, type, pBlock, ref index);
                        pStm.Parent().RemoveChild(pStm);
                    }
                }
            }

            public override void CaseABinopExp(ABinopExp node)
            {
                if (node.GetBinop() is AEqBinop && node.GetRight() is ANullExp && data.ExpTypes[node.GetLeft()] is APointerType)
                {
                    //We have a null check
                    APointerType pointerType = (APointerType)data.ExpTypes[node.GetLeft()];
                    AMethodDecl nullCheckMethod = CreateNullCheckMethod(node, pointerType.GetType(), data);

                    ASimpleInvokeExp invoke = new ASimpleInvokeExp(new TIdentifier("IsNull"), new ArrayList(){node.GetLeft()});
                    node.ReplaceBy(invoke);

                    data.ExpTypes[invoke] = new ANamedType(new TIdentifier("bool"), null);
                    data.SimpleMethodLinks[invoke] = nullCheckMethod;
                    CaseASimpleInvokeExp(invoke);
                    return;
                }
                base.CaseABinopExp(node);
            }


            private void MakeAssignmentBothDynamic(PExp leftSide, PExp rightSide, PType type, AABlock block, ref int index)
            {
                if (Util.IsBulkCopy(type))
                {
                    if (type is ANamedType)
                    {
                        ANamedType aType = (ANamedType)type;
                        AStructDecl structDecl = data.StructTypeLinks[aType];
                        foreach (AALocalDecl localDecl in structDecl.GetLocals())
                        {
                            ABinopExp newleftSide = new ABinopExp(leftSide, new APlusBinop(new TPlus("+")),
                                                     new AStringConstExp(
                                                         new TStringLiteral("\"." + localDecl.GetName().Text + "\"")));
                            ABinopExp newrightSide = new ABinopExp(rightSide, new APlusBinop(new TPlus("+")),
                                                     new AStringConstExp(
                                                         new TStringLiteral("\"." + localDecl.GetName().Text + "\"")));
                            data.ExpTypes[newleftSide] =
                                data.ExpTypes[newrightSide] =
                                data.ExpTypes[newleftSide.GetRight()] =
                                data.ExpTypes[newrightSide.GetRight()] =
                                new ANamedType(new TIdentifier("string"), null);

                            MakeAssignmentBothDynamic(newleftSide, newrightSide, localDecl.GetType(), block, ref index);
                        }
                    }
                    else
                    {//Is array type. Can Only be a constant array type
                        AArrayTempType aType = (AArrayTempType) type;
                        for (int i = 0; i < int.Parse(aType.GetIntDim().Text); i++)
                        {
                            ABinopExp newleftSide = new ABinopExp(leftSide, new APlusBinop(new TPlus("+")),
                                                     new AStringConstExp(
                                                         new TStringLiteral("\"[" + i + "]\"")));
                            ABinopExp newrightSide = new ABinopExp(rightSide, new APlusBinop(new TPlus("+")),
                                                     new AStringConstExp(
                                                         new TStringLiteral("\"[" + i + "]\"")));
                            data.ExpTypes[newleftSide] =
                                data.ExpTypes[newrightSide] =
                                data.ExpTypes[newleftSide.GetRight()] =
                                data.ExpTypes[newrightSide.GetRight()] =
                                new ANamedType(new TIdentifier("string"), null);

                            MakeAssignmentBothDynamic(newleftSide, newrightSide, aType.GetType(), block, ref index);

                            
                        }

                    }
                }
                else
                {
                    //ANamedType aType = type is APointerType ? new ANamedType(new TIdentifier("string"), null) : (ANamedType) type;
                    ANamedType aType;// = type is APointerType ? new ANamedType(new TIdentifier("string"), null) : (ANamedType)type;
                    if (type is APointerType)
                    {
                        if (Util.IsIntPointer(type, ((APointerType)type).GetType(), data))
                            aType = new ANamedType(new TIdentifier("int"), null);
                        else
                            aType = new ANamedType(new TIdentifier("string"), null);
                    }
                    else
                    {
                        aType = (ANamedType)type;
                    }
                    string capitalType = Util.Capitalize(aType.AsIdentifierString());//Char.ToUpper(aType.AsIdentifierString()[0]) + aType.AsIdentifierString().Substring(1);
                    leftSide = Util.MakeClone(leftSide, data);
                    rightSide = Util.MakeClone(rightSide, data);

                    ABooleanConstExp trueConst1 = new ABooleanConstExp(new ATrueBool());
                    ABooleanConstExp trueConst2 = new ABooleanConstExp(new ATrueBool());
                    ASimpleInvokeExp innerInvoke = new ASimpleInvokeExp(new TIdentifier("DataTableGet" + capitalType), new ArrayList(){trueConst1, rightSide});
                    ASimpleInvokeExp outerInvoke = new ASimpleInvokeExp(new TIdentifier("DataTableSet" + capitalType), new ArrayList() { trueConst2, leftSide, innerInvoke });
                    block.GetStatements().Insert(index, new AExpStm(new TSemicolon(";"), outerInvoke));
                    index++;

                    data.ExpTypes[trueConst1] =
                        data.ExpTypes[trueConst2] = new ANamedType(new TIdentifier("bool"), null);

                    data.ExpTypes[innerInvoke] = aType;
                    data.ExpTypes[outerInvoke] = new AVoidType(new TVoid("void"));

                    data.SimpleMethodLinks[innerInvoke] =
                        data.Libraries.Methods.First(m => m.GetName().Text == innerInvoke.GetName().Text);
                    data.SimpleMethodLinks[outerInvoke] =
                        data.Libraries.Methods.First(m => m.GetName().Text == outerInvoke.GetName().Text);
                }
            }

            private void MakeAssignmentLeftDynamic(PExp leftSide, PExp rightSide, PType type, AABlock block, ref int index)
            {
                if (Util.IsBulkCopy(type))
                {
                    if (type is ANamedType)
                    {
                        ANamedType aType = (ANamedType)type;
                        AStructDecl structDecl = data.StructTypeLinks[aType];
                        foreach (AALocalDecl localDecl in structDecl.GetLocals())
                        {
                            ABinopExp newleftSide = new ABinopExp(leftSide, new APlusBinop(new TPlus("+")),
                                                   new AStringConstExp(
                                                       new TStringLiteral("\"." + localDecl.GetName().Text + "\"")));
                            ALvalueExp newrightSide =
                                new ALvalueExp(new AStructLvalue(rightSide, new ADotDotType(new TDot(".")),
                                                                 new TIdentifier(localDecl.GetName().Text)));
                            data.ExpTypes[newleftSide] =
                                data.ExpTypes[newrightSide] =
                                data.ExpTypes[newleftSide.GetRight()] =
                                new ANamedType(new TIdentifier("string"), null);

                            data.ExpTypes[newrightSide] =
                                data.LvalueTypes[newrightSide.GetLvalue()] = localDecl.GetType();

                            data.StructFieldLinks[(AStructLvalue) newrightSide.GetLvalue()] = localDecl;

                            MakeAssignmentLeftDynamic(newleftSide, newrightSide, localDecl.GetType(), block, ref index);
                        }
                    }
                    else
                    {//Is array type. Can Only be a constant array type
                        AArrayTempType aType = (AArrayTempType)type;
                        for (int i = 0; i < int.Parse(aType.GetIntDim().Text); i++)
                        {
                            ABinopExp newleftSide = new ABinopExp(leftSide, new APlusBinop(new TPlus("+")),
                                                     new AStringConstExp(
                                                         new TStringLiteral("\"[" + i + "]\"")));
                            AArrayLvalue rightLvalue = new AArrayLvalue(new TLBracket("["), rightSide, new AIntConstExp(new TIntegerLiteral(i.ToString())));
                            ALvalueExp newrightSide = new ALvalueExp(rightLvalue);

                            data.ExpTypes[newleftSide] =
                                data.ExpTypes[newrightSide] =
                                data.ExpTypes[newleftSide.GetRight()] =
                                new ANamedType(new TIdentifier("string"), null);

                            data.ExpTypes[newrightSide] = data.LvalueTypes[rightLvalue] = aType.GetType();
                            data.ExpTypes[rightLvalue.GetIndex()] = new ANamedType(new TIdentifier("int"), null);



                            MakeAssignmentLeftDynamic(newleftSide, newrightSide, aType.GetType(), block, ref index);


                        }

                    }
                }
                else
                {
                    ANamedType aType = type is APointerType ? new ANamedType(new TIdentifier(Util.IsIntPointer(type, ((APointerType)type).GetType(), data) ? "int" : "string"), null) : (ANamedType)type;
                    string capitalType = Util.Capitalize(aType.AsIdentifierString());// Char.ToUpper(aType.GetName().Text[0]) + aType.GetName().Text.Substring(1);
                    leftSide = Util.MakeClone(leftSide, data);
                    rightSide = Util.MakeClone(rightSide, data);

                    ABooleanConstExp trueConst1 = new ABooleanConstExp(new ATrueBool());
                    ABooleanConstExp trueConst2 = new ABooleanConstExp(new ATrueBool());
                    ASimpleInvokeExp outerInvoke = new ASimpleInvokeExp(new TIdentifier("DataTableSet" + capitalType), new ArrayList() { trueConst2, leftSide, rightSide });
                    block.GetStatements().Insert(index, new AExpStm(new TSemicolon(";"), outerInvoke));
                    index++;

                    data.ExpTypes[trueConst1] =
                        data.ExpTypes[trueConst2] = new ANamedType(new TIdentifier("bool"), null);

                    data.ExpTypes[outerInvoke] = new AVoidType(new TVoid("void"));


                    data.SimpleMethodLinks[outerInvoke] =
                        data.Libraries.Methods.First(m => m.GetName().Text == outerInvoke.GetName().Text);
                }
            }

            


            private void MakeAssignmentRightDynamic(PLvalue leftSide, PExp rightSide, PType type, AABlock block, ref int index)
            {
                if (Util.IsBulkCopy(type))
                {
                    if (type is ANamedType)
                    {
                        ANamedType aType = (ANamedType)type;
                        AStructDecl structDecl = data.StructTypeLinks[aType];
                        foreach (AALocalDecl localDecl in structDecl.GetLocals())
                        {
                            AStructLvalue newleftSide = new AStructLvalue(new ALvalueExp(leftSide),
                                                                          new ADotDotType(new TDot(".")),
                                                                          new TIdentifier(localDecl.GetName().Text));

                            ABinopExp newrightSide = new ABinopExp(rightSide, new APlusBinop(new TPlus("+")),
                                                     new AStringConstExp(
                                                         new TStringLiteral("\"." + localDecl.GetName().Text + "\"")));
                           
                             data.ExpTypes[newrightSide] =
                                data.ExpTypes[newrightSide.GetRight()] =
                                new ANamedType(new TIdentifier("string"), null);

                            data.ExpTypes[newleftSide.GetReceiver()] = type;
                            data.LvalueTypes[newleftSide] = localDecl.GetType();

                            data.StructFieldLinks[newleftSide] = localDecl;

                            MakeAssignmentRightDynamic(newleftSide, newrightSide, localDecl.GetType(), block, ref index);
                        }
                    }
                    else
                    {//Is array type. Can Only be a constant array type
                        AArrayTempType aType = (AArrayTempType)type;
                        for (int i = 0; i < int.Parse(aType.GetIntDim().Text); i++)
                        {
                            AArrayLvalue newleftSide = new AArrayLvalue(new TLBracket("["), new ALvalueExp(leftSide), new AIntConstExp(new TIntegerLiteral(i.ToString())));
                            
                            ABinopExp newrightSide = new ABinopExp(rightSide, new APlusBinop(new TPlus("+")),
                                                     new AStringConstExp(
                                                         new TStringLiteral("\"[" + i + "]\"")));
                            data.ExpTypes[newrightSide] =
                                data.ExpTypes[newrightSide.GetRight()] =
                                new ANamedType(new TIdentifier("string"), null);


                            data.ExpTypes[newleftSide.GetBase()] = type;
                            data.ExpTypes[newleftSide.GetIndex()] = new ANamedType(new TIdentifier("int"), null);
                            data.LvalueTypes[newleftSide] = aType.GetType();

                            MakeAssignmentRightDynamic(newleftSide, newrightSide, aType.GetType(), block, ref index);


                        }

                    }
                }
                else
                {
                    ANamedType aType;// = type is APointerType ? new ANamedType(new TIdentifier("string"), null) : (ANamedType)type;
                    if (type is APointerType)
                    {
                        if (Util.IsIntPointer(type, ((APointerType)type).GetType(), data))
                            aType = new ANamedType(new TIdentifier("int"), null);
                        else
                            aType = new ANamedType(new TIdentifier("string"), null);
                    }
                    else
                    {
                        aType = (ANamedType) type;
                    }
                    string capitalType = Util.Capitalize(aType.AsIdentifierString());//Char.ToUpper(aType.GetName().Text[0]) + aType.GetName().Text.Substring(1);
                    leftSide = Util.MakeClone(leftSide, data);
                    rightSide = Util.MakeClone(rightSide, data);

                    ABooleanConstExp trueConst1 = new ABooleanConstExp(new ATrueBool());
                    //ABooleanConstExp trueConst2 = new ABooleanConstExp(new ATrueBool());
                    ASimpleInvokeExp innerInvoke = new ASimpleInvokeExp(new TIdentifier("DataTableGet" + capitalType), new ArrayList() { trueConst1, rightSide });
                    //ASimpleInvokeExp outerInvoke = new ASimpleInvokeExp(new TIdentifier("DataTableSet" + capitalType), new ArrayList() { trueConst2, leftSide, innerInvoke });
                    AAssignmentExp assignment = new AAssignmentExp(new TAssign("="), leftSide, innerInvoke);
                    block.GetStatements().Insert(index, new AExpStm(new TSemicolon(";"), assignment));
                    index++;

                    data.ExpTypes[trueConst1] = new ANamedType(new TIdentifier("bool"), null);

                    data.ExpTypes[innerInvoke] = aType;
                    data.ExpTypes[assignment] = aType;

                    data.SimpleMethodLinks[innerInvoke] =
                        data.Libraries.Methods.First(m => m.GetName().Text == innerInvoke.GetName().Text);
                }
            }



            public override void DefaultIn(Node node)
            {
                if (node is PLvalue)
                    hadPointer = false;
                base.DefaultIn(node);
            }


            public override void CaseAPointerLvalue(APointerLvalue node)
            {
                hadPointer = false;
                nameExp = null;
                base.CaseAPointerLvalue(node);
                if (Util.IsIntPointer(node, data.LvalueTypes[node], data))
                {
                    if (nameExp != null)
                    {
                        //Create a data table get string exp
                        nameExp = CreateDynaicGetStm("int");
                    }
                    else
                    {
                        nameExp = node.GetBase();
                    }
                    //Replace by str_Array[<base>];
                    //If this is a compared pointer, replace it by str_Array[<base> >> <<bitsLeft>>]
                    GlobalStructVars vars;
                    int allocateLimit;
                    if (data.EnrichmentTypeLinks.ContainsKey(data.LvalueTypes[node]))
                    {
                        AEnrichmentDecl enrichmentDecl = data.EnrichmentTypeLinks[data.LvalueTypes[node]];
                        vars = CreateEnrichmentFields(node, enrichmentDecl, data);
                        allocateLimit = int.Parse(enrichmentDecl.GetIntDim().Text);
                    }
                    else
                    {
                        AStructDecl structDecl = data.StructTypeLinks[(ANamedType)data.LvalueTypes[node]];
                        vars = CreateStructFields(node, structDecl, data);
                        allocateLimit = int.Parse(structDecl.GetIntDim().Text);
                    }
                    
                    AFieldLvalue array = new AFieldLvalue(new TIdentifier(vars.Array.GetName().Text));
                    ALvalueExp arrayExp = new ALvalueExp(array);

                    PExp arrayIndex = nameExp;
                    if (vars.IdentifierArray != null)
                    {
                        int usedBits = allocateLimit == 0 ? 0 : ((int)Math.Floor(Math.Log(allocateLimit, 2)) + 1);
                        int bitsLeft = 31 - usedBits;
                        int biggestIdentifier = (1 << (bitsLeft + 1)) - 1;
                        
                        AIntConstExp bitsLeftConst = new AIntConstExp(new TIntegerLiteral(bitsLeft.ToString()));
                        arrayIndex = new ABinopExp(arrayIndex, new ARBitShiftBinop(new TRBitShift(">>")), bitsLeftConst);

                        data.ExpTypes[bitsLeftConst] =
                            data.ExpTypes[arrayIndex] = new ANamedType(new TIdentifier("int"), null);
                    }

                    AArrayLvalue replacer = new AArrayLvalue(new TLBracket("["), arrayExp, arrayIndex);
                    node.ReplaceBy(replacer);

                    data.FieldLinks[array] = vars.Array;
                    data.LvalueTypes[array] = data.ExpTypes[arrayExp] = vars.Array.GetType();
                    data.LvalueTypes[replacer] = ((AArrayTempType) vars.Array.GetType()).GetType();

                    hadPointer = false;
                    nameExp = null;
                    return;
                }
                //if (Util.GetAncestor<AAssignmentExp>(node) != null || Util.GetAncestor<APArrayLengthLvalue>(node) != null)
                {
                    if (nameExp != null)
                    {
                        //Create a data table get string exp

                        nameExp = CreateDynaicGetStm("string");
                    }
                    else
                    {
                        nameExp = node.GetBase();
                    }
                }
                hadPointer = true;
                CheckDynamicLvalue(node);
                //Todo: Insert runtime check that the pointer is actually not null / points to a delete object
                //(unless the same pointer was checked before, and it has not been assigned to since, and there has not been a method call since.
                
            }



            public override void CaseAStructLvalue(AStructLvalue node)
            {
                hadPointer = false;
                base.CaseAStructLvalue(node);
                if (hadPointer)
                {
                    //if (Util.GetAncestor<AAssignmentExp>(node) != null || Util.GetAncestor<APArrayLengthLvalue>(node) != null)
                    {
                        ABinopExp binopExp = new ABinopExp(nameExp, new APlusBinop(new TPlus("+")),
                                                           new AStringConstExp(
                                                               new TStringLiteral("\"." + node.GetName().Text + "\"")));
                        data.ExpTypes[binopExp] =
                            data.ExpTypes[binopExp.GetRight()] = new ANamedType(new TIdentifier("string"), null);
                        nameExp = binopExp;
                    }
                    CheckDynamicLvalue(node);
                }
            }

            public override void CaseAArrayLvalue(AArrayLvalue node)
            {
                hadPointer = false;
                node.GetIndex().Apply(this);
                if (hadPointer)
                {//The index is a dynamic int
                    string typeName = ((ANamedType) data.ExpTypes[node.GetIndex()]).AsString();
                    node.GetIndex().ReplaceBy(CreateDynaicGetStm(typeName));
                }
                node.GetBase().Apply(this);
                if (hadPointer)
                {

                    //if (Util.GetAncestor<AAssignmentExp>(node) != null || Util.GetAncestor<APArrayLengthLvalue>(node) != null)
                    {
                        //Todo: Check if the index is within array (runtime)

                        ASimpleInvokeExp intToString = new ASimpleInvokeExp(new TIdentifier("IntToString"),
                                                                            new ArrayList());
                        intToString.GetArgs().Add(Util.MakeClone(node.GetIndex(), data));

                        ABinopExp binopExp1 = new ABinopExp(nameExp, new APlusBinop(new TPlus("+")), new AStringConstExp(new TStringLiteral("\"[\"")));
                        ABinopExp binopExp2 = new ABinopExp(binopExp1, new APlusBinop(new TPlus("+")), intToString);
                        ABinopExp binopExp3 = new ABinopExp(binopExp2, new APlusBinop(new TPlus("+")), new AStringConstExp(new TStringLiteral("\"]\"")));

                        data.ExpTypes[binopExp1] =
                        data.ExpTypes[binopExp2] =
                        data.ExpTypes[binopExp3] =
                        data.ExpTypes[binopExp1.GetRight()] =
                        data.ExpTypes[binopExp3.GetRight()] =
                            data.ExpTypes[intToString] =new ANamedType(new TIdentifier("string"), null);
                        data.SimpleMethodLinks[intToString] =
                            data.Libraries.Methods.First(method => method.GetName().Text == intToString.GetName().Text);
                        nameExp = binopExp3;
                    }
                    CheckDynamicLvalue(node);
                }
            }

            bool assignmentRightSideSet = false;
            private void CheckDynamicLvalue(PLvalue node)
            {
                if (node.Parent() is ALvalueExp)
                {
                    ALvalueExp parent = (ALvalueExp)node.Parent();
                    if (parent.Parent() is AAssignmentExp)
                        return;
                    if (!(parent.Parent() is PLvalue))
                    {//Then this should be a data table get
                        //If this is a bulk copy type, move it out to a new variable.
                        //var pointerVar;
                        //pointerVar = dataTableGet(...)
                        //...
                        PType type = data.ExpTypes[parent];
                        if (Util.IsBulkCopy(type))
                        {
                            PStm pStm = Util.GetAncestor<PStm>(node);
                            AABlock pBlock = (AABlock)pStm.Parent();
                            AAssignmentExp assignment;
                            if (parent.Parent() is AALocalDecl)
                            {
                                //Turn into assignment
                                ALocalLvalue leftSide = new ALocalLvalue(new TIdentifier("tempName"));
                                data.LocalLinks[leftSide] = (AALocalDecl) parent.Parent();
                                assignment = new AAssignmentExp(new TAssign("="), leftSide, parent);


                                pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm) + 1, new AExpStm(new TSemicolon(";"), assignment));

                                data.ExpTypes[assignment] =  data.LvalueTypes[leftSide] = data.LocalLinks[leftSide].GetType();
                                assignment.Apply(this);
                                return;
                            }


                            pStm = Util.GetAncestor<PStm>(node);
                            AALocalDecl localDecl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, Util.MakeClone(type, data), new TIdentifier("pointerVar"), null);
                            ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier("pointerVar"));
                            ALocalLvalue lvalue2 = new ALocalLvalue(new TIdentifier("pointerVar"));
                            ALvalueExp lvalue2Exp = new ALvalueExp(lvalue2);
                            parent.ReplaceBy(lvalue2Exp);
                            assignment = new AAssignmentExp(new TAssign("="), lvalue, parent);
                            pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm), new ALocalDeclStm(new TSemicolon(";"), localDecl));
                            pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm), new AExpStm(new TSemicolon(";"), assignment));
                            

                            data.LocalLinks[lvalue] = localDecl;
                            data.LocalLinks[lvalue2] = localDecl;
                            data.ExpTypes[assignment] = data.ExpTypes[lvalue2Exp] = data.LvalueTypes[lvalue] = data.LvalueTypes[lvalue2] = type;

                            assignmentRightSideSet = true;
                            assignment.Apply(this);
                        }
                        else
                        {
                            string s = "string";
                            if (type is ANamedType)
                                s = ((ANamedType) type).AsIdentifierString();
                            else if (type is APointerType && Util.IsIntPointer(node, ((APointerType)type).GetType(), data))
                            {
                                s = "int";
                            }
                            parent.ReplaceBy(CreateDynaicGetStm(s));
                        }
                        hadPointer = false;
                    }
                }
            }

            private PExp CreateDynaicGetStm(string type)
            {
                string upperType = Util.Capitalize(type);//char.ToUpper(type[0]) + type.Substring(1);
                
                //Create a data table get string exp
                ASimpleInvokeExp invoke = new ASimpleInvokeExp(new TIdentifier("DataTableGet" + upperType), new ArrayList());
                invoke.GetArgs().Add(new ABooleanConstExp(new ATrueBool()));
                invoke.GetArgs().Add(nameExp);

                data.ExpTypes[invoke] = new ANamedType(new TIdentifier(type), null);
                data.ExpTypes[(PExp)invoke.GetArgs()[0]] = new ANamedType(new TIdentifier("bool"), null);
                data.SimpleMethodLinks[invoke] =
                    data.Libraries.Methods.First(method => method.GetName().Text == invoke.GetName().Text);

                return invoke;
            }

            private PExp CreateDynaicSetStm(string type, PExp target, PExp arg)
            {
                string upperType = char.ToUpper(type[0]) + type.Substring(1);

                //Create a data table get string exp
                ASimpleInvokeExp invoke = new ASimpleInvokeExp(new TIdentifier("DataTableSet" + upperType), new ArrayList());
                invoke.GetArgs().Add(new ABooleanConstExp(new ATrueBool()));
                invoke.GetArgs().Add(target);
                invoke.GetArgs().Add(arg);

                data.ExpTypes[invoke] = new AVoidType(new TVoid("void"));
                data.ExpTypes[(PExp)invoke.GetArgs()[0]] = new ANamedType(new TIdentifier("bool"), null);
                data.SimpleMethodLinks[invoke] =
                    data.Libraries.Methods.First(method => method.GetName().Text == invoke.GetName().Text);

                return invoke;
            }

            public override void CaseAMethodDecl(AMethodDecl node)
            {
                //Remove ref and out
                foreach (AALocalDecl formal in node.GetFormals())
                {
                    formal.SetRef(null);
                    formal.SetOut(null);
                }
                base.CaseAMethodDecl(node);
            }
        }

       

        /*
         * a Rename all types to string 
         * b Transform new into a method call that returns a unique string, and sets \exists
         * c Transform delete into a method that removes all entries of the type (depending on type)
         *      One for each type of struct
         *      One for arrays
         *      One for single pointers
         * d Transform (*a)[b] into a get/set request on a+"[" + IntToString(b) + "]"
         * e Transform (*a).b.c into a get/set request on a+".b.c"
         * 
         * 
         * you can only make new expressions matching
         *  new int()//Single
         *  new int*()//Single
         *  new int[2]()//Array, where each element 
         *  new int*[2]()//Array
         *  
         *  new int[2][3]() doesnt make sence, since it will be a p->dt ar->dt ar, so p = new int[3], for(i = 0..2) p[i]= new int[2]()
         *  new int**() doesnt make sence since p->dt->dt->dt is p = new int*(), *p = new int();
         *  
         * but ofcourse, the user can still make theese things - they just don't really exist at runtime
         * 
         * 
         * 
         * 
         * *(a.b)   =>  DataTable(a.b, ...
         * (*a).b   =>  DataTable(a + ".b", ...
         * (*a).b.c =>  DataTable(a + ".b" + ".c", ...
         * (*a).b[c]=>  DataTable(a + ".b" + "[" + b + "]", ...
         * (*a)[b]  =>  DataTable(a + "[" + b + "]". ...    
         * *(*(a))  =>  DataTable(DataTableGet(a), ...
         * *(*(a.b).c).d  =>  DataTable(DataTableGet(a.b + ".c") + ".d", ...
         *  
         */

        
    }
}

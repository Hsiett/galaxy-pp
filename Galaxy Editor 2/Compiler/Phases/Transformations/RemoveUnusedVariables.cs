using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class RemoveUnusedVariables : DepthFirstAdapter
    {
        private FinalTransformations finalTrans;

        public RemoveUnusedVariables(FinalTransformations finalTrans)
        {
            this.finalTrans = finalTrans;
            unusedMethods.AddRange(finalTrans.data.Methods.Select((declItem) => declItem.Decl));
            foreach (SharedData.DeclItem<AFieldDecl> declItem in finalTrans.data.Fields)
            {
                assignedToFields[declItem.Decl] = new List<AAssignmentExp>();
            }
        }

        List<AALocalDecl> definedLocals = new List<AALocalDecl>();
        List<AALocalDecl> usedLocals = new List<AALocalDecl>();
        Dictionary<AALocalDecl, List<AAssignmentExp>> assignedToLocals = new Dictionary<AALocalDecl, List<AAssignmentExp>>();

        List<AMethodDecl> unusedMethods = new List<AMethodDecl>();

        List<AFieldDecl> fieldsWithMethodCalls = new List<AFieldDecl>();

        private List<AFieldDecl> usedFields = new List<AFieldDecl>();
        List<AFieldDecl> reportedFields = new List<AFieldDecl>();
        Dictionary<AFieldDecl, List<AAssignmentExp>> assignedToFields = new Dictionary<AFieldDecl, List<AAssignmentExp>>();

        List<AStructDecl> usedStructs = new List<AStructDecl>();

        private bool processFieldsOnly, processMethodsOnly, processStructs;

        private bool firstMethodRun = true;
        private bool firstFieldRun = true;
        private bool firstStructRun = true;
        List<ErrorCollection.Error> children = new List<ErrorCollection.Error>();
        public override void OutAAProgram(AAProgram node)
        {
            if (!processStructs)
            {
                if (!processFieldsOnly)
                {
                    unusedMethods.RemoveAll(method => method.GetTrigger() != null);
                    
                    if (finalTrans.mainEntry != null) unusedMethods.Remove(finalTrans.mainEntry);
                    if (finalTrans.data.DeobfuscateMethod != null) unusedMethods.Remove(finalTrans.data.DeobfuscateMethod);
                    foreach (AMethodDecl unusedMethod in unusedMethods)
                    {
                        if (firstMethodRun && finalTrans.data.UserMethods.Contains(unusedMethod))
                            children.Add(new ErrorCollection.Error(unusedMethod.GetName(),
                                                                        Util.GetAncestor<AASourceFile>(unusedMethod),
                                                                        "Unused method: " + unusedMethod.GetName().Text, true));
                        if (Options.Compiler.RemoveUnusedMethods)
                            unusedMethod.Parent().RemoveChild(unusedMethod);
                    }
                    firstMethodRun = false;
                    if (Options.Compiler.RemoveUnusedMethods)
                    {
                        finalTrans.data.Methods.RemoveAll(declItem => unusedMethods.Contains(declItem.Decl));
                        if (unusedMethods.Count > 0)
                        {
                            //We removed a method. this may cause other methods to be unused
                            processMethodsOnly = true;
                            unusedMethods.Clear();
                            unusedMethods.AddRange(finalTrans.data.Methods.Select((declItem) => declItem.Decl));
                            base.CaseAAProgram(node);
                            return;
                        }
                    }
                    unusedMethods.Clear();
                    processMethodsOnly = false;
                }
                if (!processFieldsOnly)
                {
                    fieldsWithMethodCalls.Clear();
                    usedFields.Clear();
                    processFieldsOnly = true;
                    base.CaseAAProgram(node);
                    return;
                }

                usedFields.AddRange(finalTrans.data.ObfuscationFields);
                List<SharedData.DeclItem<AFieldDecl>> removedFields = new List<SharedData.DeclItem<AFieldDecl>>();
                foreach (SharedData.DeclItem<AFieldDecl> declItem in finalTrans.data.Fields)
                {
                    AFieldDecl fieldDecl = declItem.Decl;
                    if (fieldDecl.GetConst() == null && !usedFields.Contains(fieldDecl))
                    {
                        if (!reportedFields.Contains(declItem.Decl))
                        {
                            if (firstFieldRun && finalTrans.data.UserFields.Contains(fieldDecl))
                                children.Add(new ErrorCollection.Error(fieldDecl.GetName(),
                                                                            Util.GetAncestor<AASourceFile>(fieldDecl),
                                                                            "Unread field: " + fieldDecl.GetName().Text, true));
                            reportedFields.Add(declItem.Decl);
                        }
                        if (Options.Compiler.RemoveUnusedFields || (fieldDecl.GetType() is AArrayTempType && ((AArrayTempType)fieldDecl.GetType()).GetIntDim().Text == "0"))
                        {
                            //We cannot remove it if there is a method call in it 
                            if (fieldsWithMethodCalls.Contains(fieldDecl))
                                continue;

                            //Remove assignments to the field
                            foreach (AAssignmentExp assignmentExp in assignedToFields[fieldDecl])
                            {
                                if (assignmentExp.Parent() is AExpStm)
                                {
                                    AExpStm stm = (AExpStm)assignmentExp.Parent();
                                    RemoveVariableStatement(stm, assignmentExp.GetExp(), stm.GetToken().Line,
                                                            stm.GetToken().Pos);

                                    continue;
                                }
                                PExp exp = assignmentExp.GetExp();
                                assignmentExp.ReplaceBy(exp);
                            }
                            removedFields.Add(declItem);
                            fieldDecl.Parent().RemoveChild(fieldDecl);
                        }
                    }
                }
                firstFieldRun = false;
                foreach (var removedField in removedFields)
                {
                    finalTrans.data.Fields.Remove(removedField);
                }
               /* if (Options.Compiler.RemoveUnusedFields)
                    finalTrans.data.Fields.RemoveAll(
                        declItem =>
                        (!usedFields.Contains(declItem.Decl)) && (!fieldsWithMethodCalls.Contains(declItem.Decl)));*/
                if (removedFields.Count > 0)
                {
                    //Other fields may have become unused
                    fieldsWithMethodCalls.Clear();
                    usedFields.Clear();
                    processFieldsOnly = true;
                    base.CaseAAProgram(node);
                    return;
                }
                //Remove empty arrays from struct fields
                foreach (var pair in finalTrans.data.StructFields)
                {
                    for (int i = 0; i < pair.Value.Count; i++)
                    {
                        AALocalDecl field = pair.Value[i];
                        if (field.GetType() is AArrayTempType && ((AArrayTempType)field.GetType()).GetIntDim().Text == "0")
                        {
                            field.Parent().RemoveChild(field);
                            pair.Value.RemoveAt(i);
                            --i;
                        }
                    }
                }
            }
            //Remove unused structs
            processFieldsOnly = false;
            if (!processStructs)
            {
                processStructs = true;
                base.CaseAAProgram(node);
                return;
            }
            foreach (SharedData.DeclItem<AStructDecl> declItem in finalTrans.data.Structs)
            {
                if (!usedStructs.Contains(declItem.Decl))
                {
                    if (firstStructRun)
                        children.Add(new ErrorCollection.Error(declItem.Decl.GetName(),
                                                                    Util.GetAncestor<AASourceFile>(declItem.Decl),
                                                                    "Unused struct: " + declItem.Decl.GetName().Text, true));

                    if (Options.Compiler.RemoveUnusedStructs)
                    {
                        if (declItem.Decl != null && declItem.Decl.Parent() != null)
                            declItem.Decl.Parent().RemoveChild(declItem.Decl);
                    }
                }
            }
            if (Options.Compiler.RemoveUnusedStructs)
                finalTrans.data.Structs.RemoveAll(declItem => !usedStructs.Contains(declItem.Decl));


            if (children.Count > 0)
            {
                finalTrans.errors.Add(new ErrorCollection.Error(children[0], "You have unused definitions.", children.ToArray()));
            }
        }


        public override void CaseAMethodDecl(AMethodDecl node)
        {
            if (processStructs || processFieldsOnly || processMethodsOnly)
            {
                base.CaseAMethodDecl(node);
                return;
            }

            bool removed = true;
            List<AALocalDecl> couldntRemove = new List<AALocalDecl>();
            while (removed)
            {
                removed = false;
                definedLocals.Clear();
                usedLocals.Clear();
                assignedToLocals.Clear();
                base.CaseAMethodDecl(node);
                usedLocals.AddRange(finalTrans.data.GeneratedVariables);
                foreach (AALocalDecl definedLocal in definedLocals)
                {
                    if (!usedLocals.Contains(definedLocal) && !couldntRemove.Contains(definedLocal))
                    {
                        if ((Util.GetAncestor<AABlock>(definedLocal) != null || node.GetTrigger() == null) && 
                            finalTrans.data.UserLocals.Contains(definedLocal))
                            children.Add(new ErrorCollection.Error(definedLocal.GetName(),
                                                                            Util.GetAncestor<AASourceFile>(node),
                                                                            "Unread local: " + definedLocal.GetName().Text, true));
                        

                        removed = true;
                        //Remove decl);
                        if (definedLocal.Parent() is ALocalDeclStm)
                        {
                            ALocalDeclStm localDeclStm = (ALocalDeclStm)definedLocal.Parent();
                            RemoveVariableStatement(localDeclStm, definedLocal.GetInit(), localDeclStm.GetToken().Line, localDeclStm.GetToken().Pos);
                        }
                        //Dont remove parameters
                        else
                            couldntRemove.Add(definedLocal);

                        //Remove assignments);
                        foreach (AAssignmentExp assignmentExp in assignedToLocals[definedLocal])
                        {
                            if (assignmentExp.Parent() is AExpStm)
                            {
                                AExpStm stm = (AExpStm)assignmentExp.Parent();
                                RemoveVariableStatement(stm, assignmentExp.GetExp(), stm.GetToken().Line, stm.GetToken().Pos);

                                continue;
                            }
                            PExp exp = assignmentExp.GetExp();
                            assignmentExp.ReplaceBy(exp);
                        }
                    }
                }
            }
        }

        public override void CaseAALocalDecl(AALocalDecl node)
        {
            if (!processFieldsOnly && !processMethodsOnly && !processStructs)
                if (!definedLocals.Contains(node))
                {
                    definedLocals.Add(node);
                    assignedToLocals[node] = new List<AAssignmentExp>();
                }
            base.CaseAALocalDecl(node);
        }

        public override void CaseAAssignmentExp(AAssignmentExp node)
        {
            if (!processMethodsOnly && !processStructs)
            {
                if (!processFieldsOnly)
                    if (node.GetLvalue() is ALocalLvalue)
                    {
                        ALocalLvalue lvalue = (ALocalLvalue)node.GetLvalue();
                        assignedToLocals[finalTrans.data.LocalLinks[lvalue]].Add(node);
                        node.GetExp().Apply(this);
                        return;
                    }
                if (node.GetLvalue() is AFieldLvalue)
                {
                    AFieldLvalue lvalue = (AFieldLvalue)node.GetLvalue();
                    AFieldDecl decl = finalTrans.data.FieldLinks[lvalue];
                    if (!assignedToFields[decl].Contains(node))
                        assignedToFields[decl].Add(node);
                    node.GetExp().Apply(this);
                    return;
                }
            }
            base.CaseAAssignmentExp(node);
        }

        public override void CaseALocalLvalue(ALocalLvalue node)
        {
            if (processFieldsOnly || processMethodsOnly || processStructs) return;

            AALocalDecl decl = finalTrans.data.LocalLinks[node];
            if (!usedLocals.Contains(decl))
                usedLocals.Add(decl);
            base.CaseALocalLvalue(node);
        }

        public override void CaseAFieldLvalue(AFieldLvalue node)
        {
            if (processStructs) return;

            AFieldDecl decl = finalTrans.data.FieldLinks[node];
            if (!usedFields.Contains(decl))
                usedFields.Add(decl);
            base.CaseAFieldLvalue(node);
        }


        public override void CaseASimpleInvokeExp(ASimpleInvokeExp node)
        {
            if (processStructs) return;

            AMethodDecl decl = finalTrans.data.SimpleMethodLinks[node];
            if (!processFieldsOnly && unusedMethods.Contains(decl))
                unusedMethods.Remove(decl);

            AFieldDecl field = Util.GetAncestor<AFieldDecl>(node);
            if (!processMethodsOnly && field != null)
            {
                if (!fieldsWithMethodCalls.Contains(field))
                    fieldsWithMethodCalls.Add(field);
            }

            base.CaseASimpleInvokeExp(node);
        }

        public override void CaseANonstaticInvokeExp(ANonstaticInvokeExp node)
        {
            if (processStructs) return;

            AMethodDecl decl = finalTrans.data.StructMethodLinks[node];
            if (!processFieldsOnly && unusedMethods.Contains(decl))
                unusedMethods.Remove(decl);
            AFieldDecl field = Util.GetAncestor<AFieldDecl>(node);
            if (!processMethodsOnly && field != null)
            {
                if (!fieldsWithMethodCalls.Contains(field))
                    fieldsWithMethodCalls.Add(field);
            }
            base.CaseANonstaticInvokeExp(node);
        }

        public override void CaseANamedType(ANamedType node)
        {
            if (processStructs)
            {
                AStructDecl structDecl = finalTrans.data.StructTypeLinks.ContainsKey(node)
                                             ? finalTrans.data.StructTypeLinks[node]
                                             : null;
                if (structDecl != null && !usedStructs.Contains(structDecl))
                    usedStructs.Add(structDecl);
            }
            base.CaseANamedType(node);
        }

        private PStm RemoveVariableStatement(PStm stm, PExp rightSide, int line, int pos)
        {
            if (rightSide != null)
            {
                List<PStm> statements = MakeStatements(rightSide, line, pos);

                if (statements.Count == 0)
                    stm.Parent().RemoveChild(stm);
                else
                {
                    PStm statement;
                    if (statements.Count == 1)
                        statement = statements[0];
                    else
                        statement = new ABlockStm(new TLBrace("{"), new AABlock(statements, new TRBrace("}")));
                    stm.ReplaceBy(statement);
                    return statement;
                }
            }
            else
                stm.Parent().RemoveChild(stm);
            return null;
        }

        private List<PStm> MakeStatements(PExp exp, int line, int pos)
        {
            List<PStm> list = new List<PStm>();
            if (exp is ASimpleInvokeExp)
            {
                list.Add(new AExpStm(new TSemicolon(";", line, pos), exp));
                return list;
            }
            if (exp is AAssignmentExp)
            {
                list.Add(new AExpStm(new TSemicolon(";", line, pos), exp));
                return list;
            }
            if (exp is ANonstaticInvokeExp)
            {
                list.Add(new AExpStm(new TSemicolon(";", line, pos), exp));
                return list;
            }
            if (exp is ABinopExp)
            {
                ABinopExp aExp = (ABinopExp)exp;
                list.AddRange(MakeStatements(aExp.GetLeft(), line, pos));
                list.AddRange(MakeStatements(aExp.GetRight(), line, pos));
                return list;
            }
            if (exp is AUnopExp)
            {
                AUnopExp aExp = (AUnopExp)exp;
                list.AddRange(MakeStatements(aExp.GetExp(), line, pos));
                return list;
            }
            if (exp is AParenExp)
            {
                AParenExp aExp = (AParenExp)exp;
                list.AddRange(MakeStatements(aExp.GetExp(), line, pos));
                return list;
            }
            if (exp is ALvalueExp)
            {
                ALvalueExp aExp = (ALvalueExp)exp;
                PLvalue lvalue = aExp.GetLvalue();
                if (lvalue is AStructLvalue)
                {
                    AStructLvalue aLvalue = (AStructLvalue)lvalue;
                    list.AddRange(MakeStatements(aLvalue.GetReceiver(), line, pos));
                    return list;
                }
                if (lvalue is AArrayLvalue)
                {
                    AArrayLvalue aLvalue = (AArrayLvalue)lvalue;
                    list.AddRange(MakeStatements(aLvalue.GetBase(), line, pos));
                    list.AddRange(MakeStatements(aLvalue.GetIndex(), line, pos));
                    return list;
                }
            }
            return list;
        }
    }
}

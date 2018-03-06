using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Phases.Transformations.Optimizations;
using Galaxy_Editor_2.Compiler.Phases.Transformations.Pointer_null.Variables;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Pointer_null
{
    class PointerNullFixes
    {
        public static void Parse(AAProgram ast, FinalTransformations finalTrans)
        {
            /* 
             * Should be done in Pointers phase
             * foo == null > foo == "" || !DataTableExists(foo);
             *             | foo == -1 || !inUse[foo] || GetIdentifier(foo) != foo->identifier
             *      
             * 
             * delete foo > delete foo;
             *              foo = null;
             * 
             * foo = null > foo = ""
             *            | foo = -1
             *            
             * 
             * This phase: 
             * Add identifier to all structs and classes that are ever dynamically created
             * At any (*foo), analyze if foo can be null, and if so, create null check.
             * 
             * Phase 1: Generate SafeVariablesData for all methods.
             * 
             */
        }

        private class VariableDecl
        {
            public VariableDecl Base;
            public AFieldDecl Field;
            public AALocalDecl Local;

            public VariableDecl(AFieldDecl field, AALocalDecl local, VariableDecl @base)
            {
                Field = field;
                Local = local;
                Base = @base;
            }

            public static bool operator ==(VariableDecl decl1, VariableDecl decl2)
            {
                return decl1.Equals(decl2);
            }

            public static bool operator !=(VariableDecl decl1, VariableDecl decl2)
            {
                return !(decl1 == decl2);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is VariableDecl))
                    return false;
                VariableDecl other = (VariableDecl) obj;
                if (Field != other.Field ||
                    Local != other.Local ||
                    (Base == null) != (other.Base == null))
                    return false;
                if (Base == null)
                    return true;
                return Base == other.Base;
            }

            public bool IsField
            {
                get
                {
                    if (Base == null)
                        return Field != null;
                    return Base.IsField;
                }
            }
        }


        private class SafeVariablesData
        {
            public List<VariableDecl> SafeFields = new List<VariableDecl>();
            public bool SafeReturn;
            public bool HasDelete;
        }

        private Dictionary<AMethodDecl, SafeVariablesData> Methods = new Dictionary<AMethodDecl, SafeVariablesData>();

        private class SafeVariableDataGenerator : DepthFirstAdapter
        {
            private SharedData data;

            public SafeVariableDataGenerator(SharedData data)
            {
                this.data = data;
            }

            public Dictionary<AMethodDecl, SafeVariablesData> Methods = new Dictionary<AMethodDecl, SafeVariablesData>();
            private AMethodDecl currentMethod;
            private Dictionary<AMethodDecl, List<AMethodDecl>> dependancies = new Dictionary<AMethodDecl, List<AMethodDecl>>();
            


            public override void CaseAMethodDecl(AMethodDecl node)
            {
                currentMethod = node;
                dependancies[node] = new List<AMethodDecl>();
                base.CaseAMethodDecl(node);
                currentMethod = null;
            }

            public override void OutASimpleInvokeExp(ASimpleInvokeExp node)
            {
                if (currentMethod == null)
                    return;
                
                AMethodDecl targetMethod = data.SimpleMethodLinks[node];
                if (node.Parent() is APointerLvalue || node.Parent() is AStructLvalue)
                {
                    //Move node out
                    MoveOut(node, targetMethod.GetReturnType());
                }


                //Only if it is not a library method
                if (Util.HasAncestor<AAProgram>(node))
                {
                    if (!dependancies[currentMethod].Contains(targetMethod))
                        dependancies[currentMethod].Add(targetMethod);
                }
            }

            public override void OutANewExp(ANewExp node)
            {
                if (node.Parent() is APointerLvalue || node.Parent() is AStructLvalue)
                {
                    //Move node out
                    MoveOut(node, node.GetType());
                }
            }

            private void MoveOut(PExp exp, PType type)
            {
                PStm pStm = Util.GetAncestor<PStm>(exp);
                AABlock pBlock = (AABlock)pStm.Parent();

                ALocalLvalue lvalue = new ALocalLvalue(new TIdentifier("gppVar"));
                ALvalueExp lvalueExp = new ALvalueExp(lvalue);
                exp.ReplaceBy(lvalueExp);
                AALocalDecl decl = new AALocalDecl(new APublicVisibilityModifier(), null, null, null, null, Util.MakeClone(type, data), new TIdentifier("gppVar"), exp);
                pBlock.GetStatements().Insert(pBlock.GetStatements().IndexOf(pStm), new ALocalDeclStm(new TSemicolon(";"), decl));

                data.LvalueTypes[lvalue] =
                    data.ExpTypes[lvalueExp] = decl.GetType();
                data.LocalLinks[lvalue] = decl;
            }

            public override void OutAAProgram(AAProgram node)
            {
                //First, analyze methods with no dependancies
                List<AMethodDecl> analyzedMethods = new List<AMethodDecl>();
                while (true)
                {
                    bool processedSomething = false;
                    foreach (KeyValuePair<AMethodDecl, List<AMethodDecl>> pair in dependancies)
                    {
                        AMethodDecl method = pair.Key;
                        if (analyzedMethods.Contains(method))
                            continue;

                        bool hasDependancies = false;
                        foreach (AMethodDecl dependancy in pair.Value)
                        {
                            if (!analyzedMethods.Contains(dependancy))
                            {
                                hasDependancies = true;
                                break;
                            }
                        }
                        if (hasDependancies)
                            continue;
                        processedSomething = true;
                        Analyze(method);
                        analyzedMethods.Add(method);
                    }
                    if (analyzedMethods.Count == dependancies.Count)
                        break;
                    if (processedSomething)
                        continue;
                    //There is a cycle left.
                }
            }

            private void Analyze(AMethodDecl method)
            {
                MethodAnalyzer analyzer = new MethodAnalyzer(method, Methods, data);
                method.GetBlock().Apply(this);
                Methods[method] = analyzer.SafeData;



                /*  List of safe variables after each statement
                 *      While statements need a list for first statement in the while, and a list for first statement after the while
                 *          Also need to account for break and continue statments
                 *      If statements need a list for then then branch, and one for the else branch.
                 *  
                 *  CFG:
                 *  Join(v):
                 *      First, set safeList = intersection(pred(stm))
                 *      Parse through expression, do the folloing in the order you get out of nodes
                 *          pointerLvalue: If unsafe, make a test, and restart check ///NO, its an iterative build. do this after
                 *          delete: clear safeList, safeIfTrue and safeIfFalse
                 *          p == null: if p is a pointer, add p to a safeIfTrue list
                 *          p != null: if p is a pointer, add p to a safeIfFalse list
                 *          !<exp>: swap safeIfTrue and safeIfFalse lists
                 *          <exp> || <exp>: intersection between left and right safeIfTrue list
                 *          <exp> && <exp>: intersection between left and right safeIfFalse list
                 *          
                 *          if stm:         thenList = safeList U safeIfTrue
                 *                          afterList = safeList U safeIfFalse
                 *          if-else stm:    thenList = safeList U safeIfTrue
                 *                          elseList = safeList U safeIfFalse
                 *          while stm:      thenList = safeList U safeIfTrue
                 *          
                 * 
                 * Problem: if something is safe before a while, it currently can't become safe in the while, since it will not initially be safe at the end of the while.
                 *  
                 *  
                 * 
                 * -------------------------------
                 * 
                 * List of unsafe variables after each CFG node.
                 * 
                 * Preprocess step: List of all used variables (Base: Field/Local, Others:Pointer/StructField)
                 * 
                 * All those variables are unsafe before first statment
                 * 
                 * 
                 * Join(v):
                 *      
                 *  
                 */
            }

            private class MethodGetUsedVariables : DepthFirstAdapter
            {
                private SharedData data;
                public List<IVariable> Variables = new List<IVariable>();

                public MethodGetUsedVariables(SharedData data)
                {
                    this.data = data;
                }

                public override void OutALocalLvalue(ALocalLvalue node)
                {
                    base.OutALocalLvalue(node);
                }
            }

            private class MethodAnalyzer : DepthFirstAdapter
            {
                private SharedData data;
                private AMethodDecl method;
                private Dictionary<AMethodDecl, SafeVariablesData> methods;
                public SafeVariablesData SafeData = new SafeVariablesData();

                private Dictionary<PStm, List<VariableDecl>> safeVariablesAfterStatement = new Dictionary<PStm, List<VariableDecl>>();

                public MethodAnalyzer(AMethodDecl method, Dictionary<AMethodDecl, SafeVariablesData> methods, SharedData data)
                {
                    this.method = method;
                    this.data = data;
                    this.methods = methods;
                }


                private List<VariableDecl> unsafeVariables;
                private List<VariableDecl> safeVariables;
                private VariableDecl currentDecl;

                public override void DefaultIn(Node node)
                {
                    if (node is PStm)
                    {
                        safeVariables = new List<VariableDecl>();
                        PStm prev = GetPrevious((PStm) node);
                        if (prev != null)
                        {
                            safeVariables.AddRange(safeVariablesAfterStatement[prev]);
                        }
                        unsafeVariables = new List<VariableDecl>();
                    }
                    base.DefaultIn(node);
                }

                public override void OutAFieldLvalue(AFieldLvalue node)
                {
                    currentDecl = new VariableDecl(data.FieldLinks[node], null, null);
                }

                public override void OutALocalLvalue(ALocalLvalue node)
                {
                    currentDecl = new VariableDecl(null, data.LocalLinks[node], null);
                }

                public override void OutAStructLvalue(AStructLvalue node)
                {
                    currentDecl = new VariableDecl(null, data.StructFieldLinks[node], currentDecl);
                }



                private static PStm GetLast(PStm stm)
                {
                    if (stm is ABlockStm)
                    {
                        AABlock block = (AABlock)((ABlockStm)stm).GetBlock();
                        stm = null;
                        for (int i = block.GetStatements().Count - 1; i >= 0; i--)
                        {
                            stm = GetLast((PStm)block.GetStatements()[i]);
                            if (stm != null)
                                return stm;
                        }
                    }
                    return stm;
                }

                private static PStm GetFirst(PStm stm)
                {
                    if (stm is ABlockStm)
                    {
                        AABlock block = (AABlock)((ABlockStm)stm).GetBlock();
                        stm = null;
                        for (int i = 0; i < block.GetStatements().Count; i++)
                        {
                            stm = GetFirst((PStm)block.GetStatements()[i]);
                            if (stm != null)
                                return stm;
                        }
                    }
                    return stm;
                }

                private static PStm GetNext(PStm stm)
                {
                    while (true)
                    {
                        AABlock pBlock = Util.GetAncestor<AABlock>(stm);
                        if (pBlock == null)
                            return null;
                        int index = pBlock.GetStatements().IndexOf(stm);
                        while (index < pBlock.GetStatements().Count - 1)
                        {
                            stm = GetFirst((PStm)pBlock.GetStatements()[index + 1]);
                            if (stm != null)
                                return stm;
                            index++;
                        }
                        stm = Util.GetAncestor<PStm>(pBlock);
                        if (stm == null)
                            return null;
                    }
                }

                private static PStm GetPrevious(PStm stm)
                {
                    while (true)
                    {
                        AABlock pBlock = Util.GetAncestor<AABlock>(stm);
                        if (pBlock == null)
                            return null;
                        int index = pBlock.GetStatements().IndexOf(stm);
                        while (index > 0)
                        {
                            stm = GetLast((PStm)pBlock.GetStatements()[index - 1]);
                            if (stm != null)
                                return stm;
                            index--;
                        }
                        stm = Util.GetAncestor<PStm>(pBlock);
                        if (stm == null)
                            return null;
                    }
                }

            }
        }
    }
}

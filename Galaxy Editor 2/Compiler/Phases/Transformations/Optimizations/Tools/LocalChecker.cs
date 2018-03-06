using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Optimizations.Tools
{
    class LocalChecker
    {
        public class ModifyData
        {
            public class Values
            {
                public bool Reads;
                public bool Writes;

                public Values(bool reads, bool writes)
                {
                    Reads = reads;
                    Writes = writes;
                }

                public Values(Values copy) : this(copy.Reads, copy.Writes){}

                public void Union(Values other)
                {
                    Reads |= other.Reads;
                    Writes |= other.Writes;
                }

                public void Union(bool reads, bool writes)
                {
                    Reads |= reads;
                    Writes |= writes;
                }

                public bool Conflicts(Values other)
                {
                    return (Writes || other.Writes) && (Reads || Writes) && (other.Reads || other.Writes);
                }
            }

            public Values DataTable = new Values(false, false);
            public Values GameData = new Values(false, false);
            public bool Waits;
            public Dictionary<AFieldDecl, Values> Fields = new Dictionary<AFieldDecl, Values>();
            public Dictionary<AALocalDecl, Values> Locals = new Dictionary<AALocalDecl, Values>();

            public void Add(AFieldDecl field, bool read, bool written)
            {
                if (Fields.ContainsKey(field))
                    Fields[field].Union(read, written);
                else
                    Fields[field] = new Values(read, written);
            }


            public void Add(AALocalDecl local, bool read, bool written)
            {
                if (Locals.ContainsKey(local))
                    Locals[local].Union(read, written);
                else
                    Locals[local] = new Values(read, written);
            }

            public void Union(ModifyData other)
            {
                if (other == null)
                    return;

                DataTable.Union(other.DataTable);
                GameData.Union(other.GameData);
                foreach (var pair in other.Fields)
                {
                    if (Fields.ContainsKey(pair.Key))
                        Fields[pair.Key].Union(pair.Value);
                    else
                        Fields[pair.Key] = new Values(pair.Value);
                }
                foreach (var pair in other.Locals)
                {
                    if (Locals.ContainsKey(pair.Key))
                        Locals[pair.Key].Union(pair.Value);
                    else
                        Locals[pair.Key] = new Values(pair.Value);
                }
                Waits |= other.Waits;
            }

            public bool Conflicts(ModifyData other)
            {
                if (Waits || other.Waits)
                    return true;
                foreach (KeyValuePair<AFieldDecl, Values> pair in other.Fields)
                {
                    if (Fields.ContainsKey(pair.Key))
                    {
                        if (Fields[pair.Key].Conflicts(pair.Value))
                            return true;
                    }
                }

                foreach (KeyValuePair<AALocalDecl, Values> pair in other.Locals)
                {
                    if (Locals.ContainsKey(pair.Key))
                    {
                        if (Locals[pair.Key].Conflicts(pair.Value))
                            return true;
                    }
                }

                return DataTable.Conflicts(other.DataTable) ||
                       GameData.Conflicts(other.GameData);
            }
        }

        public static ModifyData GetLocalData(PExp expression, SharedData data)
        {
            if (expression == null)
                return new ModifyData();

            AMethodDecl method = Util.GetAncestor<AMethodDecl>(expression);
            GetDependancies dependancies = new GetDependancies(data, method);
            expression.Apply(dependancies);
            ModifyData modifyData = new ModifyData();

            foreach (AFieldDecl fieldDecl in dependancies.ReadFields[method])
            {
                modifyData.Add(fieldDecl, true, false);
            }
            foreach (AFieldDecl fieldDecl in dependancies.WrittenFields[method])
            {
                modifyData.Add(fieldDecl, false, true);
            }
            foreach (AALocalDecl localDecl in dependancies.ReadLocals[method])
            {
                modifyData.Add(localDecl, true, false);
            }
            foreach (AALocalDecl localDecl in dependancies.WrittenLocals[method])
            {
                modifyData.Add(localDecl, false, true);
            }

            foreach (AMethodDecl usedMethod in dependancies.UsedMethods[method])
            {
                modifyData.Union(GetModifyData(usedMethod));
            }

            return modifyData;
        }

        

        private static ModifyData GetModifyData(AMethodDecl method)
        {
            if (methodData.ContainsKey(method))
                return methodData[method];

            if (Util.GetAncestor<AAProgram>(method) == null)
            {
                ModifyData modifyData = new ModifyData();
                string name = method.GetName().Text.ToLower();
                if (name.Contains("datatable"))
                {
                    modifyData.DataTable.Reads |= name.Contains("get");
                    modifyData.DataTable.Writes |= name.Contains("set") || name.Contains("remove") || name.Contains("clear");
                }
                else
                {
                    modifyData.GameData.Writes |= name.Contains("set") || name.Contains("create");
                    modifyData.GameData.Reads = true;
                }
                if (name == "wait")
                    modifyData.Waits = true;
                methodData[method] = modifyData;
                return modifyData;
            }
            return null;
        }

        private static Dictionary<AMethodDecl, ModifyData> methodData = new Dictionary<AMethodDecl, ModifyData>();

        public static void CalculateMethodModify(AAProgram ast, SharedData data, out int methodCount)
        {
            //Calculate what global variables all methods might modify.
            methodData.Clear();


            GetDependancies dependancies = new GetDependancies(data);
            ast.Apply(dependancies);
            methodCount = dependancies.UsedMethods.Count;

            while (dependancies.UsedMethods.Count > 0)
            {
                LinkedList<AMethodDecl> modified = new LinkedList<AMethodDecl>();
                foreach (var pair in dependancies.UsedMethods)
                {
                    AMethodDecl method = pair.Key;
                    List<AMethodDecl> usedMethods = pair.Value;


                    ModifyData modifyData = new ModifyData();
                    foreach (AFieldDecl fieldDecl in dependancies.ReadFields[method])
                    {
                        modifyData.Add(fieldDecl, true, false);
                    }
                    foreach (AFieldDecl fieldDecl in dependancies.WrittenFields[method])
                    {
                        modifyData.Add(fieldDecl, false, true);
                    }
                    bool skip = false;
                    foreach (AMethodDecl usedMethod in usedMethods)
                    {
                        ModifyData usedData = GetModifyData(usedMethod);
                        if (usedData == null)
                        {
                            skip = true;
                            break;
                        }
                        modifyData.Union(usedData);
                    }
                    if (skip)
                        continue;
                    methodData[method] = modifyData;
                    modified.AddLast(method);
                }
                foreach (AMethodDecl decl in modified)
                {
                    dependancies.UsedMethods.Remove(decl);
                    dependancies.ReadFields.Remove(decl);
                    dependancies.WrittenFields.Remove(decl);
                }
                if (modified.Count == 0)
                {//The rest is in a circular dependancy
                    foreach (var pair in dependancies.UsedMethods)
                    {
                        AMethodDecl method = pair.Key;

                        ModifyData modifyData = new ModifyData();

                        Update(method, new List<AMethodDecl>(), modifyData, dependancies.UsedMethods, dependancies.ReadFields, dependancies.WrittenFields);

                        methodData[method] = modifyData;
                        modified.AddLast(method);
                    }
                    dependancies.UsedMethods.Clear();
                }
            }
        }


        private static void Update(AMethodDecl method, List<AMethodDecl> parsedMethods, ModifyData modifyData, Dictionary<AMethodDecl, List<AMethodDecl>> usedMethods, Dictionary<AMethodDecl, List<AFieldDecl>> readFields, Dictionary<AMethodDecl, List<AFieldDecl>> writtenFields)
        {
            if (parsedMethods.Contains(method))
                return;

            parsedMethods.Add(method);

            foreach (AFieldDecl fieldDecl in readFields[method])
            {
                modifyData.Add(fieldDecl, true, false);
            }
            foreach (AFieldDecl fieldDecl in writtenFields[method])
            {
                modifyData.Add(fieldDecl, false, true);
            }


            foreach (AMethodDecl usedMethod in usedMethods[method])
            {
                ModifyData data = GetModifyData(usedMethod);
                if (data == null)
                {
                    Update(usedMethod, parsedMethods, modifyData, usedMethods, readFields, writtenFields);
                }
                else
                {
                    modifyData.Union(data);
                }
            }
        }

        private class GetDependancies : DepthFirstAdapter
        {
            private SharedData data;
            public Dictionary<AMethodDecl, List<AMethodDecl>> UsedMethods = new Dictionary<AMethodDecl, List<AMethodDecl>>();
            public Dictionary<AMethodDecl, List<AFieldDecl>> WrittenFields = new Dictionary<AMethodDecl, List<AFieldDecl>>();
            public Dictionary<AMethodDecl, List<AFieldDecl>> ReadFields = new Dictionary<AMethodDecl, List<AFieldDecl>>();
            public Dictionary<AMethodDecl, List<AALocalDecl>> WrittenLocals = new Dictionary<AMethodDecl, List<AALocalDecl>>();
            public Dictionary<AMethodDecl, List<AALocalDecl>> ReadLocals = new Dictionary<AMethodDecl, List<AALocalDecl>>();

            private AMethodDecl currentMethod;

            public GetDependancies(SharedData data, AMethodDecl currentMethod = null)
            {
                this.currentMethod = currentMethod;
                if (currentMethod != null)
                {
                    UsedMethods[currentMethod] = new List<AMethodDecl>();
                    ReadFields[currentMethod] = new List<AFieldDecl>();
                    WrittenFields[currentMethod] = new List<AFieldDecl>();
                    ReadLocals[currentMethod] = new List<AALocalDecl>();
                    WrittenLocals[currentMethod] = new List<AALocalDecl>();
                }
                this.data = data;
            }

            public override void InAMethodDecl(AMethodDecl node)
            {
                currentMethod = node;
                UsedMethods[node] = new List<AMethodDecl>();
                ReadFields[node] = new List<AFieldDecl>();
                WrittenFields[node] = new List<AFieldDecl>();
                ReadLocals[node] = new List<AALocalDecl>();
                WrittenLocals[node] = new List<AALocalDecl>();
            }

            public override void OutAMethodDecl(AMethodDecl node)
            {
                currentMethod = null;
            }

            public override void OutASimpleInvokeExp(ASimpleInvokeExp node)
            {
                if (currentMethod == null)
                    return;
                AMethodDecl decl = data.SimpleMethodLinks[node];
                if (!UsedMethods[currentMethod].Contains(decl))
                    UsedMethods[currentMethod].Add(decl);
            }

            public override void OutAFieldLvalue(AFieldLvalue node)
            {
                if (currentMethod == null)
                    return;

                AFieldDecl field = data.FieldLinks[node];

                Node n = node;
                while (true)
                {
                    n = Util.GetNearestAncestor(n.Parent(), typeof (AMethodDecl), typeof (AAssignmentExp), typeof (AArrayLvalue));
                    if (n is AMethodDecl)
                        break;
                    if (n is AAssignmentExp)
                    {
                        if (Util.IsAncestor(node, ((AAssignmentExp)n).GetLvalue()))
                        {
                            if (!WrittenFields[currentMethod].Contains(field))
                                WrittenFields[currentMethod].Add(field); 
                            return;
                        }
                        break;
                    }
                    if (n is AArrayLvalue)
                    {
                        if (Util.IsAncestor(node, ((AArrayLvalue)n).GetBase()))
                            continue;
                        break;
                    }
                    break;
                }

                if (!ReadFields[currentMethod].Contains(field))
                    ReadFields[currentMethod].Add(field);
            }

            public override void OutALocalLvalue(ALocalLvalue node)
            {
                if (currentMethod == null)
                    return;

                AALocalDecl decl = data.LocalLinks[node];

                Node n = node;
                while (true)
                {
                    n = Util.GetNearestAncestor(n.Parent(), typeof(AMethodDecl), typeof(AAssignmentExp), typeof(AArrayLvalue));
                    if (n is AMethodDecl)
                        break;
                    if (n is AAssignmentExp)
                    {
                        if (Util.IsAncestor(node, ((AAssignmentExp)n).GetLvalue()))
                        {
                            if (!WrittenLocals[currentMethod].Contains(decl))
                                WrittenLocals[currentMethod].Add(decl);
                            return;
                        }
                        break;
                    }
                    if (n is AArrayLvalue)
                    {
                        if (Util.IsAncestor(node, ((AArrayLvalue)n).GetBase()))
                            continue;
                        break;
                    }
                    break;
                }

                if (!ReadLocals[currentMethod].Contains(decl))
                    ReadLocals[currentMethod].Add(decl);
            }
        }
    }
}

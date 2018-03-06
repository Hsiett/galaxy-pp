using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Compiler.Phases;

namespace Galaxy_Editor_2.Compiler.NotGenerated
{
    class InvokeStm
    {
        public Node Node { get; private set; }

        public bool IsAsync { get { return Node is AAsyncInvokeStm; } }
        public AAsyncInvokeStm AsyncNode { get { return (AAsyncInvokeStm)Node; } }
        public ASyncInvokeExp SyncNode { get { return (ASyncInvokeExp)Node; } }

        public Token Token { get { return IsAsync ? (Token)AsyncNode.GetToken() : (Token)SyncNode.GetToken(); } }
        public TIdentifier Name { get; private set; }
        public Node Base { get; private set; }
        public IList Args { get { return IsAsync ? AsyncNode.GetArgs() : SyncNode.GetArgs(); } }

        private PLvalue Target { get { return IsAsync ? AsyncNode.GetName() : SyncNode.GetName(); }
            set
            {
                if (IsAsync) AsyncNode.SetName(value);
                else SyncNode.SetName(value);
            }
        }
        public PExp BaseExp
        {
            get
            {
                /*if (Target == null)
                    return null;
                if (Target is AAmbiguousNameLvalue)
                {
                    AAmbiguousNameLvalue ambigious = (AAmbiguousNameLvalue)Target;
                    AAName name = (AAName)ambigious.GetAmbiguous();
                    if (name.GetIdentifier().Count == 1)
                        return null;
                    Name = (TIdentifier)name.GetIdentifier()[name.GetIdentifier().Count - 1];
                    name.GetIdentifier().RemoveAt(name.GetIdentifier().Count - 1);
                    if (name.GetIdentifier().Count == 0)
                        Base = null;
                    else
                        Base = name;
                }
                else
                {
                    AStructLvalue lvalue = (AStructLvalue)Target;
                    Name = lvalue.GetName();
                    Base = lvalue.GetReceiver();
                }*/
                if (Target is AStructLvalue)
                {
                    AStructLvalue lvalue = (AStructLvalue)Target;
                    return lvalue.GetReceiver();
                }
                return null;
            }
            set
            {
                if (value == null)
                    Target = null;
                else
                    Target = new AStructLvalue(value, new ADotDotType(new TDot(".")), Name);
            }
        }

        public InvokeStm(ASyncInvokeExp node) 
        {
            Node = node;
            if (node.GetName() is AAmbiguousNameLvalue)
            {
                AAmbiguousNameLvalue ambigious = (AAmbiguousNameLvalue) node.GetName();
                AAName name = (AAName)ambigious.GetAmbiguous();
                /*List<List<Node>>[] targets;
                List<ANamespaceDecl> namespaces = new List<ANamespaceDecl>();
                /*bool b;
                TypeLinking.GetTargets(name, out targets, namespaces, data, null, out b);
                for (int i = 0; i < 3; i++)
                {
                    
                }*/
                Name = (TIdentifier)name.GetIdentifier()[name.GetIdentifier().Count - 1];
                name.GetIdentifier().RemoveAt(name.GetIdentifier().Count - 1);
                if (name.GetIdentifier().Count == 0)
                    Base = null;
                else
                    Base = name;
            }
            else
            {
                AStructLvalue lvalue = (AStructLvalue) node.GetName();
                Name = lvalue.GetName();
                Base = lvalue.GetReceiver();
            }
        }

        public InvokeStm(AAsyncInvokeStm node)
        {
            Node = node;
            if (node.GetName() is AAmbiguousNameLvalue)
            {
                AAmbiguousNameLvalue ambigious = (AAmbiguousNameLvalue)node.GetName();
                AAName name = (AAName)ambigious.GetAmbiguous();
                Name = (TIdentifier)name.GetIdentifier()[name.GetIdentifier().Count - 1];
                name.GetIdentifier().RemoveAt(name.GetIdentifier().Count - 1);
                if (name.GetIdentifier().Count == 0)
                    Base = null;
                else
                    Base = name;
            }
            else
            {
                AStructLvalue lvalue = (AStructLvalue)node.GetName();
                Name = lvalue.GetName();
                Base = lvalue.GetReceiver();
            }
        }
    }
}

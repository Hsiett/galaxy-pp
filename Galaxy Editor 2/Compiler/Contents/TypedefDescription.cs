using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Editor_control;

namespace Galaxy_Editor_2.Compiler.Contents
{
    class TypedefDescription : SuggestionBoxItem
    {
        public string Name;
        public ATypedefDecl Snapshot;
        public TextPoint Position { get; private set; }
        public PType realType;

        public TypedefDescription(ATypedefDecl typeDef)
        {
            Name = ((ANamedType) typeDef.GetName()).AsString();
            Snapshot = typeDef;
            typeDef.Parent().RemoveChild(typeDef);
            Position = TextPoint.FromCompilerCoords(typeDef.GetToken());
            realType = typeDef.GetType();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TypedefDescription)) return false;
            TypedefDescription other = (TypedefDescription)obj;
            if (Name != other.Name)
                return false;
            return true;
        }

        public string DisplayText
        {
            get { return Name; }
        }

        public string InsertText
        {
            get { return Name; }
        }

        public string TooltipText
        {
            get { return "typedef" + Name; }
        }

        public string Signature
        {
            get
            {
                return "TD:" + Name;
            }
        }

        public IDeclContainer ParentFile { get; set; }

        public string Comment
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
    }
}

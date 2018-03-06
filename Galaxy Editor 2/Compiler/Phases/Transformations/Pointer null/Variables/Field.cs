using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Pointer_null.Variables
{
    class Field : IVariable
    {
        public AFieldDecl FieldDecl;

        public Field(AFieldDecl fieldDecl)
        {
            FieldDecl = fieldDecl;
        }


        public override bool Equals(object obj)
        {
            if (!(obj is Field))
                return false;
            Field other = (Field) obj;
            return FieldDecl == other.FieldDecl;
        }
    }
}

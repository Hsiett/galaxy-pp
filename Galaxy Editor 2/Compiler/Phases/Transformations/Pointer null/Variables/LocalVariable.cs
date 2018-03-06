using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Pointer_null.Variables
{
    class LocalVariable : IVariable
    {
        public AALocalDecl LocalDecl;

        public LocalVariable(AALocalDecl localDecl)
        {
            LocalDecl = localDecl;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is LocalVariable))
                return false;
            LocalVariable other = (LocalVariable)obj;
            return LocalDecl == other.LocalDecl;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Pointer_null.Variables
{
    class StructField : IVariable
    {
        public AALocalDecl StructFieldDecl;
        public IVariable Base;

        public StructField(AALocalDecl structFieldDecl, IVariable @base)
        {
            StructFieldDecl = structFieldDecl;
            Base = @base;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is StructField))
                return false;
            StructField other = (StructField)obj;
            return StructFieldDecl == other.StructFieldDecl && Base.Equals(other.Base);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations.Pointer_null.Variables
{
    class Pointer : IVariable
    {
        public IVariable Base;

        public Pointer(IVariable @base)
        {
            Base = @base;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Pointer))
                return false;
            Pointer other = (Pointer)obj;
            return Base.Equals(other.Base);
        }
    }
}

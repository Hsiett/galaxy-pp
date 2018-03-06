using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Generated.node
{
    public class ASimpleName
    {
        public TIdentifier Identifier;

        public ASimpleName(TIdentifier identifier)
        {
            Identifier = identifier;
        }
    }
}

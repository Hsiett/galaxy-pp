using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Galaxy_Editor_2.Compiler.Contents
{
    interface IDeclContainer
    {
        List<List<string>> Usings { get; }
        List<MethodDescription> Methods { get; }
        List<VariableDescription> Fields { get; }
        List<StructDescription> Structs { get; }
        List<EnrichmentDescription> Enrichments { get; }
        List<TypedefDescription> Typedefs { get; }
        List<NamespaceDescription> Namespaces { get; }
        SourceFileContents File { get; }
        List<string> NamespaceList { get; }
        string FullName { get; }
    }
}

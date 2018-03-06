using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Editor_control;

namespace Galaxy_Editor_2.Compiler.Contents
{
    interface SuggestionBoxItem
    {
        string DisplayText { get; }
        string InsertText { get; }
        string TooltipText { get; }
        string Comment { get; set; }
        string Signature { get; }
        IDeclContainer ParentFile { get; }
        TextPoint Position { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Contents;
using Galaxy_Editor_2.Editor_control;

namespace Galaxy_Editor_2.Suggestion_box
{
    class CustomSuggestionBoxItem : SuggestionBoxItem
    {
        public string DisplayText { get; set; }

        public string InsertText
        {
            get;
            set; 
        }

        public string TooltipText
        {
            get;
            set; 
        }

        public CustomSuggestionBoxItem(string displayText, string insertText, string tooltipText, Type baseType)
        {
            DisplayText = displayText;
            InsertText = insertText;
            TooltipText = tooltipText;
            BaseType = baseType;
        }

        public Type BaseType;

        public string Signature
        {
            get { return "O" + DisplayText; }
        }

        public IDeclContainer ParentFile
        {
            get { throw new NotImplementedException(); }
        }

        public TextPoint Position
        {
            get { throw new NotImplementedException(); }
        }

        public string Comment
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        string SuggestionBoxItem.Comment
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

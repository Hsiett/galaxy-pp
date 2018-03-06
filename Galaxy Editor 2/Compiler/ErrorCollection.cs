using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Galaxy_Editor_2.Compiler.Generated.node;
using Galaxy_Editor_2.Editor_control;

namespace Galaxy_Editor_2.Compiler
{
    class ErrorCollection
    {
        /*public enum ErrorClass
        {
            Warning,
            Error,

        }*/
        public class Error : TreeNode 
        {
            public TextPoint pos;
            public string FileName;
            public string Message;
            public bool Warning;

            public Error(string message, bool warning = false)
            {
                Message = message;
                Warning = warning;
                Text = ToString();
                SelectedImageIndex = ImageIndex = warning ? 1 : 0;
            }

            public Error(Token pos, string message, bool warning = false, params TreeNode[] children) :
                this(pos, Util.GetAncestor<AASourceFile>(pos), message, warning, children)
            {

            }

            public Error(Token pos, AASourceFile sourceFile, string message, bool warning = false, params TreeNode[] children)
            {
                this.pos = new TextPoint(pos.Line - 1, pos.Pos - 1);
                FileName = sourceFile == null || sourceFile.GetName() == null ? "Library file" : sourceFile.GetName().Text;
                Message = message;
                Warning = warning;
                Text = ToString();
                if (children.Length > 0)
                    Nodes.AddRange(children);
                SelectedImageIndex = ImageIndex = warning ? 1 : 0;
            }

            public Error(Error baseError, string newMessage, params TreeNode[] children)
            {
                pos = baseError.pos;
                FileName = baseError.FileName;
                Message = newMessage;
                Warning = baseError.Warning;
                Text = Message;
                SelectedImageIndex = ImageIndex = Warning ? 1 : 0;
                if (children.Length > 0)
                    Nodes.AddRange(children);
            }
            

            public Error(Token pos, string fileName, string message, bool warning = false)
            {
                this.pos = new TextPoint(pos.Line - 1, pos.Pos - 1);
                FileName = fileName;
                Message = message;
                Warning = warning;
                Text = ToString();
                SelectedImageIndex = ImageIndex = warning ? 1 : 0;
            }


            public override string ToString()
            {
                if (pos != null)
                    return FileName + "[" + (pos.Line + 1) + ", " + (pos.Pos + 1) + "]: " + Message;
                return Message;
            }

            public string ToPrettyString(int indents = 0)
            {
                string s = "";
                for (int i = 0; i < indents; i++)
                    s += " ";
                s += ToString();
                foreach (Error child in Nodes)
                {
                    s += "\r\n" + child.ToPrettyString(indents + 4);
                }
                return s;
            }
        }

        public delegate void ErrorAddedEventHandler(ErrorCollection sender, Error error);

        public event ErrorAddedEventHandler ErrorAdded;

        public List<Error> Errors = new List<Error>();
        public bool FatalErrors = false;

        public void Add(Error error, bool fatal = false)
        {
            if (fatal) FatalErrors = true;
            Errors.Add(error);
            if (ErrorAdded != null) ErrorAdded(this, error);
        }

        public bool HasErrors
        {
            get{ return Errors.Any(error => !error.Warning); }
        }
    }
}

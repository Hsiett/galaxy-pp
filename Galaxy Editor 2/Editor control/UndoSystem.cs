using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Galaxy_Editor_2.Compiler.Generated.lexer;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Editor_control
{
    class UndoSystem
    {
        private enum UndoType
        {
            TextAdded,
            TextRemoved,
            TextReplaced
        }
        private class UndoItem
        {
            public string text;
            public string replaceNewText;
            public MyEditor editor;
            public TextPoint position;
            public UndoType type;

            public UndoItem(string text, MyEditor editor, TextPoint position, UndoType type)
            {
                this.text = text;
                this.editor = editor;
                this.position = position;
                this.type = type;
            }
        }

        private List<UndoItem> undoList = new List<UndoItem>();
        private List<UndoItem> redoList = new List<UndoItem>();

        public bool CanUndo
        {
            get { return undoList.Count > 0; }
        }

        public bool CanRedo
        {
            get { return redoList.Count > 0; }
        }

        public void TextAdded(string text, MyEditor editor, TextPoint position)
        {
            updatedFiles.Remove(editor);
            if (text.Contains(";") || text.Contains("}"))
                AddEditorSignal(editor);

            redoList.Clear();
            //If this and the two previous can be joined into 1 token, join them in 1 undo (if they follow eachother)
            UndoItem item = new UndoItem(text, editor, position, UndoType.TextAdded);
            if (undoList.Count > 0)
            {
                UndoItem lastItem = undoList[undoList.Count - 1];
                if (FollowEachother(lastItem, item))
                {
                    string joined = lastItem.text + item.text;
                    Lexer lexer = new Lexer(new StringReader(joined));
                    List<Token> tokens = new List<Token>();
                    while (!(lexer.Peek() is EOF))
                    {
                        tokens.Add(lexer.Next());
                    }
                    if (tokens.Count == 1 || tokens.All(elm => elm is TWhiteSpace))
                    {
                        lastItem.text += item.text;
                        item = lastItem;

                        if (undoList.Count > 1)
                        {
                            lastItem = undoList[undoList.Count - 2];
                            if (FollowEachother(lastItem, item))
                            {
                                joined = lastItem.text + item.text;
                                lexer = new Lexer(new StringReader(joined));
                                tokens = new List<Token>();
                                while (!(lexer.Peek() is EOF))
                                {
                                    tokens.Add(lexer.Next());
                                }
                                if (tokens.Count == 1 || tokens.All(elm => elm is TWhiteSpace))
                                {
                                    lastItem.text += item.text;
                                    undoList.RemoveAt(undoList.Count - 1);
                                }
                            }
                        }
                        return;
                    }
                }
            }
            undoList.Add(item);
        }

        public void TextRemoved(string text, MyEditor editor, TextPoint position)
        {
            AddEditorSignal(editor);

            redoList.Clear();
            //If this and the two previous can be joined into 1 token, join them in 1 undo (if they follow eachother)
            UndoItem item = new UndoItem(text, editor, position, UndoType.TextRemoved);
            if (undoList.Count > 0)
            {
                UndoItem lastItem = undoList[undoList.Count - 1];
                if (FollowEachother(item, lastItem))
                {
                    string joined = item.text + lastItem.text;
                    Lexer lexer = new Lexer(new StringReader(joined));
                    List<Token> tokens = new List<Token>();
                    while (!(lexer.Peek() is EOF))
                    {
                        tokens.Add(lexer.Next());
                    }
                    if (tokens.Count == 1 || tokens.All(elm => elm is TWhiteSpace))
                    {
                        lastItem.text = joined;
                        lastItem.position = item.position;
                        item = lastItem;

                        if (undoList.Count > 1)
                        {
                            lastItem = undoList[undoList.Count - 2];
                            if (FollowEachother(item, lastItem))
                            {
                                joined = item.text + lastItem.text;
                                lexer = new Lexer(new StringReader(joined));
                                tokens = new List<Token>();
                                while (!(lexer.Peek() is EOF))
                                {
                                    tokens.Add(lexer.Next());
                                }
                                if (tokens.Count == 1 || tokens.All(elm => elm is TWhiteSpace))
                                {
                                    lastItem.text = joined;
                                    lastItem.position = item.position;
                                    undoList.RemoveAt(undoList.Count - 1);
                                }
                            }
                        }
                        return;
                    }
                }
            }
            undoList.Add(item);
        }

        public void TextReplaced(string oldText, string newText, MyEditor editor, TextPoint position)
        {
            AddEditorSignal(editor);
            if (oldText == newText) return;
            UndoItem item = new UndoItem(oldText, editor, position, UndoType.TextReplaced);
            item.replaceNewText = newText;
            undoList.Add(item);
        }

        private bool FollowEachother(UndoItem first, UndoItem second)
        {
            if (first.editor != second.editor || first.type != second.type) return false;
            return GetEndPoint(first) == second.position;
        }

        private TextPoint GetEndPoint(UndoItem item)
        {
            TextPoint endPoint = new TextPoint(item.position.Line, item.position.Pos);
            foreach (char c in item.text)
            {
                if (c == '\n')
                    endPoint = new TextPoint(endPoint.Line + 1, 0);
                else
                    endPoint.Pos++;
            }
            return endPoint;
        }



        public void Undo()
        {
            if (undoList.Count > 0)
            {
                UndoItem item = undoList[undoList.Count - 1];
                undoList.RemoveAt(undoList.Count - 1);
                redoList.Add(item);
                AddEditorSignal(item.editor);
                if (item.type == UndoType.TextAdded)
                {//Do the reversed
                    item.editor.UndoRemove(item.position, GetEndPoint(item));
                }
                else if (item.type == UndoType.TextRemoved)
                {
                    item.editor.UndoInsert(item.position, item.text);
                }
                else if (item.type == UndoType.TextReplaced)
                {
                    item.editor.UndoRemove(item.position, GetEndPoint(new UndoItem(item.replaceNewText, item.editor, item.position, item.type)));
                    item.editor.UndoInsert(item.position, item.text);
                }
            }
        }

        public void Redo()
        {
            if (redoList.Count > 0)
            {
                UndoItem item = redoList[redoList.Count - 1];
                redoList.RemoveAt(redoList.Count - 1);
                undoList.Add(item);
                AddEditorSignal(item.editor);
                if (item.type == UndoType.TextAdded)
                {//Do the reversed
                    item.editor.UndoInsert(item.position, item.text);
                }
                else if (item.type == UndoType.TextRemoved)
                {
                    item.editor.UndoRemove(item.position, GetEndPoint(item));
                }
                else if (item.type == UndoType.TextReplaced)
                {
                    item.editor.UndoRemove(item.position, GetEndPoint(item));
                    item.editor.UndoInsert(item.position, item.replaceNewText);
                }
            }
        }



        //Signal light compiler
        //This is only here because it is convinient - whenever text changes in the editor it has an effect here
        //Dont signal a file more often than 5 seconds
        static List<MyEditor> signalFiles = new List<MyEditor>();
        static List<MyEditor> updatedFiles = new List<MyEditor>();
        private static DateTime lastChange = DateTime.Now;
        private static Timer timer = new Timer();

        static UndoSystem()
        {
            timer.Interval = 5000;
            timer.Enabled = false;
            timer.Tick += SignalFile;
        }

        private static void SignalFile(object sender, EventArgs e)
        {
            timer.Enabled = false;
            SignalFile();
        }

        private static void AddEditorSignal(MyEditor editor)
        {
            /*updatedFiles.Remove(editor);
            signalFiles.Remove(editor);
            signalFiles.Add(editor);
            SignalFile();*/
        }

        public static void AddUnsureEditorSignal(MyEditor editor)
        {
            /*if (updatedFiles.Contains(editor) || signalFiles.Contains(editor))
                return;
            signalFiles.Remove(editor);
            signalFiles.Add(editor);
            SignalFile();*/
        }

        private static void SignalFile()
        {
            if (signalFiles.Count == 0)
                return;
            if (DateTime.Now.Subtract(lastChange).TotalSeconds < 5)
            {
                if (timer.Enabled)
                    return;
                timer.Enabled = true;
                timer.Interval = (int) (5000 - DateTime.Now.Subtract(lastChange).TotalMilliseconds);
                timer.Start();
                return;
            }
            lastChange = DateTime.Now;
            Form1.OpenFileData fileData = (Form1.OpenFileData) signalFiles[0].Tag;
            signalFiles.RemoveAt(0);
            Form1.Form.compiler.SourceFileChanged(fileData.File, fileData.Editor);
            if (signalFiles.Count > 0 && timer.Enabled == false)
            {
                timer.Enabled = true;
                timer.Interval = 5000;
                timer.Start();
            }
        }
    }
}

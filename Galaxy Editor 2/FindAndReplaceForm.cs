using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Galaxy_Editor_2.Editor_control;

namespace Galaxy_Editor_2
{
    public partial class FindAndReplaceForm : AutoSizeForm
    {
        private bool findHadFocus;
        static FindAndReplaceForm()
        {
            form = new FindAndReplaceForm();
            form.Owner = Form1.Form;
            form.Hide();
        }



        public static FindAndReplaceForm form;
        private FindAndReplaceForm()
        {
            InitializeComponent();
            TBFind.GotFocus += TBFind_GotFocus;
            TBReplace.GotFocus += TBReplace_GotFocus;
            foreach (Control control in Controls)
            {
                if (control != TBFind && control != TBReplace)
                    control.GotFocus += AnythingElse_GotFocus;
            }
        }

        private void TBReplace_GotFocus(object sender, EventArgs e)
        {
            findHadFocus = false;
        }

        private void TBFind_GotFocus(object sender, EventArgs e)
        {
            findHadFocus = true;
        }

        private void AnythingElse_GotFocus(object sender, EventArgs e)
        {
            (findHadFocus ? TBFind : TBReplace).Focus();
        }

        private struct Position
        {
            public FileItem File;
            public int Index;
        }

        private Position startPosition;
        private Position currentPosition;
        private bool steppedNextLast;


        public void InitSearch(string searchString)
        {
            TBFind.Text = searchString;
            InitSearch();
        }

        public void InitSearch()
        {
            ResetPos();
            if (startPosition.File.IsDecendantOf(Form1.Form.openProjectSrcDir))
            {
                CBLookSource.Checked = true;
                CBLookOutput.Checked = false;
            }
            else
            {
                CBLookSource.Checked = false;
                CBLookOutput.Checked = true;
            }
            Show();
            TBFind.Focus();
        }

        public void ResetPos()
        {
            if (Form1.Form.CurrentOpenFile == null)
            {
                if (Form1.Form.ProjectSourceFiles.Count > 0)
                {
                    startPosition.File = Form1.Form.ProjectSourceFiles[0];
                    RBProject.Checked = true;
                }
                else
                {
                    MessageBox.Show(this, "There are no files to search in.");
                    Close();
                }
                startPosition.Index = 0;
            }
            else
            {
                RBCurrent.Checked = true;
                startPosition.File = Form1.Form.CurrentOpenFile;
                startPosition.Index = GetIndexFromTextpoint(startPosition.File, startPosition.File.OpenFile.Editor.caret.GetPosition(true));
            }
            currentPosition = startPosition;
        }

        private int GetIndexFromTextpoint(FileItem file, TextPoint point)
        {
            string text = GetText(file);
            int index = 0;
            while (point.Line > 0 || point.Pos > 0)
            {
                if (text[index] == '\n')
                    point.Line--;
                else if (point.Line == 0)
                    point.Pos--;
                index++;
            }
            return index;
        }

        private TextPoint GetTextPointFromIndex(FileItem file, int index)
        {
            string text = GetText(file);
            TextPoint point = new TextPoint(0, 0);
            while (index > 0)
            {
                index--;
                if (text[index] == '\n')
                    point.Line++;
                else if (point.Line == 0)
                    point.Pos++;
            }
            return point;
        }

        private List<FileItem> GetAllFiles()
        {
            List<FileItem> list = new List<FileItem>();
            if (CBLookSource.Checked)
                list.AddRange(Form1.GetSourceFiles(Form1.Form.openProjectSrcDir));
            if (CBLookOutput.Checked)
                list.AddRange(Form1.GetSourceFiles(Form1.Form.openProjectOutputDir));
            return list;
        }

        private FileItem GetNextFile(FileItem file)
        {
            if (RBCurrent.Checked)
                return file;

            List<FileItem> files = GetAllFiles();
            int i = files.IndexOf(currentPosition.File);
            i = (i + 1)%files.Count;
            return files[i];
        }

        private FileItem GetPreviousFile(FileItem file)
        {
            if (RBCurrent.Checked)
                return file;

            List<FileItem> files = GetAllFiles();
            int i = files.IndexOf(currentPosition.File);
            i++;
            if (i < 0)
                i += files.Count;
            return files[i];
        }

        private string GetText(FileItem file)
        {
            if (file.OpenFile != null)
            {
                if (!CBMatchCase.Checked)
                    return file.OpenFile.Editor.Text.ToLower();
                return file.OpenFile.Editor.Text;
            }
            //Open the file
            StreamReader stream = new StreamReader(file.File.OpenRead());
            string text = stream.ReadToEnd();
            stream.Close();
            if (!CBMatchCase.Checked) 
                text = text.ToLower();
            return text;
        }

        private string SearchString
        {
            get
            {
                if (CBMatchCase.Checked)
                    return TBFind.Text;
                return TBFind.Text.ToLower();
            }
        }

        //Return false if start pos was reached
        private bool StepNext(bool mark)
        {
            if (!steppedNextLast)
            {
                startPosition = currentPosition;
                steppedNextLast = true;
            }
            string text = GetText(currentPosition.File);
            int index = text.IndexOf(SearchString, currentPosition.Index + 1);
            if (index == -1)
            {//If we passed start pos, return false
                if (startPosition.File == currentPosition.File && startPosition.Index > currentPosition.Index)
                {
                    currentPosition = startPosition;
                    return false;
                }
                //Else, continue from next file
                currentPosition.File = GetNextFile(currentPosition.File);
                currentPosition.Index = -1;
                return StepNext(mark);
            }
            //We found a new thingy. Check that it is before the start position
            if (startPosition.File == currentPosition.File && startPosition.Index > currentPosition.Index && startPosition.Index <= index)
            {
                currentPosition = startPosition;
                return false;
            }

            currentPosition.Index = index;
            if (mark) MarkCurrentPos();
            return true;
        }



        private bool StepPrevious(bool mark)
        {
            if (steppedNextLast)
            {
                startPosition = currentPosition;
                steppedNextLast = false;
            }
            string text = GetText(currentPosition.File);
            //Find index in reverse
            int index = text.Substring(0, currentPosition.Index).LastIndexOf(SearchString);
            if (index == -1)
            {//If we passed start pos, return false
                if (startPosition.File == currentPosition.File && startPosition.Index < currentPosition.Index)
                {
                    currentPosition = startPosition;
                    return false;
                }
                //Else, continue from next file
                currentPosition.File = GetPreviousFile(currentPosition.File);
                currentPosition.Index = GetText(currentPosition.File).Length;
                return StepPrevious(mark);
            }
            //We found a new thingy. Check that it is before the start position
            if (startPosition.File == currentPosition.File && startPosition.Index < currentPosition.Index && startPosition.Index >= index)
            {
                currentPosition = startPosition;
                return false;
            }

            currentPosition.Index = index;
            if (mark) MarkCurrentPos();
            return true;
        }

        private bool Next(bool mark)
        {
            if (CBSearchUp.Checked)
                return StepPrevious(mark);
            return StepNext(mark);
        }

        private bool Replace(bool mark)
        {
            //Replace currently marked text
            if (currentPosition.File.OpenFile == null)
            {//Open stream and replace. This will only occur in replace all.
                StreamReader reader = currentPosition.File.File.OpenText();
                string text = reader.ReadToEnd();
                reader.Close();
                text = text.Remove(currentPosition.Index, TBFind.Text.Length);
                text = text.Insert(currentPosition.Index, TBReplace.Text);
                StreamWriter writer = new StreamWriter(currentPosition.File.File.Open(FileMode.Create, FileAccess.Write));
                writer.Write(text);
                writer.Close();
            }
            else
            {
                if (currentPosition.File.OpenFile.Editor.TextMarked)
                    currentPosition.File.OpenFile.Editor.ReplaceMarkedText(TBReplace.Text);
                else
                    currentPosition.File.OpenFile.Editor.ReplaceTextAt(
                        GetTextPointFromIndex(currentPosition.File, currentPosition.Index),
                        GetTextPointFromIndex(currentPosition.File, currentPosition.Index + SearchString.Length),
                        TBReplace.Text);
            }
            return Next(mark);
        }

        

        private int ReplaceAll()
        {
            //Replace untill there are no more to replace
            int count = 0;
            if (Next(false))
            {
                count++;
                while (Replace(false))
                    count++;
            }
            return count;
        }

        private void MarkCurrentPos()
        {
            Form1.Form.FocusFile(currentPosition.File);
            currentPosition.File.OpenFile.Editor.Mark(
                GetTextPointFromIndex(currentPosition.File, currentPosition.Index),
                GetTextPointFromIndex(currentPosition.File, currentPosition.Index + SearchString.Length));
        }

        private void BTNFind_Click(object sender, EventArgs e)
        {
            if (!Next(true))
            {
                MessageBox.Show(this, "No more occurences found.", "Search ended");
            }
        }


        private void BTNReplace_Click(object sender, EventArgs e)
        {
            if (!Replace(true))
            {
                MessageBox.Show(this, "No more occurences found.", "Search ended");
            }
        }

        private void BTNReplaceAll_Click(object sender, EventArgs e)
        {
            int count = ReplaceAll();
            MessageBox.Show(this, "Replaced " + count + " occurences.", "Search ended");
        }

        private void CBLook_CheckedChanged(object sender, EventArgs e)
        {
            //Ensure that some files are selected
            if (RBProject.Checked)
            {
                List<FileItem> items = GetAllFiles();
                BTNFind.Enabled = BTNReplace.Enabled = BTNReplaceAll.Enabled = items.Count > 0;
            }
        }

        private void TBFind_TextChanged(object sender, EventArgs e)
        {
            ResetPos();
        }

        private void FindAndReplaceForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void FindAndReplaceForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Escape)
            {
                Hide();
                e.Handled = true;
            }
            if (e.KeyData == Keys.Return)
            {
                BTNFind_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}

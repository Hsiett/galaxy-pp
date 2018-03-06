using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Galaxy_Editor_2.Editor_control;

namespace Galaxy_Editor_2
{
    public partial class OptionsForm : AutoSizeForm
    {
        public OptionsForm()
        {
            InitializeComponent();

            CBCRemoveFields.Checked = Options.Compiler.RemoveUnusedFields;
            CBCRemoveMethods.Checked = Options.Compiler.RemoveUnusedMethods;
            CBCRemoveStructs.Checked = Options.Compiler.RemoveUnusedStructs;
            CBCOneFile.Checked = Options.Compiler.OneOutputFile;
            CBCShortNames.Checked = Options.Compiler.MakeShortNames;
            CBCObfuscateStrings.Checked = Options.Compiler.ObfuscateStrings;
            CBCRunCopy.Checked = Options.Compiler.RunCopy;
            TBCMapBackups.Text = Options.Compiler.NumberOfMapBackups.ToString();
            cbEditorReadOnlyOut.Checked = Options.Editor.ReadOnlyOutput;
            cbEditorReplaceTabs.Checked = Options.Editor.ReplaceTabsWithSpaces;
            CBCNeverAskToOpenSavedFile.Checked = Options.Compiler.NeverAskToRunSavedMap;
            CBEInsertEndBracket.Checked = Options.Editor.InsertEndBracket;
            CBEOpenPreviousProjectAtLaunch.Checked = Options.Editor.OpenInLastProject;
            TBECharWidth.Text = Options.Editor.CharWidth.ToString();
            CBCAutoInline.Checked = Options.Compiler.AutomaticallyInlineShortMethods;

            CBRODifficulty.SelectedIndex = Options.Run.Difficulty;
            CBROGameSpeed.SelectedIndex = Options.Run.GameSpeed;
            CBROFixedSeed.Checked = LROSeed.Enabled = TBROSeed.Enabled = Options.Run.UseFixedSeed;
            TBROSeed.Text = Options.Run.Seed.ToString();
            CBROWindowed.Checked = CBROShowTriggerDebug.Enabled = Options.Run.Windowed;
            CBROShowTriggerDebug.Checked = Options.Run.ShowDebug;
            CBROEnablePreload.Checked = Options.Run.EnablePreload;
            CBROAllowCheat.Checked = Options.Run.AllowCheat;
            TBROAdditionalArgs.Text = Options.Run.AdditionalArgs;


            tabStrip.Items.Clear();
            tabStrip.Items.Add(compilerTab);
            tabStrip.Items.Add(editorTab);
            tabStrip.Items.Add(runOptionsTab);
            tabStrip.SelectedItem = compilerTab;
            initializing = false;
        }

        private bool initializing = true;

        private void CBCRemoveDecls_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            Options.Compiler.RemoveUnusedFields = CBCRemoveFields.Checked;
            Options.Compiler.RemoveUnusedMethods = CBCRemoveMethods.Checked;
            Options.Compiler.RemoveUnusedStructs = CBCRemoveStructs.Checked;
        }

        private void cbEditorReadOnlyOut_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            Options.Editor.ReadOnlyOutput = cbEditorReadOnlyOut.Checked;
            if (!cbEditorReadOnlyOut.Checked)
                MessageBox.Show("Any changes made to the output files will be overwritten next time you compile.",
                                "Warning");
        }

        private void CBCOneFile_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            Options.Compiler.OneOutputFile = CBCOneFile.Checked;
        }

        private void CBCShortNames_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            Options.Compiler.MakeShortNames = CBCShortNames.Checked;
        }

        private void TBCMapBackups_TextChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            try
            {
                uint i = uint.Parse(TBCMapBackups.Text);
                Options.Compiler.NumberOfMapBackups = i;
                TBCMapBackups.BackColor = Color.White;
            }
            catch (Exception err)
            {
                TBCMapBackups.BackColor = Color.Red;
            }
        }

        private void CBCRunCopy_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            Options.Compiler.RunCopy = CBCRunCopy.Checked;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            Options.Compiler.ObfuscateStrings = CBCObfuscateStrings.Checked;

        }

        private void cbEditorReplaceTabs_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            Options.Editor.ReplaceTabsWithSpaces = cbEditorReplaceTabs.Checked;
        }

        private void CBCNeverAskToOpenSavedFile_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            Options.Compiler.NeverAskToRunSavedMap = CBCNeverAskToOpenSavedFile.Checked;
        }

        private void CBEInsertEndBracket_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            Options.Editor.InsertEndBracket = CBEInsertEndBracket.Checked;
        }

        private void CBEOpenPreviousProjectAtLaunch_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            Options.Editor.OpenInLastProject = CBEOpenPreviousProjectAtLaunch.Checked;

        }

        private bool SelectingFontContext;
        private Color currentFontColor;
        private void CBEPickFontContext_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectingFontContext = true;
            FontModification mod = Options.Editor.GetMod((Options.FontStyles)CBEPickFontContext.SelectedIndex);
            CBEBoldFond.Checked = (mod.Style & FontStyle.Bold) != 0;
            CBEItalicsFont.Checked = (mod.Style & FontStyle.Italic) != 0;
            CBEFontUnderline.Checked = (mod.Style & FontStyle.Underline) != 0;
            CBEFontStrikeout.Checked = (mod.Style & FontStyle.Strikeout) != 0;
            currentFontColor = mod.Color;
            SelectingFontContext = false;
        }

        private void FontCheckBoxes_CheckedChanged(object sender, EventArgs e)
        {
            if (SelectingFontContext) return;
            if (CBEPickFontContext.SelectedIndex == -1) return;

            Options.Editor.SetMod((Options.FontStyles) CBEPickFontContext.SelectedIndex, new FontModification(
                                                                                             (CBEBoldFond.Checked
                                                                                                  ? FontStyle.Bold
                                                                                                  : FontStyle.Regular) |
                                                                                             (CBEItalicsFont.Checked
                                                                                                  ? FontStyle.Italic
                                                                                                  : FontStyle.Regular) |
                                                                                             (CBEFontUnderline.Checked
                                                                                                  ? FontStyle.Underline
                                                                                                  : FontStyle.Regular) |
                                                                                             (CBEFontStrikeout.Checked
                                                                                                  ? FontStyle.Strikeout
                                                                                                  : FontStyle.Regular),
                                                                                             currentFontColor
                                                                                             ));
        }

        private void BTNEFontColor_Click(object sender, EventArgs e)
        {
            if (SelectingFontContext) return;
            if (CBEPickFontContext.SelectedIndex == -1) return;

            ColorDialog dialog = new ColorDialog();
            dialog.Color = currentFontColor;
            if (dialog.ShowDialog(this) == DialogResult.Cancel)
                return;
            currentFontColor = dialog.Color;

            Options.Editor.SetMod((Options.FontStyles)CBEPickFontContext.SelectedIndex, new FontModification(
                                                                                             (CBEBoldFond.Checked
                                                                                                  ? FontStyle.Bold
                                                                                                  : FontStyle.Regular) |
                                                                                             (CBEItalicsFont.Checked
                                                                                                  ? FontStyle.Italic
                                                                                                  : FontStyle.Regular) |
                                                                                             (CBEFontUnderline.Checked
                                                                                                  ? FontStyle.Underline
                                                                                                  : FontStyle.Regular) |
                                                                                             (CBEFontStrikeout.Checked
                                                                                                  ? FontStyle.Strikeout
                                                                                                  : FontStyle.Regular),
                                                                                             currentFontColor
                                                                                             ));
        }

        private void BTNEFontPick_Click(object sender, EventArgs e)
        {
            FontDialog dialog = new FontDialog();
            dialog.Font = Options.Editor.Font;
            if (dialog.ShowDialog(this) == DialogResult.Cancel)
                return;
            Options.Editor.Font = dialog.Font;
            if (Form1.Form.CurrentOpenFile != null)
                Form1.Form.CurrentOpenFile.OpenFile.Editor.Restyle();
        }

        private void TBECharWidth_TextChanged(object sender, EventArgs e)
        {
            if (initializing)
                return;
            try
            {
                Options.Editor.CharWidth = int.Parse(TBECharWidth.Text);
                TBECharWidth.BackColor = Color.White;
                if (Form1.Form.CurrentOpenFile != null)
                    Form1.Form.CurrentOpenFile.OpenFile.Editor.Restyle();
            }
            catch (Exception)
            {
                TBECharWidth.BackColor = Color.Red;
            }
        }

        private void CBCAutoInline_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing)
                return;

            Options.Compiler.AutomaticallyInlineShortMethods = CBCAutoInline.Checked;
        }

        private void CBRODifficulty_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (initializing)
                return;
            Options.Run.Difficulty = CBRODifficulty.SelectedIndex;
        }

        private void CBROGameSpeed_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (initializing)
                return;
            Options.Run.GameSpeed = CBROGameSpeed.SelectedIndex;
        }

        private void CBROFixedSeed_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing)
                return;
            Options.Run.UseFixedSeed = LROSeed.Enabled = TBROSeed.Enabled = CBROFixedSeed.Checked;
        }

        private void TBROSeed_TextChanged(object sender, EventArgs e)
        {
            int i;
            try
            {
                i = int.Parse(TBROSeed.Text);
                if (i < 0)
                    throw new Exception();
                TBROSeed.BackColor = Color.White;
            }
            catch (Exception)
            {
                TBROSeed.BackColor = Color.Red;
                return;
            }
            if (initializing)
                return;
            Options.Run.Seed = i;
        }

        private void CBROWindowed_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing)
                return;
            Options.Run.Windowed = CBROShowTriggerDebug.Enabled = CBROWindowed.Checked;
        }

        private void CBROShowTriggerDebug_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing)
                return;
            Options.Run.ShowDebug = CBROShowTriggerDebug.Checked;
        }

        private void CBROEnablePreload_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing)
                return;
            Options.Run.EnablePreload = CBROEnablePreload.Checked;
        }

        private void CBROAllowCheat_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing)
                return;
            Options.Run.AllowCheat = CBROAllowCheat.Checked;
        }

        private void TBROAdditionalArgs_TextChanged(object sender, EventArgs e)
        {
            if (initializing)
                return;
            Options.Run.AdditionalArgs = TBROAdditionalArgs.Text;
        }

        


    }
}

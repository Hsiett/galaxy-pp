using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Galaxy_Editor_2.Dialog_Creator.Controls;
using Galaxy_Editor_2.Dialog_Creator.Enums;
using Button = Galaxy_Editor_2.Dialog_Creator.Controls.Button;
using CheckBox = Galaxy_Editor_2.Dialog_Creator.Controls.CheckBox;
using Label = Galaxy_Editor_2.Dialog_Creator.Controls.Label;
using ListBox = Galaxy_Editor_2.Dialog_Creator.Controls.ListBox;
using ProgressBar = Galaxy_Editor_2.Dialog_Creator.Controls.ProgressBar;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Galaxy_Editor_2.Dialog_Creator
{
    partial class DialogCreatorControl : UserControl
    {
        private DialogData data;
        public DialogCreatorControl()
        {
            data = new DialogData();
            Dialog mainDialog = new Dialog(null, new Rectangle(0, 0, 500, 400), data);
            mainDialog.Anchor = Dialog_Creator.Enums.Anchor.Center;
            data.Dialogs.Add(mainDialog);
            InitializeComponent();
            graphicsControl1.Parent = this;
        }

        public DialogCreatorControl(DialogData d)
        {
            data = d;
            InitializeComponent();
            graphicsControl1.Parent = this;
            TBMaxInstances.Text = d.MaxInstances == null ? "" : d.MaxInstances.ToString();
        }

        private void DialogCreatorControl_Load(object sender, EventArgs e)
        {
            graphicsControl1.SetBackgroundImage(File.Exists("DialogBackground.jpg")
                                                    ? new Bitmap("DialogBackground.jpg")
                                                    : Properties.Resources.DefaultDialogBackground);
            graphicsControl1.SetDialogData(data);

            foreach (Dialog dialog in data.Dialogs)
            {
                ControlAdded(dialog);
                foreach (DialogControl control in dialog.ChildControls)
                {
                    ControlAdded(control);
                }
            }
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Label == "RenderPriority")
                ((DialogControl) propertyGrid.SelectedObject).Parent.ResortChildren();
            graphicsControl1.Invalidate();
            if (!data.Changed)
            {
                data.Changed = true;
                if (data.TabPage != null)
                    data.TabPage.Title += " *";
            }
            if (e.ChangedItem.Parent.Label == "Events")
            {
                //data.AppendCode("void " + (string)e.ChangedItem.Value + "(int sender, Dialog dialog)\n{\n}");
                data.InsertEvent((string)e.ChangedItem.Value, false);
            }
            data.UpdateDesigener();
        }

        public void ControlMovedOrResized()
        {
            //propertyGrid.Refresh();
            if (!data.Changed)
            {
                data.Changed = true;
                data.TabPage.Title += " *";
            }
        }

        public void UpdateSelectedItem()
        {
            CBMainSelectedControl.SelectedItem = graphicsControl1.MainSelectedItem;
        }

        private void ControlAdded(AbstractControl control)
        {
            CBMainSelectedControl.Items.Add(control);
            if (CBMainSelectedControl.SelectedIndex == -1)
                CBMainSelectedControl.SelectedIndex = 0;
        }

        private void CBMainSelectedControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            propertyGrid.SelectedObject = CBMainSelectedControl.SelectedItem;
            propertyGrid.PropertyTabs.Clear(PropertyTabScope.Document);
            if (!(CBMainSelectedControl.SelectedItem is Dialog))
                propertyGrid.PropertyTabs.AddTabType(typeof(EventsPropertyTab), PropertyTabScope.Document);
            else
            {
                //propertyGrid.ResetSelectedProperty();
                propertyGrid.SelectedObject = CBMainSelectedControl.SelectedItem;
            }
            graphicsControl1.SelectItem((AbstractControl) CBMainSelectedControl.SelectedItem);
        }

        private void splitter_SplitterMoved(object sender, SplitterEventArgs e)
        {
            graphicsControl1.Invalidate();
        }

        private void CBViewTerran_CheckedChanged(object sender, EventArgs e)
        {
            graphicsControl1.DisplayRace = Race.Terran;
            propertyGrid.Refresh();
        }

        private void CBViewProtoss_CheckedChanged(object sender, EventArgs e)
        {
            graphicsControl1.DisplayRace = Race.Protoss;
            propertyGrid.Refresh();
        }

        private void CBViewZerg_CheckedChanged(object sender, EventArgs e)
        {
            graphicsControl1.DisplayRace = Race.Zerg;
            propertyGrid.Refresh();
        }

        private void TBScreenHeight_TextChanged(object sender, EventArgs e)
        {
            try
            {
                graphicsControl1.SetTargetHeight(int.Parse(TBScreenHeight.Text));
                TBScreenHeight.BackColor = Color.White;
            }
            catch (Exception err)
            {
                TBScreenHeight.BackColor = Color.Red;
            }
        }

        public void ControlCreated(AbstractControl control)
        {
            CBMainSelectedControl.Items.Add(control);
            UncheckOtherCheckboxes(null);
            if (!data.Changed)
            {
                data.Changed = true;
                data.TabPage.Title += "*";
            }
        }

        public void UncheckOtherCheckboxes(object cb)
        {
            foreach (Control control in TPNewControl.Controls)
            {
                if (control is System.Windows.Forms.CheckBox && control != cb)
                    ((System.Windows.Forms.CheckBox) control).Checked = false;
            }
        }

        public void CancelIfNoneChecked()
        {
            foreach (Control control in TPNewControl.Controls)
            {
                if (control is System.Windows.Forms.CheckBox && ((System.Windows.Forms.CheckBox)control).Checked)
                    return;
            }
            graphicsControl1.CancelCreate();
        }

        private void CBAddDialog_CheckedChanged(object sender, EventArgs e)
        {
            if (CBAddDialog.Checked)
            {
                UncheckOtherCheckboxes(sender);
                ChildDialog dialog = new ChildDialog(graphicsControl1, new Rectangle(0, 0, 200, 300),
                                                     graphicsControl1.MainDialog, data);
                graphicsControl1.Create(dialog);
            }
            else
                CancelIfNoneChecked();
        }

        private void CBAddButton_CheckedChanged(object sender, EventArgs e)
        {
            if (CBAddButton.Checked)
            {
                UncheckOtherCheckboxes(sender);
                Button control = new Button(graphicsControl1, graphicsControl1.MainDialog, data);
                graphicsControl1.Create(control);
            }
            else
                CancelIfNoneChecked();

        }

        private void CBNewImage_CheckedChanged(object sender, EventArgs e)
        {
            if (CBNewImage.Checked)
            {
                UncheckOtherCheckboxes(sender);
                ImageControl control = new ImageControl(graphicsControl1, graphicsControl1.MainDialog, data);
                graphicsControl1.Create(control);
            }
            else
                CancelIfNoneChecked();
        }

        private void CBAddLabel_CheckedChanged(object sender, EventArgs e)
        {
            if (CBAddLabel.Checked)
            {
                UncheckOtherCheckboxes(sender);
                Label control = new Label(graphicsControl1, graphicsControl1.MainDialog, data);
                graphicsControl1.Create(control);
            }
            else
                CancelIfNoneChecked();
        }

        private void CBEditSelectedRaceOnly_CheckedChanged(object sender, EventArgs e)
        {
            graphicsControl1.EditDisplayRaceOnly = CBEditSelectedRaceOnly.Checked;
        }

        private void CBAddCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (CBAddCheckbox.Checked)
            {
                UncheckOtherCheckboxes(sender);
                CheckBox control = new CheckBox(graphicsControl1, graphicsControl1.MainDialog, data);
                graphicsControl1.Create(control);
            }
            else
                CancelIfNoneChecked();
        }

        private void CBAddEditBox_CheckedChanged(object sender, EventArgs e)
        {
            if (CBAddEditBox.Checked)
            {
                UncheckOtherCheckboxes(sender);
                EditBoxControl control = new EditBoxControl(graphicsControl1, graphicsControl1.MainDialog, data);
                graphicsControl1.Create(control);
            }
            else
                CancelIfNoneChecked();
        }

        public void ControlRemoved(AbstractControl ctrl)
        {
            CBMainSelectedControl.Items.Remove(ctrl);
            propertyGrid.SelectedObject = null;
        }

        private void TBMaxInstances_TextChanged(object sender, EventArgs e)
        {
            try
            {
                uint max = UInt32.Parse(TBMaxInstances.Text);
                if (max == data.MaxInstances)
                    return;
                data.MaxInstances = max;
            }
            catch (Exception)
            {
                data.MaxInstances = null;
            }
            if (!data.Changed)
            {
                data.Changed = true;
                data.TabPage.Title += "*";
            }
        }


        private void propertyGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            jumpToEventToolStripMenuItem.Enabled = e.NewSelection != null && e.NewSelection.Parent.Label == "Events";
        }

        private void jumpToEventToolStripMenuItem_Click(object sender, EventArgs e)
        {
            data.InsertEvent((string)propertyGrid.SelectedGridItem.Value, true);
        }

        private void CBAddListBox_CheckedChanged(object sender, EventArgs e)
        {
            if (CBAddListBox.Checked)
            {
                UncheckOtherCheckboxes(sender);
                ListBox control = new ListBox(graphicsControl1, graphicsControl1.MainDialog, data);
                graphicsControl1.Create(control);
            }
            else
                CancelIfNoneChecked();
        }

        private void CBAddProgressBar_CheckedChanged(object sender, EventArgs e)
        {
            if (CBAddProgressBar.Checked)
            {
                UncheckOtherCheckboxes(sender);
                ProgressBar control = new ProgressBar(graphicsControl1, graphicsControl1.MainDialog, data);
                graphicsControl1.Create(control);
            }
            else
                CancelIfNoneChecked();
        }

        private void CBAddSlider_CheckedChanged(object sender, EventArgs e)
        {
            if (CBAddSlider.Checked)
            {
                UncheckOtherCheckboxes(sender);
                Slider control = new Slider(graphicsControl1, graphicsControl1.MainDialog, data);
                graphicsControl1.Create(control);
            }
            else
                CancelIfNoneChecked();
        }

        private void CBAddPullldown_CheckedChanged(object sender, EventArgs e)
        {
            if (CBAddPullldown.Checked)
            {
                UncheckOtherCheckboxes(sender);
                Pulldown control = new Pulldown(graphicsControl1, graphicsControl1.MainDialog, data);
                graphicsControl1.Create(control);
            }
            else
                CancelIfNoneChecked();
        }

        private void delete_Click(object sender, EventArgs e)
        {
            AbstractControl dc = ((AbstractControl)propertyGrid.SelectedObject);
            if (dc != null)
            {

                CBMainSelectedControl.Items.Remove(dc);
                propertyGrid.SelectedObject = null;
                UpdateSelectedItem();

                graphicsControl1.RemoveInterfaceItem(dc);

                graphicsControl1.Invalidate();
            }
            //propertyGrid.ResetSelectedProperty();
            //dc = null;
        }
    }
}

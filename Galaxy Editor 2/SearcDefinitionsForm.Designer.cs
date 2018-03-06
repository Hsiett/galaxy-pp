namespace Galaxy_Editor_2
{
    partial class SearcDefinitionsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearcDefinitionsForm));
            this.CB_Method = new System.Windows.Forms.RadioButton();
            this.CB_Field = new System.Windows.Forms.RadioButton();
            this.CB_Type = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.TB_EndsWith = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.TB_StartsWith = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.TB_Contains = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.GB_Params = new System.Windows.Forms.GroupBox();
            this.BTN_RemoveParameters = new System.Windows.Forms.Button();
            this.LB_Parameters = new System.Windows.Forms.ListBox();
            this.BTN_AddParameter = new System.Windows.Forms.Button();
            this.LB_Types = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.TB_Output = new System.Windows.Forms.RichTextBox();
            this.GB_Type = new System.Windows.Forms.GroupBox();
            this.CB_RType = new System.Windows.Forms.ComboBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.GB_Params.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.GB_Type.SuspendLayout();
            this.SuspendLayout();
            // 
            // CB_Method
            // 
            this.CB_Method.AutoSize = true;
            this.CB_Method.Checked = true;
            this.CB_Method.Location = new System.Drawing.Point(6, 19);
            this.CB_Method.Name = "CB_Method";
            this.CB_Method.Size = new System.Drawing.Size(61, 17);
            this.CB_Method.TabIndex = 0;
            this.CB_Method.TabStop = true;
            this.CB_Method.Text = "Method";
            this.CB_Method.UseVisualStyleBackColor = true;
            this.CB_Method.CheckedChanged += new System.EventHandler(this.CB_Method_CheckedChanged);
            // 
            // CB_Field
            // 
            this.CB_Field.AutoSize = true;
            this.CB_Field.Location = new System.Drawing.Point(73, 19);
            this.CB_Field.Name = "CB_Field";
            this.CB_Field.Size = new System.Drawing.Size(47, 17);
            this.CB_Field.TabIndex = 1;
            this.CB_Field.Text = "Field";
            this.CB_Field.UseVisualStyleBackColor = true;
            // 
            // CB_Type
            // 
            this.CB_Type.AutoSize = true;
            this.CB_Type.Location = new System.Drawing.Point(126, 19);
            this.CB_Type.Name = "CB_Type";
            this.CB_Type.Size = new System.Drawing.Size(49, 17);
            this.CB_Type.TabIndex = 2;
            this.CB_Type.Text = "Type";
            this.CB_Type.UseVisualStyleBackColor = true;
            this.CB_Type.CheckedChanged += new System.EventHandler(this.CB_Type_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.TB_EndsWith);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.TB_StartsWith);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.TB_Contains);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 47);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(357, 170);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Name";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 119);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Ends with";
            // 
            // TB_EndsWith
            // 
            this.TB_EndsWith.Location = new System.Drawing.Point(6, 135);
            this.TB_EndsWith.Name = "TB_EndsWith";
            this.TB_EndsWith.Size = new System.Drawing.Size(342, 20);
            this.TB_EndsWith.TabIndex = 5;
            this.TB_EndsWith.TextChanged += new System.EventHandler(this.InputChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 80);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Starts with";
            // 
            // TB_StartsWith
            // 
            this.TB_StartsWith.Location = new System.Drawing.Point(6, 96);
            this.TB_StartsWith.Name = "TB_StartsWith";
            this.TB_StartsWith.Size = new System.Drawing.Size(342, 20);
            this.TB_StartsWith.TabIndex = 3;
            this.TB_StartsWith.TextChanged += new System.EventHandler(this.InputChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Contains";
            // 
            // TB_Contains
            // 
            this.TB_Contains.Location = new System.Drawing.Point(6, 57);
            this.TB_Contains.Name = "TB_Contains";
            this.TB_Contains.Size = new System.Drawing.Size(342, 20);
            this.TB_Contains.TabIndex = 1;
            this.TB_Contains.TextChanged += new System.EventHandler(this.InputChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(207, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Seperate multiple search texts with comma";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.CB_Method);
            this.groupBox2.Controls.Add(this.CB_Field);
            this.groupBox2.Controls.Add(this.CB_Type);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(357, 47);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Search Type";
            // 
            // GB_Params
            // 
            this.GB_Params.Controls.Add(this.BTN_RemoveParameters);
            this.GB_Params.Controls.Add(this.LB_Parameters);
            this.GB_Params.Controls.Add(this.BTN_AddParameter);
            this.GB_Params.Controls.Add(this.LB_Types);
            this.GB_Params.Controls.Add(this.label5);
            this.GB_Params.Dock = System.Windows.Forms.DockStyle.Top;
            this.GB_Params.Location = new System.Drawing.Point(0, 264);
            this.GB_Params.Name = "GB_Params";
            this.GB_Params.Size = new System.Drawing.Size(357, 174);
            this.GB_Params.TabIndex = 5;
            this.GB_Params.TabStop = false;
            this.GB_Params.Text = "Parameters";
            // 
            // BTN_RemoveParameters
            // 
            this.BTN_RemoveParameters.Location = new System.Drawing.Point(270, 144);
            this.BTN_RemoveParameters.Name = "BTN_RemoveParameters";
            this.BTN_RemoveParameters.Size = new System.Drawing.Size(75, 23);
            this.BTN_RemoveParameters.TabIndex = 4;
            this.BTN_RemoveParameters.Text = "Remove";
            this.BTN_RemoveParameters.UseVisualStyleBackColor = true;
            this.BTN_RemoveParameters.Click += new System.EventHandler(this.BTN_RemoveParameters_Click);
            // 
            // LB_Parameters
            // 
            this.LB_Parameters.FormattingEnabled = true;
            this.LB_Parameters.Location = new System.Drawing.Point(9, 59);
            this.LB_Parameters.Name = "LB_Parameters";
            this.LB_Parameters.Size = new System.Drawing.Size(255, 108);
            this.LB_Parameters.TabIndex = 3;
            // 
            // BTN_AddParameter
            // 
            this.BTN_AddParameter.Location = new System.Drawing.Point(270, 30);
            this.BTN_AddParameter.Name = "BTN_AddParameter";
            this.BTN_AddParameter.Size = new System.Drawing.Size(38, 23);
            this.BTN_AddParameter.TabIndex = 2;
            this.BTN_AddParameter.Text = "Add";
            this.BTN_AddParameter.UseVisualStyleBackColor = true;
            this.BTN_AddParameter.Click += new System.EventHandler(this.BTN_AddParameter_Click);
            // 
            // LB_Types
            // 
            this.LB_Types.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LB_Types.FormattingEnabled = true;
            this.LB_Types.Location = new System.Drawing.Point(9, 32);
            this.LB_Types.Name = "LB_Types";
            this.LB_Types.Size = new System.Drawing.Size(255, 21);
            this.LB_Types.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(31, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Type";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 566);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(357, 34);
            this.panel1.TabIndex = 6;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.button1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(157, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(200, 34);
            this.panel2.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(144, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(44, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Close";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.TB_Output);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(0, 438);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(357, 128);
            this.groupBox4.TabIndex = 7;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Output";
            // 
            // TB_Output
            // 
            this.TB_Output.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TB_Output.Location = new System.Drawing.Point(3, 16);
            this.TB_Output.Name = "TB_Output";
            this.TB_Output.Size = new System.Drawing.Size(351, 109);
            this.TB_Output.TabIndex = 0;
            this.TB_Output.Text = "";
            // 
            // GB_Type
            // 
            this.GB_Type.Controls.Add(this.CB_RType);
            this.GB_Type.Dock = System.Windows.Forms.DockStyle.Top;
            this.GB_Type.Location = new System.Drawing.Point(0, 217);
            this.GB_Type.Name = "GB_Type";
            this.GB_Type.Size = new System.Drawing.Size(357, 47);
            this.GB_Type.TabIndex = 8;
            this.GB_Type.TabStop = false;
            this.GB_Type.Text = "Type";
            // 
            // CB_RType
            // 
            this.CB_RType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CB_RType.FormattingEnabled = true;
            this.CB_RType.Location = new System.Drawing.Point(9, 19);
            this.CB_RType.Name = "CB_RType";
            this.CB_RType.Size = new System.Drawing.Size(255, 21);
            this.CB_RType.TabIndex = 2;
            this.CB_RType.SelectedIndexChanged += new System.EventHandler(this.InputChanged);
            // 
            // SearcDefinitionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(357, 600);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.GB_Params);
            this.Controls.Add(this.GB_Type);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SearcDefinitionsForm";
            this.Text = "Search Definitions";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.GB_Params.ResumeLayout(false);
            this.GB_Params.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.GB_Type.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton CB_Method;
        private System.Windows.Forms.RadioButton CB_Field;
        private System.Windows.Forms.RadioButton CB_Type;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox TB_EndsWith;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TB_StartsWith;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TB_Contains;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox GB_Params;
        private System.Windows.Forms.Button BTN_RemoveParameters;
        private System.Windows.Forms.ListBox LB_Parameters;
        private System.Windows.Forms.Button BTN_AddParameter;
        private System.Windows.Forms.ComboBox LB_Types;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RichTextBox TB_Output;
        private System.Windows.Forms.GroupBox GB_Type;
        private System.Windows.Forms.ComboBox CB_RType;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Timer timer1;
    }
}
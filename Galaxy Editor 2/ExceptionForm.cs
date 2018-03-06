using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using SharedClasses;

namespace Galaxy_Editor_2
{
    public partial class ExceptionForm : AutoSizeForm
    {
        private Exception error;
        private bool askForCode;
        public ExceptionForm(Exception err, bool askForCode = false)
        {
            error = err;
            this.askForCode = askForCode;
            InitializeComponent();

            exceptionMessage.Text = err.ToString();

            CBSendCode.Enabled = ProjectProperties.CurrentProjectPropperties != null;
        }

        private void ExceptionForm_Load(object sender, EventArgs e)
        {
        }

        private void BTNSend_Click(object sender, EventArgs e)
        {
            BTNSend.Enabled = false;
            userMessage.ReadOnly = true;
            byte[] code = null;
            if (CBSendCode.Checked)
            {
                try
                {
                    MemoryStream stream = new MemoryStream();
                    FastZip zipper = new FastZip();
                    zipper.CreateZip(stream,
                                     ProjectProperties.CurrentProjectPropperties.ProjectDir,
                                     true, @"(.*\.galaxy\+\+$)|(.*\.dat$)|(.*\.Dialog$)", "");
                    code = stream.ToArray();
                    stream.Dispose();
                }
                catch (Exception)
                {
                    code = null;
                }
            }
            try
            {
                TcpClient client = new TcpClient(Form1.ServerIP, 25634);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(client.GetStream(), new MyErrorMessage(userMessage.Text, error, Application.ProductVersion, code));
                client.Close();
                Close();
            }
            catch (Exception)
            {
                MessageBox.Show(this,
                                "Unable to send the error message.\nAre you connected to the internet?\nYou could send the information in a PM to SBeier on sc2mapster instead.");

                BTNSend.Enabled = true;
                userMessage.ReadOnly = false;
            }
        }

        private void BTNClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ExceptionForm_Shown(object sender, EventArgs e)
        {
            if (askForCode)
            {
                CBSendCode.Checked = MessageBox.Show(this,
                                                     "For this error, it will most likely be a lot easier to fix it if I have some code to replicate it.\n" +
                                                     "Will you allow that the code in your current project is also included in the error message?",
                                                     "Also send code", MessageBoxButtons.YesNo) == DialogResult.Yes;
            }
        }
    }
}

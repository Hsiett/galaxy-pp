using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Galaxy_Editor_2
{
    public partial class Change_log_form : Form
    {
        public Change_log_form()
        {
            InitializeComponent();

            //Try downloading it
            new Thread(DownloadChangeLog).Start();
        }

        private void DownloadChangeLog()
        {
            try
            {
                var req =
                        (FtpWebRequest)
                        FtpWebRequest.Create(
                            new Uri("ftp://46.163.69.112/Change log.txt"));
                req.UseBinary = true;
                req.Method = WebRequestMethods.Ftp.DownloadFile;
                req.Timeout = 5000;
                var response = (FtpWebResponse)req.GetResponse();
                StreamReader readStream = new StreamReader(response.GetResponseStream());
                string text = readStream.ReadToEnd();
                readStream.Close();
                response.Close();
                Invoke(new UpdateTextDelegate(UpdateText), text);
            }
            catch (Exception)
            {
                return;
            }
        }

        private delegate void UpdateTextDelegate(string text);
        private void UpdateText(string text)
        {
            richTextBox1.Text = text;
        }
    }
}

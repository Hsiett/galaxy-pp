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
using SharedClasses;

namespace Galaxy_Editor_2
{
    public partial class UploadLibraryForm : AutoSizeForm
    {
        public UploadLibraryForm()
        {
            InitializeComponent();
            TBLibName.Text = ProjectProperties.CurrentProjectPropperties.lastLibName;
            TBVersion.Text = ProjectProperties.CurrentProjectPropperties.lastLibVersion;
            TBDescription.Text = ProjectProperties.CurrentProjectPropperties.lastLibDescription;
            TBChangelog.Text = ProjectProperties.CurrentProjectPropperties.lastLibChangeLog;
        }

        private void BTNCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BTNUpload_Click(object sender, EventArgs e)
        {
            Form1.Form.SaveAll();
            ProjectProperties.CurrentProjectPropperties.lastLibName = TBLibName.Text;
            ProjectProperties.CurrentProjectPropperties.lastLibVersion = TBVersion.Text;
            ProjectProperties.CurrentProjectPropperties.lastLibDescription = TBDescription.Text;
            ProjectProperties.CurrentProjectPropperties.lastLibChangeLog = TBChangelog.Text;
            ProjectProperties.CurrentProjectPropperties.Save();

            //Get username and password
            LoginForm loginForm = new LoginForm();
            if (loginForm.ShowDialog(this) == DialogResult.Cancel)
                return;

            Library library = new Library();
            library.Name = TBLibName.Text;
            library.Version = TBVersion.Text;
            library.Description = TBDescription.Text;
            library.ChangeLog = TBChangelog.Text;
            library.UploadDate = DateTime.Now;
            library.Items = new List<Library.Item>();
            AddFiles(ProjectProperties.CurrentProjectPropperties.SrcFolder, library.Items);
            library.Author = loginForm.TBUsername.Text;
            foreach (Library l in ProjectProperties.CurrentProjectPropperties.Libraries)
            {
                library.Dependancies.Add(new LibraryDescription(){Name = l.Name, Version = l.Version, Author = l.Author});
            }

            UploadLibMessage msg = new UploadLibMessage();
            msg.Library = library;
            msg.Password = new EncryptedMessage();
            msg.Overwrite = CBOverwrite.Checked;
            msg.Password.EncryptObject(loginForm.TBPassword.Text, Properties.Resources.serverPublicKey);

            //Send it
            TcpClient client = new TcpClient(Form1.ServerIP, 25634);
            NetworkStream networkStream = client.GetStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(networkStream, msg);
            object obj = formatter.Deserialize(networkStream);
            client.Close();

            if (obj is string && ((string)obj) == "OK")
            {
                MessageBox.Show(this, "Library uploaded successfully", "Success");
                Close();
            }
            else if (obj is ErrorMessage)
            {
                MessageBox.Show(this, ((ErrorMessage)obj).MSG, "Error");
            }
            else
            {
                MessageBox.Show(this, "Unknown response recieved from server.", "Error");
            }
        }

        //Return true if it contained enabled files
        private void AddFiles(FolderItem dir, List<Library.Item> items)
        {
            foreach (DirItem child in dir.Children)
            {
                if (child is FolderItem)
                {
                    List<Library.Item> newItems = new List<Library.Item>();
                    AddFiles((FolderItem) child, newItems);
                    if (newItems.Count > 0)
                    {
                        items.Add(new Library.Folder(){Name = child.Text, Items = newItems});
                    }
                }
                else if (!((FileItem)child).Deactivated)
                {
                    StreamReader reader = ((FileItem) child).File.OpenText();
                    string text = reader.ReadToEnd();
                    reader.Close();
                    reader.Dispose();
                    items.Add(new Library.File(){Name = child.Text, Text = text});
                }
            }
        }
    }
}

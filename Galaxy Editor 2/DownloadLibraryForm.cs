using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using SharedClasses;

namespace Galaxy_Editor_2
{
    public partial class DownloadLibraryForm : AutoSizeForm
    {
        List<string> libraryNames;
        private Dictionary<string, List<Library>> downloadedLibraries = new Dictionary<string, List<Library>>();
        public Library SelectedLibrary;

        public DownloadLibraryForm()
        {
            InitializeComponent();

            //Download library names
            //send LibraryList
            //recieve list<string>
            TcpClient client = new TcpClient(Form1.ServerIP, 25634);
            NetworkStream networkStream = client.GetStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(networkStream, "LibraryList");
            libraryNames = (List<string>) formatter.Deserialize(networkStream);
            client.Close();

            libraryNames.Sort();

            foreach (string libraryName in libraryNames)
            {
                LBLibraries.Items.Add(libraryName);
            }
        }

        private string oldSearchText = "";
        private void TBSearchBox_TextChanged(object sender, EventArgs e)
        {
            string searchText = TBSearchBox.Text.ToLower();
            if (searchText.Contains(oldSearchText))
            {//Remove items from current list
                for (int i = 0; i < LBLibraries.Items.Count; i++)
                {
                    string s = (string) LBLibraries.Items[i];
                    if (!s.ToLower().Contains(searchText))
                    {
                        LBLibraries.Items.RemoveAt(i--);
                    }
                }
            }
            else
            {//Rebuild list entirely
                LBLibraries.Items.Clear();
                foreach (string libraryName in libraryNames)
                {
                    if (libraryName.ToLower().Contains(searchText))
                        LBLibraries.Items.Add(libraryName);
                }
            }
            oldSearchText = searchText;
        }

        private void LBLibraries_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (LBLibraries.SelectedIndex == -1)
            {
                LLibraryName.Text = "Name";
                CBVersions.Items.Clear();
                LAuthor.Text = "Author:";
                RTBDescription.Text = RTBChangeLog.Text = "";
                CBVersions.Enabled = BTNDownload.Enabled = false;
                LBDependancies.Items.Clear();
                return;
            }

            Show((string)LBLibraries.SelectedItem, null);
           
        }

        void Show(string name, string version)
        {
            if (downloadedLibraries.ContainsKey(name))
            {
                Library lib = downloadedLibraries[name].Last();
                foreach (Library library in downloadedLibraries[name])
                {
                    if (library.Version == version)
                    {
                        lib = library;
                        break;
                    }
                }
                LLibraryName.Text = lib.Name;
                CBVersions.Items.Clear();
                foreach (Library library in downloadedLibraries[name])
                {
                    CBVersions.Items.Add(library.Version);
                }
                CBVersions.SelectedIndex = CBVersions.Items.Count - 1;
                LAuthor.Text = "Author: " + lib.Author;
                RTBDescription.Text = lib.Description;
                RTBChangeLog.Text = lib.ChangeLog;
                LBDependancies.Items.Clear();
                foreach (LibraryDescription dependancy in lib.Dependancies)
                {
                    LBDependancies.Items.Add(dependancy);
                }
                CBVersions.Enabled = BTNDownload.Enabled = true;
                return;
            }

            GetLibraryMessage msg = new GetLibraryMessage() { Name = name };

            TcpClient client = new TcpClient(Form1.ServerIP, 25634);
            NetworkStream networkStream = client.GetStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(networkStream, msg);
            List<Library> libraries = (List<Library>)formatter.Deserialize(networkStream);
            client.Close();

            libraries.Sort(Compare);
            downloadedLibraries[name] = libraries;

            {
                Library lib = downloadedLibraries[name].Last();
                foreach (Library library in downloadedLibraries[name])
                {
                    if (library.Version == version)
                    {
                        lib = library;
                        break;
                    }
                }
                LLibraryName.Text = lib.Name;
                CBVersions.Items.Clear();
                foreach (Library library in downloadedLibraries[name])
                {
                    CBVersions.Items.Add(library.Version);
                }
                CBVersions.SelectedIndex = CBVersions.Items.Count - 1;
                LAuthor.Text = "Author: " + lib.Author;
                RTBDescription.Text = lib.Description;
                RTBChangeLog.Text = lib.ChangeLog;
                LBDependancies.Items.Clear();
                foreach (LibraryDescription dependancy in lib.Dependancies)
                {
                    LBDependancies.Items.Add(dependancy);
                }
                CBVersions.Enabled = BTNDownload.Enabled = true;
                return;
            }
        }

        private int Compare(Library lib1, Library lib2)
        {
            return lib1.UploadDate.CompareTo(lib2.UploadDate);
        }

        private void CBVersions_SelectedIndexChanged(object sender, EventArgs e)
        {
            Library lib = downloadedLibraries[(string) LBLibraries.SelectedItem][CBVersions.SelectedIndex];
            LLibraryName.Text = lib.Name;
            LAuthor.Text = "Author: " + lib.Author;
            RTBDescription.Text = lib.Description;
            RTBChangeLog.Text = lib.ChangeLog;
            LBDependancies.Items.Clear();
            foreach (LibraryDescription dependancy in lib.Dependancies)
            {
                LBDependancies.Items.Add(dependancy);
            }
        }

        private void BTNDownload_Click(object sender, EventArgs e)
        {
            SelectedLibrary = downloadedLibraries[(string)LBLibraries.SelectedItem][CBVersions.SelectedIndex];
            Close();
        }

        private void BTNCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void LBDependancies_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (LBDependancies.SelectedIndex == -1)
                return;
            LibraryDescription lib = (LibraryDescription) LBDependancies.SelectedItem;
            Show(lib.Name, lib.Version);
        }
    }
}

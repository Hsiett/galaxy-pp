using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Text;
using System.Windows.Forms;
using SharedClasses;

namespace Galaxy_Editor_2
{
    public partial class ManageUserForm : Form
    {
        private string username;
        private SecureString password;
        private string ClearTextPassword
        {
            get
            {
                return Marshal.PtrToStringUni(Marshal.SecureStringToBSTR(password));
            }
        }

        private GetUserDataReturnMessage data;
        public ManageUserForm()
        {
            InitializeComponent();
        }

        private void ManageUserForm_Load(object sender, EventArgs e)
        {
            LoginForm loginForm = new LoginForm();
            if (loginForm.ShowDialog(this) == DialogResult.Cancel)
            {
                Close();
                return;
            }
            username = loginForm.TBUsername.Text;
            password = new SecureString();
            foreach (char c in loginForm.TBPassword.Text)
            {
                password.AppendChar(c);
            }
            password.MakeReadOnly();
            loginForm.TBPassword.Text = "";

            LUsername.Text = username;


            TcpClient client = new TcpClient(Form1.ServerIP, 25634);
            NetworkStream networkStream = client.GetStream();
            BinaryFormatter formatter = new BinaryFormatter();
            EncryptedMessage msg = new EncryptedMessage();
            byte[] AESKey = msg.EncryptObject(new GetUserDataMessage() { Username = username, Password = ClearTextPassword}, Properties.Resources.serverPublicKey);
            formatter.Serialize(networkStream, msg);
            object obj = formatter.Deserialize(networkStream);
            client.Close();

            if (obj is EncryptedMessage)
            {
                msg = (EncryptedMessage) obj;
                obj = msg.DecryptObject(AESKey);
                data = (GetUserDataReturnMessage) obj;
                TBEmail.Text = data.Email;
                foreach (string lib in data.Libraries.Keys)
                {
                    LBLibraries.Items.Add(lib);
                }
                return;
            }
            if (obj is ErrorMessage)
            {
                MessageBox.Show(this, ((ErrorMessage)obj).MSG, "Error");
            }
            else
            {
                MessageBox.Show(this, "Unknown response recieved from server.", "Error");
            }
            Close();
        }

        private void BTNChangePassword_Click(object sender, EventArgs e)
        {
            if (TBNewPassword.Text == "")
            {
                MessageBox.Show(this, "You must specify a password.", "Error");
                return;
            }

            TcpClient client = new TcpClient(Form1.ServerIP, 25634);
            NetworkStream networkStream = client.GetStream();
            BinaryFormatter formatter = new BinaryFormatter();
            EncryptedMessage msg = new EncryptedMessage();
            msg.EncryptObject(new ChangePasswordMessage() { Username = username, OldPassword = ClearTextPassword, NewPassword = TBNewPassword.Text}, Properties.Resources.serverPublicKey);
            formatter.Serialize(networkStream, msg);
            object obj = formatter.Deserialize(networkStream);
            client.Close();

            if (obj is string && ((string)obj) == "OK")
            {
                MessageBox.Show(this, "Password changed successfully", "Success");
                password.Clear();
                password.Dispose();
                password = new SecureString();
                foreach (char c in TBNewPassword.Text)
                {
                    password.AppendChar(c);
                }
                password.MakeReadOnly();
            }
            else if (obj is ErrorMessage)
            {
                MessageBox.Show(this, ((ErrorMessage)obj).MSG, "Error");
            }
            else
            {
                MessageBox.Show(this, "Unknown response recieved from server.", "Error");
            }
            TBNewPassword.Text = "";
        }

        private void BTNChangeEmail_Click(object sender, EventArgs e)
        {
            TcpClient client = new TcpClient(Form1.ServerIP, 25634);
            NetworkStream networkStream = client.GetStream();
            BinaryFormatter formatter = new BinaryFormatter();
            EncryptedMessage msg = new EncryptedMessage();
            msg.EncryptObject(new ChangeEmailMessage() { Username = username, Password = ClearTextPassword, NewEmail = TBEmail.Text }, Properties.Resources.serverPublicKey);
            formatter.Serialize(networkStream, msg);
            object obj = formatter.Deserialize(networkStream);
            client.Close();

            if (obj is string && ((string)obj) == "OK")
            {
                MessageBox.Show(this, "Email changed successfully", "Success");
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

        private void LBLibraries_SelectedIndexChanged(object sender, EventArgs e)
        {
            CBLibVersion.Items.Clear();
            if (LBLibraries.SelectedIndex == -1)
                return;
            foreach (string s in data.Libraries[(string) LBLibraries.SelectedItem])
            {
                CBLibVersion.Items.Add(s);
            }
        }

        private void BTNDeleteLibrary_Click(object sender, EventArgs e)
        {
            if (LBLibraries.SelectedIndex == -1 || CBLibVersion.SelectedIndex == -1)
                return;
            string libName = (string) LBLibraries.SelectedItem;
            string libVersion = (string) CBLibVersion.SelectedItem;

            TcpClient client = new TcpClient(Form1.ServerIP, 25634);
            NetworkStream networkStream = client.GetStream();
            BinaryFormatter formatter = new BinaryFormatter();
            EncryptedMessage msg = new EncryptedMessage();
            msg.EncryptObject(new DeleteLibraryMessage()
                                    {
                                        Username = username,
                                        Password = ClearTextPassword,
                                        LibName = libName,
                                        LibVersion = libVersion
                                    }, 
                                Properties.Resources.serverPublicKey);
            formatter.Serialize(networkStream, msg);
            object obj = formatter.Deserialize(networkStream);
            client.Close();

            if (obj is string && ((string)obj) == "OK")
            {
                MessageBox.Show(this, "Library deleted successfully", "Success");
                LBLibraries.Items.RemoveAt(LBLibraries.SelectedIndex);
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

        private void BTNClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

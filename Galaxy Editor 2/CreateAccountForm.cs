using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using SharedClasses;

namespace Galaxy_Editor_2
{
    public partial class CreateAccountForm : AutoSizeForm
    {
        public CreateAccountForm()
        {
            InitializeComponent();
        }

        private void BTNCreate_Click(object sender, EventArgs e)
        {
            string username = TBUsername.Text.Trim();
            string password = TBPassword.Text;
            string email = TBEmail.Text;
            if (username == "")
            {
                MessageBox.Show(this, "You need to specify a username.");
                return;
            }
            if (password == "")
            {
                MessageBox.Show(this, "You need to specify a password.");
                return;
            }

            BinaryFormatter formatter = new BinaryFormatter();
            RegisterUserMessage msg = new RegisterUserMessage(){Name = username, Password = password, Email = email};
            //Encrypt
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, msg);
            EncryptedMessage encrMsg = new EncryptedMessage();
            encrMsg.Encrypt(stream.ToArray(), Properties.Resources.serverPublicKey);
            stream.Close();






            //Send it
            TcpClient client = new TcpClient(Form1.ServerIP, 25634);
            NetworkStream networkStream = client.GetStream();
            formatter.Serialize(networkStream, encrMsg);
            object obj = formatter.Deserialize(networkStream);
            client.Close();

            if (obj is string && ((string)obj) == "OK")
            {
                MessageBox.Show(this, "Account created successfully", "Success");
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

        private void BTNCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

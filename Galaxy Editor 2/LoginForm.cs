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
    public partial class LoginForm : AutoSizeForm
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void BTNForgotPassword_Click(object sender, EventArgs e)
        {
            PLogin.Visible = false;
            PReqReset.Visible = true;
            PReqReset.Left = PReqReset.Top = 0;
        }

        private void BTNNext1_Click(object sender, EventArgs e)
        {
            TcpClient client = new TcpClient(Form1.ServerIP, 25634);
            NetworkStream networkStream = client.GetStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(networkStream, new RequestPasswordResetMessage(){Username = TBResetUsername.Text});
            object obj = formatter.Deserialize(networkStream);
            client.Close();

            if (obj is string && ((string)obj) == "OK")
            {
                MessageBox.Show(this, "Check your email for the reset code", "Success");


                PReqReset.Visible = false;
                PReset.Visible = true;
                PReset.Left = PReset.Top = 0;
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

        private void BTNNext2_Click(object sender, EventArgs e)
        {
            TcpClient client = new TcpClient(Form1.ServerIP, 25634);
            NetworkStream networkStream = client.GetStream();
            BinaryFormatter formatter = new BinaryFormatter();
            EncryptedMessage msg = new EncryptedMessage();
            msg.EncryptObject(new ResetPasswordMessage() { Username = TBResetUsername.Text, NewPassword = TBNewPassword.Text, ResetCode = TBResetCode.Text}, Properties.Resources.serverPublicKey);
            formatter.Serialize(networkStream, msg);
            object obj = formatter.Deserialize(networkStream);
            client.Close();

            if (obj is string && ((string)obj) == "OK")
            {
                MessageBox.Show(this, "Password changed successfully", "Success");


                PReset.Visible = false;
                PLogin.Visible = true;
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

        private void BTNLogin_Click(object sender, EventArgs e)
        {
            TcpClient client = new TcpClient(Form1.ServerIP, 25634);
            NetworkStream networkStream = client.GetStream();
            BinaryFormatter formatter = new BinaryFormatter();
            EncryptedMessage msg = new EncryptedMessage();
            msg.EncryptObject(new CheckPasswordMessage() { Username = TBUsername.Text, Password = TBPassword.Text}, Properties.Resources.serverPublicKey);
            formatter.Serialize(networkStream, msg);
            object obj = formatter.Deserialize(networkStream);
            client.Close();

            if (obj is string && ((string)obj) == "OK")
            {
                return;
            }
            DialogResult = DialogResult.None;
            if (obj is ErrorMessage)
            {
                MessageBox.Show(this, ((ErrorMessage)obj).MSG, "Error");
            }
            else
            {
                MessageBox.Show(this, "Unable to verify username and password.", "Error");
            }
        }

        private void TBPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                BTNLogin.PerformClick();
                e.Handled = true;
            }
        }
    }
}

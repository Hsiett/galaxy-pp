using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using System.Net.Mail;
using System.Net;
using Galaxy_Editor_2.Compiler;
using Galaxy_Editor_2.Suggestion_box;
using Microsoft.Win32;

namespace Galaxy_Editor_2
{
    class Foo<T>
    {
        private static int count = 0;

        public static void Incr()
        {
            MessageBox.Show("count = " + ++count);
        }
    }

    class IntWrapper
    {
        public int i;

        public IntWrapper(int i)
        {
            this.i = i;
        }

        public static int Comparerer(IntWrapper a, IntWrapper b)
        {
            return a.i.CompareTo(b.i);
        }
    }

    static class Program
    {


        //[STAThread]
        //static void Main(string[] args)
        //{
        //    Console.WriteLine(StarCraftExecutableFinder.findExecutable());
        //    FunctionExtractor.LoadFunctions();
        //}

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
           /* {
                //Create a new RSACryptoServiceProvider object. 
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(3072);
                //RSA.KeySize = 2376;//Exactly enough to encrypt 256 bit keys
                //Export the key information to an RSAParameters object.
                //Pass false to export the public key information or pass
                //true to export public and private key information.
                //RSAParameters RSAParams = RSA.ExportParameters(false);
                string s = RSA.ToXmlString(false);
                FileInfo file = new FileInfo("serverPublicKey");
                Stream stream = file.Open(FileMode.Create);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, s);
                stream.Close();

                //RSAParams = RSA.ExportParameters(true);
                s = RSA.ToXmlString(true);
                file = new FileInfo("serverPrivateKey");
                stream = file.Open(FileMode.Create);
                formatter.Serialize(stream, s);
                stream.Close();
            }*/


            /*if (args.Length > 0)
            {
                //Backup options
                //if (File.Exists("settings"))
                //    File.Copy("settings", "settings.bak");
                string inputProjectDir = args[0];

                GalaxyCompiler compiler = new GalaxyCompiler();
                compiler.ProjectDir = new DirectoryInfo(inputProjectDir);
                compiler.LoadLibraries();
                ProjectProperties.SetProject(new DirectoryInfo(inputProjectDir));
                compiler.Compile(true);
                return;
            }*/
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-open")
                {
                    if (i + 1 < args.Length)
                    {
                        i++;
                        Options.OverrideLoad = args[i];
                    }
                }
            }

            if (Options.Editor.Font == null)
            {
                if (FontFamily.Families.Any(family => family.Name.ToLower() == "consolas"))
                {
                    Options.Editor.Font = new Font(new FontFamily("Consolas"), 10);
                }
                else if (FontFamily.Families.Any(family => family.Name.ToLower() == "courier new"))
                {
                    Options.Editor.Font = new Font(new FontFamily("Courier New"), 10);
                }
                else
                {
                    Options.Editor.Font = new Font(FontFamily.Families[0], 10);
                }
                Options.Editor.CharWidth = 7;
            }

            /*SmtpClient client = new SmtpClient("smtp.gmail.com", 587)
                                    {
                                        Credentials =
                                            new NetworkCredential("GalaxyPPUser@gmail.com",
                                                                  "Z$yu4uphe-ruswas9enu-ujaqa6ru=e4*me4hU$haw-t6ec@b*emux7s*@n-wuzu"),
                                        EnableSsl = true

                                    };
            client.Send("GalaxyPPUser@gmail.com", "GalaxyPPErrors@gmail.com", "Testing", "This is the body");*/
            /*TcpClient client = new TcpClient(Form1.ServerIP, 25634);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(client.GetStream(), "Test message");
            client.Close();*/





            Application.ThreadException += ErrorHandeler;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Check if XNA is present
            RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\XNA\\Framework\\v4.0", false);
            if (key == null)
            {
                XNAWarning dialog = new XNAWarning();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    System.Diagnostics.Process.Start("http://www.microsoft.com/en-us/download/details.aspx?id=23714");
                    return;
                }
            }
            else
                key.Close();
            Application.Run(new Form1());
        }

        public static void ErrorHandeler(object sender, ThreadExceptionEventArgs e)
        {
            new ExceptionForm(e.Exception).ShowDialog();
        }
    }
}

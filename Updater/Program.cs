using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                if (!new FileInfo("newVersion.zip").Exists)
                {
                    MessageBox.Show("Don't open this file directly.\nUpdate through the main program.", "Updater");
                    return;
                }
                //Wait for the program to close
                List<Process> processes = new List<Process>();
                processes.AddRange(Process.GetProcessesByName("Galaxy++ Editor"));
                //processes.AddRange(Process.GetProcessesByName("Galaxy++ Editor.vshost"));
                while (processes.Count > 0)
                {
                    for (int i = 0; i < processes.Count; i++)
                    {
                        if (processes[i].HasExited)
                        {
                            processes.RemoveAt(i);
                            i--;
                        }
                    }
                }

                //Delete files, extract archive, delete archive
                string[] files = new[]
                                     {
                                         "DefaultMapScript.galaxy++", "Deobfuscator.LibraryData", "Galaxy++ Editor.exe",
                                         "icon.ico", "Precompiled.LibraryData", "SharedClasses.dll", "StormLib.dll", "Updater.exe", 
                                         "ICSharpCode.SharpZipLib.dll", "Galaxy++ Editor.pdb", "Aga.Controls.dll", "Galaxy++ Editor.exe.config",
                                         "DialogBackground.jpg"
                                     };
                string[] dirs = new[]
                                     {
                                         "Fonts"
                                     };
                foreach (string s in files)
                {
                    //FileInfo file = new FileInfo("..\\" + s);
                    int attemptNr = 1;
                    while (true)
                    {
                        if (attemptNr == 10)
                        {
                            MessageBox.Show(
                                "Unable to remove file \"" + s +
                                "\"\nThe file might be in use.\nTry deleting the file yourself before pressing OK.",
                                "Error");
                            break;
                        }
                        try
                        {
                            attemptNr++;
                            if (File.Exists("..\\" + s))
                                File.Delete("..\\" + s);


                        }
                        catch (IOException)
                        {
                            Thread.Sleep(500);
                            continue; 
                        }
                        catch (UnauthorizedAccessException)
                        {
                            Thread.Sleep(500);
                            continue;
                        }
                        break;
                    }
                }
                foreach (string s in dirs)
                {
                    //FileInfo file = new FileInfo("..\\" + s);
                    int attemptNr = 1;
                    while (true)
                    {
                        if (attemptNr == 10)
                        {
                            MessageBox.Show(
                                "Unable to remove directory \"" + s +
                                "\"\nA file in the directory might be in use.\nTry deleting the directory yourself before pressing OK.",
                                "Error");
                            break;
                        }
                        try
                        {
                            attemptNr++;
                            if (Directory.Exists("..\\" + s))
                                Directory.Delete("..\\" + s, true);


                        }
                        catch (IOException)
                        {
                            Thread.Sleep(500);
                            continue;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            Thread.Sleep(500);
                            continue;
                        }
                        break;
                    }
                }

                string dirPath = new DirectoryInfo(".\\..").FullName;
                Stream fileStream = new FileInfo("newVersion.zip").Open(FileMode.Open, FileAccess.Read);
                ZipInputStream zipInputStream = new ZipInputStream(fileStream);
                ZipEntry zipEntry = zipInputStream.GetNextEntry();
                while (zipEntry != null)
                {
                    String entryFileName = zipEntry.Name.Remove(0, zipEntry.Name.IndexOf("/") + 1);
                    if (entryFileName == "" || entryFileName == "settings")
                    {
                        zipEntry = zipInputStream.GetNextEntry();
                        continue;
                    }

                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    byte[] buffer = new byte[4096]; // 4K is optimum

                    // Manipulate the output filename here as desired.
                    String fullZipToPath = Path.Combine(dirPath, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    if (zipEntry.IsFile)
                    {
                        using (FileStream streamWriter = File.Create(fullZipToPath))
                        {
                            StreamUtils.Copy(zipInputStream, streamWriter, buffer);
                            streamWriter.Close();
                        }
                    }
                    zipEntry = zipInputStream.GetNextEntry();
                }
                var startInfo = new ProcessStartInfo();
                startInfo.WorkingDirectory = new DirectoryInfo(".\\..").FullName;
                startInfo.FileName = new FileInfo(".\\..\\Galaxy++ Editor.exe").FullName;


                Process.Start(startInfo);
            }
            catch (Exception err)
            {
                MessageBox.Show("Encountered a critical error.\n\n" + err.Message);
            }
        }
    }
}

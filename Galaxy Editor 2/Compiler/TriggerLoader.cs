using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Galaxy_Editor_2.Compiler.Phases;
using System.Windows;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Galaxy_Editor_2.Compiler
{
    class TriggerLoader
    {
        public static void loadTriggerFile(string filename)
        {
            //Look in map file first
            if (ProjectProperties.CurrentProjectPropperties.InputMap != null &&
                ProjectProperties.CurrentProjectPropperties.InputMap.Exists)
            {
                if (ProjectProperties.CurrentProjectPropperties.InputMap is FileInfo)
                {
                    using (
                        MpqEditor.MpqReader reader =
                            new MpqEditor.MpqReader(ProjectProperties.CurrentProjectPropperties.InputMap.FullName))
                    {
                        string[] foundGalaxyFiles = reader.FindFiles(filename);
                        
                        if (reader.HasFile(filename))
                        {
                            byte[] rawFile = reader.ExtractFile(filename);
                            //put the file into the output folder
                            FolderItem fi = ProjectProperties.CurrentProjectPropperties.OutputFolder;
                            FileInfo file = new FileInfo(fi.FullName + "\\" + filename);
                            if (!file.Directory.Exists) file.Directory.Create();

                            using (FileStream fsWrite = new FileStream(file.FullName, FileMode.Create))
                            {
                                fsWrite.Write(rawFile, 0, rawFile.Length);
                                fsWrite.Flush();
                                
                            };
                            if (fi.Children.Count == 2)
                            {
                                fi.FixConflicts("Triggers");
                               // fi.Children.Add();
                            }
                        }
                    }
                }
            }
        }
        public static void CopyMapToTriggers()
        {
            FolderItem fi = ProjectProperties.CurrentProjectPropperties.OutputFolder;
            FileItem map=null, trigger=null;
            foreach(DirItem it in fi.Children)
            {
               if(it is FileItem)
                {
                    FileItem fit = (FileItem)it;
                    if (fit.Name.Equals("Triggers"))
                    {
                        trigger = fit;

                    }else if (fit.Name.Equals("MapScript.galaxy"))
                    {
                        map = fit;
                    }
                }
            }
            if(map ==null|| trigger == null)
            {
                return;
            }
            FileStream tf = new FileStream(trigger.FullName,FileMode.Open);
            FileStream mf = new FileStream(map.FullName,FileMode.Open);
            StreamReader msr = new StreamReader(mf);
            StreamReader tr = new StreamReader(tf);
            string mapString=msr.ReadToEnd();//insert this stream to tf
            mapString=mapString.Insert(0, "include \"libDDE392F7\"\r\n include \"lib755ACB0A\"\r\n");
            string triggerString = tr.ReadToEnd();
            //triggerString=triggerString.Replace("void GalaxyPPInitMap(){}", mapString);
            msr.Close();
            tr.Close();
            mf.Close();
            tf.Close();
            tf = new FileStream(trigger.FullName,FileMode.Truncate);
            StreamWriter sw = new StreamWriter(tf);
            sw.WriteLine(triggerString);
            sw.Flush();
            sw.Close();
            tf.Close();
            //copy map files to tf
            
        }
        public static LibraryData AddBaseLib(LibraryData oldLib){
            LibraryData lb = new LibraryData();

            FileInfo precompFile = new FileInfo("Base.LibraryData");
            if (!precompFile.Exists)
            {
                MessageBox.Show("Unable to load baselibrary. Base.LibraryData File not found.");
                return oldLib;
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = precompFile.OpenRead();
            try
            {
                lb.Join((LibraryData)formatter.Deserialize(stream));
                lb.JoinNew(oldLib);
                stream.Close();
            }
            catch (Exception err)
            {
                stream.Close();

                MessageBox.Show("Error parsing library.");
                return oldLib;
            }

            return lb;
        }
    }   
}

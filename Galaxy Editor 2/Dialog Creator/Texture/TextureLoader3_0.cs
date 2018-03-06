using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Microsoft.Win32;
using CascLibSharp;
using System.Windows.Forms;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using AlphaSubmarines;

namespace Galaxy_Editor_2.Dialog_Creator.Texture
{
    class TextureLoader3_0 : TextureLoaderInterface
    {
        private readonly Dictionary<string, LoadedTexture> LoadedTextures = new Dictionary<string, LoadedTexture>();
        private class LoadedTexture
        {
            public Texture2D Texture;
            public int UsedCount;
        }

        private static string[] SupportedFiles = new string[] { "*.dds", "*.tga", "*.jpg", "*.png", "*.bmp" };

        public List<string> GetAllPaths()
        {
            List<string> returner = new List<string>();

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
                        string[] files = reader.FindFiles(SupportedFiles);
                        foreach (string file in files)
                        {
                            if (!returner.Contains(file))
                                returner.Add(file);
                        }
                    }
                }
            }
            //Look in standard library after
            if (Options.General.SC2Exe == null ||
                !Options.General.SC2Exe.Exists)
            {
                RegistryKey key = Registry.LocalMachine;
                key = key.OpenSubKey("SOFTWARE\\Wow6432Node\\Blizzard Entertainment\\Starcraft II Retail");
                Options.General.SC2Exe = new FileInfo((string)key.GetValue("GamePath"));
            }
            string sc2Dir = Options.General.SC2Exe.Directory.FullName;

            //no need to find new versions, just peek into the data directory to get all file path.
            //SC2Assets\Assets\Textures

            DirectoryInfo versionDir = new DirectoryInfo(Options.General.SC2Exe.Directory + @"\SC2Data");
            String strModDir = versionDir.FullName;

            using (CascStorageContext casc = new CascStorageContext(strModDir))
            {
                var files = casc.SearchFiles("*");//currently, only * operation is supported.

                foreach(var file in files)
                {
                    //ignore
                    if (!file.FileName.Contains(@"base.sc2assets/Assets/Textures/"))
                        continue;
                    bool supported = false;
                    foreach(var ftype in SupportedFiles)
                    {
                        if (file.FileName.EndsWith(ftype.Substring(1)))
                            supported = true;
                    }
                    if (!supported)
                        continue;
                    //add files to returner
                    if (!returner.Contains(file.FileName))
                        returner.Add(file.FileName);
                }
            }

            //look in mod directories

            Stack<DirectoryInfo> modDirs = new Stack<DirectoryInfo>();
            modDirs.Push(new DirectoryInfo(Options.General.SC2Exe.Directory + @"\Mods"));

            List<FileInfo> modfiles = new List<FileInfo>();
           
            //parse base mods and store SC2Mod files
            while (modDirs.Count != 0)
            {
                DirectoryInfo curVdir = modDirs.Pop();
                if (!curVdir.Exists) continue;

                foreach (var subDir in curVdir.GetDirectories())
                {                    
                    List<FileInfo> filesToSearch = new List<FileInfo>();

                    filesToSearch.AddRange(subDir.GetFiles("*.SC2Mod"));

                    if (filesToSearch.Count > 0)
                        modfiles.AddRange(filesToSearch);

                    filesToSearch.Clear();
                }

            }

            //Read Assets\Textures\files
            foreach (var archive in modfiles)
            {
                MpqEditor.MpqReader fileReader = new MpqEditor.MpqReader(archive.FullName);
                {
                    if (!fileReader.Valid) continue;
                    
                    //fileReader.FindFiles("*.SC2Data");
                    foreach(var supportedfile in SupportedFiles)
                    {
                        string[] textureFiles = fileReader.FindFiles("Assets\\Textures\\"+supportedfile);
                        foreach(var imagefile in textureFiles)
                        {
                            if (!returner.Contains(imagefile))
                                returner.Add(imagefile);
                        }
                    }
                    fileReader.Dispose();
                }
            }


            //no need to find new versions

            //StarCraft II\Versions\<newest>\patch.SC2Archive\Mods\Core.SC2Mod\Base.SC2Assets\Assets\Textures\..
            //StarCraft II\Versions\<newest>\patch.SC2Archive\Mods\Liberty.SC2Mod\Base.SC2Assets\Assets\Textures\..
            //StarCraft II\Versions\<newest>\patch.SC2Archive\Campaigns\Liberty.SC2Campaign\Base.SC2Assets\Assets\Textures\..
            //StarCraft II\Mods\Core.SC2Mod\base.SC2Assets\Assets\Textures\..
            //StarCraft II\Mods\Liberty.SC2Mod\base.SC2Assets\Assets\Textures\..
            //StarCraft II\Mods\Liberty.SC2Mod\enGB.SC2Assets\Assets\Textures\..

            //StarCraft II\*.SC2Assets|Assets\Textures\..
            returner.Sort();
            return returner;
        }

        public Texture2D Load(string path, GraphicsDevice device)
        {
            if (LoadedTextures.ContainsKey(path))
            {
                LoadedTexture texture = LoadedTextures[path];
                texture.UsedCount++;
                return texture.Texture;
            }
            //check long path
            //if(path.StartsWith("Assess"))

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
                        if (reader.HasFile(path))
                        {
                            byte[] rawFile = reader.ExtractFile(path);
                            using (MemoryStream stream = new MemoryStream(rawFile))
                            {

                                //Texture tex = Texture.FromFile(device, stream);
                                //Texture2D texture;
                                //if (tex is Texture2D)
                                //{
                                //    texture = (Texture2D)tex;
                                //}
                                //else
                                //{
                                //    /*MessageBox.Show(
                                //        "Unable to load texture:\n" + path +
                                //        "\nBecause it is not a two dimentional texture.", "Error");*/
                                //    return null;
                                //}

                                
                                Texture2D texture = null;
                                DDSLib.DDSFromStream(stream, device, 0, true, out texture);

                                if (texture == null)
                                {
                                    return null;
                                }
                                LoadedTextures.Add(path, new LoadedTexture() { Texture = texture, UsedCount = 1 });
                                return texture;
                            }
                        }
                    }
                }
            }
            //Look in standard library after
            if (Options.General.SC2Exe == null ||
                !Options.General.SC2Exe.Exists)
            {
                RegistryKey key = Registry.LocalMachine;
                key = key.OpenSubKey("SOFTWARE\\Wow6432Node\\Blizzard Entertainment\\Starcraft II Retail");
                if (key != null)
                    Options.General.SC2Exe = new FileInfo((string)key.GetValue("GamePath"));
                else
                {
                    MessageBox.Show(Form1.Form, "I lost track of where Starcraft II is located. Can you help me?", "Missing data");
                    OpenFileDialog dialog = new OpenFileDialog();
                    dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    dialog.InitialDirectory = dialog.InitialDirectory.Substring(0, dialog.InitialDirectory.IndexOf("\\"));
                    dialog.Filter = "StarCraft II (Starcraft II.exe)|Starcraft II.exe";
                    if (dialog.ShowDialog(Form1.Form) == DialogResult.Cancel)
                    {
                        return null;
                    }
                    Options.General.SC2Exe = new FileInfo(dialog.FileName);
                }
            }

            //can only get the file from the casc file system.
            DirectoryInfo DataDir = new DirectoryInfo(Options.General.SC2Exe.Directory + @"\SC2Data");
            String strDataDir = DataDir.FullName;

            using (CascStorageContext casc = new CascStorageContext(strDataDir))
            {
                try { 
                    using (var file = casc.OpenFile(path))
                    {//currently, only * operation is supported.
                        byte[] rawFile = ReadAllBytes(file);
                        using(MemoryStream stream= new MemoryStream(rawFile))
                        {
                            Texture2D texture = null;
                            DDSLib.DDSFromStream(stream, device, 0, true,out texture);
                            
                            if (texture == null)
                                return null;
                            LoadedTextures.Add(path, new LoadedTexture() { Texture = texture, UsedCount = 1 });
                            return texture;
                        }
                    }
                }catch(Exception ex)
                {
                    
                }
            }

            //load textures from mod directories?
            Stack<DirectoryInfo> modDirs = new Stack<DirectoryInfo>();
            modDirs.Push(new DirectoryInfo(Options.General.SC2Exe.Directory + @"\Mods"));

            List<FileInfo> modfiles = new List<FileInfo>();

            //parse base mods and store SC2Mod files
            while (modDirs.Count != 0)
            {
                DirectoryInfo curVdir = modDirs.Pop();
                if (!curVdir.Exists) continue;

                foreach (var subDir in curVdir.GetDirectories())
                {
                    List<FileInfo> filesToSearch = new List<FileInfo>();

                    filesToSearch.AddRange(subDir.GetFiles("*.SC2Mod"));

                    if (filesToSearch.Count > 0)
                        modfiles.AddRange(filesToSearch);

                    filesToSearch.Clear();
                }

            }

            //Read Assets\Textures\files
            foreach (var archive in modfiles)
            {
                MpqEditor.MpqReader fileReader = new MpqEditor.MpqReader(archive.FullName);
                {
                    if (!fileReader.Valid) continue;

                    //fileReader.FindFiles("*.SC2Data");
                    foreach (var supportedfile in SupportedFiles)
                    {
                        string[] files = fileReader.FindFiles("Assets\\Textures\\" + supportedfile);
                        foreach (var file in files)
                        {
                            int i = file.LastIndexOf("Assets\\Textures");
                            if (i == -1)
                                continue;
                            string fileName = file.Substring(i);
                            if (fileName == path)
                            {
                                byte[] rawFile = fileReader.ExtractFile(file);
                                using (MemoryStream stream = new MemoryStream(rawFile))
                                {
                                    Texture2D texture = null;
                                    try
                                    {
                                        
                                        DDSLib.DDSFromStream(stream, device, 0, true, out texture);
                                    }
                                    catch (Exception ex) { }
                                    if (texture == null)
                                    {
                                        return null;
                                    }
                                    LoadedTextures.Add(path, new LoadedTexture() { Texture = texture, UsedCount = 1 });
                                    return texture;
                                }
                            }
                        }
                    }
                   
                }
            }
            //StarCraft II\Versions\<newest>\patch.SC2Archive\Mods\Core.SC2Mod\Base.SC2Assets\Assets\Textures\..
            //StarCraft II\Versions\<newest>\patch.SC2Archive\Mods\Liberty.SC2Mod\Base.SC2Assets\Assets\Textures\..
            //StarCraft II\Versions\<newest>\patch.SC2Archive\Campaigns\Liberty.SC2Campaign\Base.SC2Assets\Assets\Textures\..
            //StarCraft II\Mods\Core.SC2Mod\base.SC2Assets\Assets\Textures\..
            //StarCraft II\Mods\Liberty.SC2Mod\base.SC2Assets\Assets\Textures\..
            //StarCraft II\Mods\Liberty.SC2Mod\enGB.SC2Assets\Assets\Textures\..

            //StarCraft II\*.SC2Assets|Assets\Textures\..
            return null;
        }

        private byte[] ReadAllBytes(Stream fs)
        {
            byte[] result = new byte[fs.Length];
            fs.Position = 0;
            int cur = 0;
            while (cur < fs.Length)
            {
                int read = fs.Read(result, cur, result.Length - cur);
                cur += read;
            }

            return result;
        }

        public void Unload(string path)
        {
            if (LoadedTextures.ContainsKey(path))
            {
                LoadedTexture texture = LoadedTextures[path];
                texture.UsedCount--;
                if (texture.UsedCount <= 0)
                {
                    texture.Texture.Dispose();
                    LoadedTextures.Remove(path);
                    
                }
            }
        }

        public List<string> GetPossiblePath(string shortPath)
        {
            throw new NotImplementedException();
        }
    }
}

using Microsoft.Win32;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace Galaxy_Editor_2.Dialog_Creator.Texture
{
    class LegacyTextureLoader:TextureLoaderInterface
    {
        private readonly Dictionary<string, LoadedTexture> LoadedTextures = new Dictionary<string, LoadedTexture>();
        private class LoadedTexture
        {
            public Texture2D Texture;
            public int UsedCount;
        }

        private static string[] SupportedFiles = new string[] { "*.dds", "*.tga", "*.jpg", "*.png", "*.bmp" };

        public Texture2D Load(string path, GraphicsDevice device)
        {
            if (LoadedTextures.ContainsKey(path))
            {
                LoadedTexture texture = LoadedTextures[path];
                texture.UsedCount++;
                return texture.Texture;
            }

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
                                Texture2D texture = Texture2D.FromStream(device, stream);
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
            string sc2Dir = Options.General.SC2Exe.Directory.FullName;
            //Find newest version
            int newestNr = 0;
            string newestDir = "";
            foreach (string directory in Directory.GetDirectories(Path.Combine(sc2Dir, "Versions"), "Base*"))
            {
                int i = directory.LastIndexOf("Base");
                string versionNr = directory.Substring(i + 4).Trim('\\', '/');
                i = int.Parse(versionNr);
                if (i > newestNr)
                    newestDir = directory;
            }
            using (MpqEditor.MpqReader reader = new MpqEditor.MpqReader(Path.Combine(newestDir, "patch.SC2Archive")))
            {
                string[] files = reader.FindFiles(SupportedFiles);
                foreach (string file in files)
                {
                    int i = file.LastIndexOf("Assets\\Textures");
                    if (i == -1)
                        continue;
                    string fileName = file.Substring(i);
                    if (fileName == path)
                    {
                        byte[] rawFile = reader.ExtractFile(file);
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
                            Texture2D texture = Texture2D.FromStream(device, stream);
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
            List<string> assets = new List<string>();
            GetAssetFiles(sc2Dir, assets);
            foreach (string asset in assets)
            {
                using (MpqEditor.MpqReader reader = new MpqEditor.MpqReader(asset))
                {
                    string[] files = reader.FindFiles(SupportedFiles);
                    foreach (string file in files)
                    {
                        int i = file.LastIndexOf("Assets\\Textures");
                        if (i == -1)
                            continue;
                        string fileName = file.Substring(i);
                        if (fileName == path)
                        {
                            byte[] rawFile = reader.ExtractFile(file);
                            using (MemoryStream stream = new MemoryStream(rawFile))
                            {
                                //Texture tex = Texture.FromFile(device, stream);
                                //Texture2D texture;
                                //if (tex is Texture2D)
                                //{
                                //    texture = (Texture2D) tex;
                                //}
                                //else
                                //{
                                //    /*MessageBox.Show(
                                //        "Unable to load texture:\n" + path +
                                //        "\nBecause it is not a two dimentional texture.", "Error");*/
                                //    return null;
                                //}
                                Texture2D texture = Texture2D.FromStream(device, stream);
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
            //StarCraft II\Versions\<newest>\patch.SC2Archive\Mods\Core.SC2Mod\Base.SC2Assets\Assets\Textures\..
            //StarCraft II\Versions\<newest>\patch.SC2Archive\Mods\Liberty.SC2Mod\Base.SC2Assets\Assets\Textures\..
            //StarCraft II\Versions\<newest>\patch.SC2Archive\Campaigns\Liberty.SC2Campaign\Base.SC2Assets\Assets\Textures\..
            //StarCraft II\Mods\Core.SC2Mod\base.SC2Assets\Assets\Textures\..
            //StarCraft II\Mods\Liberty.SC2Mod\base.SC2Assets\Assets\Textures\..
            //StarCraft II\Mods\Liberty.SC2Mod\enGB.SC2Assets\Assets\Textures\..

            //StarCraft II\*.SC2Assets|Assets\Textures\..
            return null;
        }

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
            //Find newest version
            int newestNr = 0;
            string newestDir = "";
            foreach (string directory in Directory.GetDirectories(Path.Combine(sc2Dir, "Versions"), "Base*"))
            {
                int i = directory.LastIndexOf("Base");
                string versionNr = directory.Substring(i + 4).Trim('\\', '/');
                i = int.Parse(versionNr);
                if (i > newestNr)
                    newestDir = directory;
            }
            using (MpqEditor.MpqReader reader = new MpqEditor.MpqReader(Path.Combine(newestDir, "patch.SC2Archive")))
            {
                string[] files = reader.FindFiles(SupportedFiles);
                foreach (string file in files)
                {
                    int i = file.LastIndexOf("Assets\\Textures");
                    if (i == -1)
                        continue;
                    string fileName = file.Substring(i);
                    if (!returner.Contains(fileName))
                        returner.Add(fileName);
                }
            }
            List<string> assets = new List<string>();
            GetAssetFiles(sc2Dir, assets);
            foreach (string asset in assets)
            {
                using (MpqEditor.MpqReader reader = new MpqEditor.MpqReader(asset))
                {
                    string[] files = reader.FindFiles(SupportedFiles);
                    foreach (string file in files)
                    {
                        if (!returner.Contains(file))
                            returner.Add(file);
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
            returner.Sort();
            return returner;
        }

        private void GetAssetFiles(string directory, List<string> assets)
        {
            foreach (string file in Directory.GetFiles(directory, "*.SC2Assets"))
            {
                assets.Add(file);
            }
            foreach (string dir in Directory.GetDirectories(directory))
            {
                GetAssetFiles(dir, assets);
            }
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

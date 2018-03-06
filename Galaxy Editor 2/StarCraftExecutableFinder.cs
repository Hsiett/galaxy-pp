using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;

namespace Galaxy_Editor_2
{
    /// <summary>
    /// Helper class for finding the StarCraft 2 executable.
    /// </summary>
    class StarCraftExecutableFinder
    {
        private readonly static List<Tuple<string, string>> REGISTRY_ENTRIES = new List<Tuple<string, string>>();

        static StarCraftExecutableFinder()
        {
            REGISTRY_ENTRIES.Add(new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Blizzard Entertainment\StarCraft II Retail", "GamePath"));
            REGISTRY_ENTRIES.Add(new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\SOFTWARE\Blizzard Entertainment\StarCraft II Retail", "GamePath"));

            REGISTRY_ENTRIES.Add(new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\StarCraft II", "DisplayIcon"));
            REGISTRY_ENTRIES.Add(new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\StarCraft II", "DisplayIcon"));

            REGISTRY_ENTRIES.Add(new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\StarCraft II", "InstallSource"));
            REGISTRY_ENTRIES.Add(new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\StarCraft II", "InstallSource"));
        }

        /// <summary>
        /// Method to search for the executable of StarCraft 2. This tries to find the file in the registry first.
        /// Most people have a normal install of StarCraft and this way the executable can be found without user interaction.
        /// If it is not found there, the user is asked.
        /// </summary>
        /// <returns>The full path to the executable if found, <code>null</code> if not.</returns>
        public static string findExecutable()
        {
            // look at the registry for the starcraft executable
            foreach (Tuple<string,string> registryEntry in REGISTRY_ENTRIES)
            {
                string s = tryRegistry(registryEntry.Item1, registryEntry.Item2);
                if (checkPathValidity(s)) return s;
            }

            // no entry found in the registry
            // ask the user for the location
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dialog.InitialDirectory = dialog.InitialDirectory.Substring(0, dialog.InitialDirectory.IndexOf("\\"));
            dialog.Filter = "StarCraft II (Starcraft II.exe)|Starcraft II.exe";

            string sc2Exe = null;
            while (!checkPathValidity(sc2Exe))
            {
                MessageBox.Show("I lost track of where Starcraft II is located. Can you help me?", "Missing data");

                if (dialog.ShowDialog() == DialogResult.Cancel)
                {
                    MessageBox.Show("StarCraft 2 Executable not found. Note that you cannot use any of the StarCraft 2 natives before you specify the location.", "Missing data");
                    return null;
                }
                sc2Exe = dialog.FileName;
            }

            return sc2Exe;
        }

        /// <summary>
        /// Checks if the specified path is a valid starcraft executable
        /// </summary>
        /// <param name="s">The string to check</param>
        /// <returns><code>true if its valid</code></returns>
        public static bool checkPathValidity(string s)
        {
            // TODO this is just a VERY simple check and the may be better ways to determine if it's really
            // the starcraft executable
            if (s == null ||s.Length == 0) return false;

            return File.Exists(s);
        }

        private static string tryRegistry(string path, string key)
        {
            object o = null;
            try
            {
                o = Registry.GetValue(path, key, null);
            }
            catch (Exception)
            {
                o = null;
            }

            if (o != null && o is string)
                return (string)o;

            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Galaxy_Editor_2.Editor_control;

namespace Galaxy_Editor_2
{
    [Serializable]
    class Options
    {
        public static bool CreatedNew;
        static Options()
        {
            FileInfo file = new FileInfo("settings");
            Compiler = new CompilerOptions();
            Editor = new EditorOptions();
            General = new GeneralOptions();
            Run = new RunOptions();
            if (file.Exists)
            {
                CreatedNew = false;
                Stream stream = file.OpenRead();
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    Compiler = (CompilerOptions)formatter.Deserialize(stream);
                    Editor = (EditorOptions)formatter.Deserialize(stream);
                    General = (GeneralOptions)formatter.Deserialize(stream);
                    Run = (RunOptions)formatter.Deserialize(stream);
                }
                catch (Exception)
                {
                }
                finally
                {
                    stream.Close();
                }
            }
            else
                CreatedNew = true;
        }

        private static void SettingsChanged()
        {
            FileInfo file = new FileInfo("settings");
            Stream stream = file.Open(FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, Compiler);
            formatter.Serialize(stream, Editor);
            formatter.Serialize(stream, General);
            formatter.Serialize(stream, Run);
            stream.Close();
        }

        public static CompilerOptions Compiler;
        public static EditorOptions Editor;
        public static GeneralOptions General;
        public static RunOptions Run;
        public static string OverrideLoad = null;
        public static ModCompileOptions Mod;

        [Serializable]
        public class GeneralOptions
        {
            private List<string> recentProjects = new List<string>();

            public string[] RecentProjects
            {
                get
                {
                    if (recentProjects == null)
                        recentProjects = new List<string>();
                    return recentProjects.ToArray();
                }
            }

            public void ProjectOpened(string project)
            {
                if (recentProjects == null)
                    recentProjects = new List<string>();
                int i = recentProjects.IndexOf(project);
                if (i == 0)
                    return;
                if (i != -1)
                    recentProjects.RemoveAt(i);
                recentProjects.Insert(0, project);
                SettingsChanged();
                Form1.RebuildJumpList();
            }
            

            private string lastVersion;
            public string LastVersion
            {
                get
                {
                    if (string.IsNullOrEmpty(lastVersion))
                        lastVersion = "0.0.0";
                    return lastVersion;
                }
                set
                {
                    if (lastVersion != value)
                    {
                        lastVersion = value;
                        SettingsChanged();
                    }
                }
            }


            private FileInfo sC2Exe;
            public FileInfo SC2Exe
            {
                get { return sC2Exe; }
                set
                {
                    if (sC2Exe != value)
                    {
                        sC2Exe = value;
                        SettingsChanged();
                    }
                }
            }

            private Point formPos = new Point(0, 0);
            public Point FormPos
            {
                get { return formPos; }
                set
                {
                    if (formPos != value)
                    {
                        formPos = value;
                        SettingsChanged();
                    }
                }
            }

            private Size formSize = new Size(800, 600);
            public Size FormSize
            {
                get { return formSize; }
                set
                {
                    if (formSize != value)
                    {
                        formSize = value;
                        SettingsChanged();
                    }
                }
            }

            private bool formMaximized = false;
            public bool FormMaximized
            {
                get { return formMaximized; }
                set
                {
                    if (formMaximized != value)
                    {
                        formMaximized = value;
                        SettingsChanged();
                    }
                }
            }

            private bool notViewObjectBrowser = false;
            public bool ViewObjectBrowser
            {
                get { return !notViewObjectBrowser; }
                set
                {
                    if (notViewObjectBrowser == value)
                    {
                        notViewObjectBrowser = !value;
                        SettingsChanged();
                    }
                }
            }

            private bool notShowErrors = false;
            public bool ShowErrors
            {
                get { return !notShowErrors; }
                set
                {
                    if (notShowErrors == value)
                    {
                        notShowErrors = !value;
                        SettingsChanged();
                    }
                }
            }

            private bool notShowWarnings = false;
            public bool ShowWarnings
            {
                get { return !notShowWarnings; }
                set
                {
                    if (notShowWarnings == value)
                    {
                        notShowWarnings = !value;
                        SettingsChanged();
                    }
                }
            }
        }


        [Serializable]
        public class CompilerOptions
        {
            private void SettingsChanged()
            {
                if (ProjectProperties.CurrentProjectPropperties != null)
                    ProjectProperties.CurrentProjectPropperties.CompileStatus = ProjectProperties.ECompileStatus.Changed;
                Options.SettingsChanged();
            }

            private bool notOneOutputFile = false;
            public bool OneOutputFile
            {
                get { return !notOneOutputFile; }
                set
                {
                    if (notOneOutputFile == value)
                    {
                        notOneOutputFile = !value;
                        SettingsChanged();
                    }
                }
            }

            private bool makeShortNames = false;
            public bool MakeShortNames
            {
                get { return makeShortNames; }
                set
                {
                    if (makeShortNames != value)
                    {
                        makeShortNames = value;
                        SettingsChanged();
                    }
                }
            }

            private bool obfuscateStrings = false;
            public bool ObfuscateStrings
            {
                get { return obfuscateStrings; }
                set
                {
                    if (obfuscateStrings != value)
                    {
                        obfuscateStrings = value;
                        SettingsChanged();
                    }
                }
            }

            private bool removeUnusedMethods = false;
            public bool RemoveUnusedMethods
            {
                get { return removeUnusedMethods; }
                set
                {
                    if (removeUnusedMethods != value)
                    {
                        removeUnusedMethods = value;
                        SettingsChanged();
                    }
                }
            }


            private bool removeUnusedFields = false;
            public bool RemoveUnusedFields
            {
                get { return removeUnusedFields; }
                set
                {
                    if (removeUnusedFields != value)
                    {
                        removeUnusedFields = value;
                        SettingsChanged();
                    }
                }
            }

            private bool removeUnusedStructs = false;
            public bool RemoveUnusedStructs
            {
                get { return removeUnusedStructs; }
                set
                {
                    if (removeUnusedStructs != value)
                    {
                        removeUnusedStructs = value;
                        SettingsChanged();
                    }
                }
            }

            private uint numberOfMapBackups = 1;
            public uint NumberOfMapBackups
            {
                get { return numberOfMapBackups; }
                set
                {
                    if (numberOfMapBackups != value)
                    {
                        numberOfMapBackups = value;
                        SettingsChanged();
                    }
                }
            }

            private bool runCopy = true;
            public bool RunCopy
            {
                get { return runCopy; }
                set
                {
                    if (runCopy != value)
                    {
                        runCopy = value;
                        SettingsChanged();
                    }
                }
            }

            private bool neverAskToRunSavedMap = false;
            public bool NeverAskToRunSavedMap
            {
                get { return neverAskToRunSavedMap; }
                set
                {
                    if (neverAskToRunSavedMap != value)
                    {
                        neverAskToRunSavedMap = value;
                        SettingsChanged();
                    }
                }
            }

            private bool dontAutomaticallyInlineShortMethods = false;
            public bool AutomaticallyInlineShortMethods
            {
                get { return !dontAutomaticallyInlineShortMethods; }
                set
                {
                    if (dontAutomaticallyInlineShortMethods == value)
                    {
                        dontAutomaticallyInlineShortMethods = !value;
                        SettingsChanged();
                    }
                }
            }
        }

        [Serializable]
        public class EditorOptions
        {
            private Font font;
            public Font Font
            {
                get { return font; }
                set
                {
                    if (font != value)
                    {
                        font = value;
                        SettingsChanged();
                    }
                }
            }

            private int charWidth = 7;
            public int CharWidth
            {
                get { return charWidth; }
                set
                {
                    if (charWidth != value)
                    {
                        charWidth = value;
                        SettingsChanged();
                    }
                }
            }


            private bool readonlyOutput = true;
            public bool ReadOnlyOutput
            {
                get { return readonlyOutput; }
                set
                {
                    if (readonlyOutput != value)
                    {
                        readonlyOutput = value;
                        SettingsChanged();
                    }
                }
            }

            private bool replaceTabsWithSpaces = false;
            public bool ReplaceTabsWithSpaces
            {
                get { return !replaceTabsWithSpaces; }
                set
                {
                    if (replaceTabsWithSpaces == value)
                    {
                        replaceTabsWithSpaces = !value;
                        SettingsChanged();
                    }
                }
            }

            private bool dontInsertEndBracket = false;
            public bool InsertEndBracket
            {
                get { return !dontInsertEndBracket; }
                set
                {
                    if (dontInsertEndBracket == value)
                    {
                        dontInsertEndBracket = !value;
                        SettingsChanged();
                    }
                }
            }

            private bool openInLastProject = false;
            public bool OpenInLastProject
            {
                get { return openInLastProject; }
                set
                {
                    if (openInLastProject != value)
                    {
                        openInLastProject = value;
                        SettingsChanged();
                    }
                }
            }

            private string lastProject = "";
            public string LastProject
            {
                get { return lastProject; }
                set
                {
                    if (lastProject != value)
                    {
                        lastProject = value;
                        SettingsChanged();
                    }
                }
            }

            private Dictionary<FontStyles, FontModification> fontMods = new Dictionary<FontStyles, FontModification>();
            private void InitFonts(bool save)
            {
                bool changed = false;
                if (fontMods == null)
                    fontMods = new Dictionary<FontStyles, FontModification>();
                if (!fontMods.ContainsKey(FontStyles.Normal))
                {
                    fontMods[FontStyles.Normal] = new FontModification(FontStyle.Regular, Color.Black);
                    changed = true;
                }
                if (!fontMods.ContainsKey(FontStyles.Primitives))
                {
                    fontMods[FontStyles.Primitives] = new FontModification(FontStyle.Bold, Color.FromArgb(0, 0, 255));
                    changed = true;
                }
                if (!fontMods.ContainsKey(FontStyles.Strings))
                {
                    fontMods[FontStyles.Strings] = new FontModification(0, Color.FromArgb(193, 21, 110));
                    changed = true;
                }
                if (!fontMods.ContainsKey(FontStyles.Comments))
                {
                    fontMods[FontStyles.Comments] = new FontModification(0, Color.FromArgb(0, 128, 0));
                    changed = true;
                }
                if (!fontMods.ContainsKey(FontStyles.Keywords))
                {
                    fontMods[FontStyles.Keywords] = new FontModification(0, Color.FromArgb(0, 0, 255));
                    changed = true;
                }
                if (!fontMods.ContainsKey(FontStyles.NativeCalls))
                {
                    fontMods[FontStyles.NativeCalls] = new FontModification(FontStyle.Regular, Color.Black);
                    changed = true;
                }
                if (!fontMods.ContainsKey(FontStyles.Structs))
                {
                    fontMods[FontStyles.Structs] = new FontModification(0, Color.FromArgb(43, 145, 175));
                    changed = true;
                }
                //add self defined function calls font
                //windywell
                if (!fontMods.ContainsKey(FontStyles.SefFuncCalls))
                {
                    fontMods[FontStyles.SefFuncCalls] = new FontModification(FontStyle.Bold, Color.FromArgb(225, 7, 7));
                    changed = true;
                }
                if (save && changed)
                    SettingsChanged();
            }

            public FontModification GetMod(FontStyles style)
            {
                InitFonts(true);
                return fontMods[style];
            }

            public void SetMod(FontStyles style, FontModification mod)
            {
                InitFonts(false);
                fontMods[style] = mod;
                SettingsChanged();
                if (Form1.Form.CurrentOpenFile != null && Form1.Form.CurrentOpenFile.OpenFile != null)
                {
                    Form1.Form.CurrentOpenFile.OpenFile.Editor.Restyle();
                }
            }
        }


        [Serializable]
        public enum FontStyles
        {
            Normal,
            Primitives,
            Strings,
            Comments,
            Keywords,
            NativeCalls,
            Structs,
            SefFuncCalls
        }

        
        [Serializable]
        public class RunOptions
        {
            public RunOptions()
            {
                difficulty = 4;//Hard
                gameSpeed = 4;//Faster
                useFixedSeed = false;
                windowed = true;
                showDebug = true;
                enablePreload = true;
                additionalArgs = "";
            }

            private void SettingsChanged()
            {
                if (ProjectProperties.CurrentProjectPropperties != null)
                    ProjectProperties.CurrentProjectPropperties.CompileStatus = ProjectProperties.ECompileStatus.Changed;
                Options.SettingsChanged();
            }

            private int difficulty;
            public int Difficulty
            {
                get { return difficulty; }
                set
                {
                    if (difficulty != value)
                    {
                        difficulty = value;
                        SettingsChanged();
                    }
                }
            }

            private int gameSpeed;
            public int GameSpeed
            {
                get { return gameSpeed; }
                set
                {
                    if (gameSpeed != value)
                    {
                        gameSpeed = value;
                        SettingsChanged();
                    }
                }
            }

            private bool useFixedSeed;
            public bool UseFixedSeed
            {
                get { return useFixedSeed; }
                set
                {
                    if (useFixedSeed != value)
                    {
                        useFixedSeed = value;
                        SettingsChanged();
                    }
                }
            }

            private int seed;
            public int Seed
            {
                get { return seed; }
                set
                {
                    if (seed != value)
                    {
                        seed = value;
                        SettingsChanged();
                    }
                }
            }

            private bool windowed;
            public bool Windowed
            {
                get { return windowed; }
                set
                {
                    if (windowed != value)
                    {
                        windowed = value;
                        SettingsChanged();
                    }
                }
            }

            private bool showDebug;
            public bool ShowDebug
            {
                get { return showDebug; }
                set
                {
                    if (showDebug != value)
                    {
                        showDebug = value;
                        SettingsChanged();
                    }
                }
            }

            private bool enablePreload;
            public bool EnablePreload
            {
                get { return enablePreload; }
                set
                {
                    if (enablePreload != value)
                    {
                        enablePreload = value;
                        SettingsChanged();
                    }
                }
            }

            private bool allowCheat;
            public bool AllowCheat
            {
                get { return allowCheat; }
                set
                {
                    if (allowCheat != value)
                    {
                        allowCheat = value;
                        SettingsChanged();
                    }
                }
            }

            private string additionalArgs;
            public string AdditionalArgs
            {
                get { return additionalArgs; }
                set
                {
                    if (additionalArgs != value)
                    {
                        additionalArgs = value;
                        SettingsChanged();
                    }
                }
            }
        }

        internal class ModCompileOptions
        {
            public bool AllowRename;
            public bool UploadNamingMap;
            public string UsingMapPath;
            public string Password;
        }
        
    }
}

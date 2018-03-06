using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.Xna.Framework.Graphics;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using Galaxy_Editor_2.Dialog_Creator.Texture;

namespace Galaxy_Editor_2.Dialog_Creator
{
    class TextureLoader
    {
        private static readonly TextureLoaderInterface mLoader=new TextureLoader3_0();
        private static List<string> cachedPaths = new List<string>();

        public static Texture2D Load(string path, GraphicsDevice device)
        {
            //if path starts with Assets\Textures\, we need try to find its long path
            if (path.StartsWith("Assets/") || path.StartsWith("Assets\\"))
            {
                if (cachedPaths.Count== 0)
                {
                    GetAllPaths();
                }
                int startPos=(path.LastIndexOf('/') > path.LastIndexOf('\\') ? path.LastIndexOf('/'):path.LastIndexOf('\\'))+1;
                foreach (var posPath in cachedPaths)
                {
                    if (posPath.EndsWith(path.Substring(startPos)))//any possible path is viable
                    {
                        return mLoader.Load(posPath, device);
                    } 
                }

                return null;//not find
            }
            return mLoader.Load(path, device);
        }

        public static List<string> GetAllPaths()
        {
            //if already in cache return immediately
            if (cachedPaths.Count!= 0)
                return cachedPaths;
            //read write file read from cache first
            List<string> allPath=new List<string>();
            try
            {
                string texturepath = "textureFileList.prebuild";

                if (File.Exists(texturepath))
                {
                    StreamReader sr = new StreamReader(texturepath);
                    string line = sr.ReadLine();
                    if(line.Length!=0)
                        allPath.Add(line);
                    //Continue to read until you reach end of file
                    while (line != null)
                    {
                        line = sr.ReadLine();
                        if(line.Length!=0)
                            allPath.Add(line);
                    }
                    //close the file
                    sr.Close();
                }
                else
                {
                    allPath=mLoader.GetAllPaths();

                    //save file
                    StreamWriter sw = new StreamWriter(texturepath);
                    foreach(var filepath in allPath)
                    {
                        sw.WriteLine(filepath);
                    }
                    sw.Close();
                }
            }
            catch(Exception ex)
            {
            }
            cachedPaths = allPath;
            return allPath;
        }

        public static void Unload(string path)
        {
             mLoader.Unload(path);
        }
    }

    
}

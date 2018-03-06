using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Galaxy_Editor_2
{
    class MapObjectsManager
    {
        public class MapObject : ListViewItem
        {
            public string InsertText;

            public MapObject(string insertText, params string[] colums) : base(colums)
            {
                InsertText = insertText;
            }

        }

         public class MapObjectComparer : IComparer<MapObject>
         {
             public int Compare(MapObject x, MapObject y)
             {
                 return x.SubItems[0].Text.CompareTo(y.SubItems[0].Text);
             }
         }


        public enum ObjectType
        {
            Units,
            Doodads,
            Points,
            Regions,
            Cameras
        }

        public static List<MapObject>[] ObjectList = new List<MapObject>[Enum.GetNames(typeof(ObjectType)).Length];

        static MapObjectsManager()
        {
            for (int i = 0; i < ObjectList.Length; i++)
            {
                ObjectList[i] = new List<MapObject>();
            }
        }

        public static void ExtractData(FileSystemInfo mapFile)
        {
            for (int i = 0; i < ObjectList.Length; i++)
            {
                ObjectList[i] = new List<MapObject>();
            }

            if (mapFile == null)
                return;

            //Extract "Regions" for regions, and "Objects" for the rest.
            //Regions
            TextReader stream = OpenRead(mapFile, "Regions");

            XmlReader reader = XmlReader.Create(stream);
            try
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "region")
                        {
                            //Read id attribute
                            string id = reader.GetAttribute("id");
                            string name = "";
                            while (reader.Read())
                            {
                                bool breakOut = false;
                                switch (reader.NodeType)
                                {
                                    case XmlNodeType.Element:
                                        if (reader.Name == "name")
                                            name = reader.GetAttribute("value");
                                        break;
                                    case XmlNodeType.EndElement:
                                        if (reader.Name == "region")
                                            breakOut = true;
                                        break;
                                }
                                if (breakOut)
                                    break;
                            }
                            ObjectList[(int)ObjectType.Regions].Add(new MapObject("RegionFromId(" + id + ")", name, id));
                        }
                    }
                }
            }
            catch(XmlException)
            {
                reader.Close();
            }
            

            //Extract all the rest
            stream = OpenRead(mapFile, "Objects");

            reader = XmlReader.Create(stream);
            try
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "ObjectUnit":
                                string id = reader.GetAttribute("Id");
                                string position = reader.GetAttribute("Position");
                                //Remove z coord
                                position = position.Remove(position.LastIndexOf(','));
                                //Add a space after ,
                                position = position.Replace(",", ", ");
                                string owner = reader.GetAttribute("Player");
                                string type = reader.GetAttribute("UnitType");
                                ObjectList[(int) ObjectType.Units].Add(new MapObject("UnitFromId(" + id + ")", type,
                                                                                     owner, id, position));
                                break;
                            case "ObjectDoodad":
                                id = reader.GetAttribute("Id");
                                position = reader.GetAttribute("Position");
                                //Remove z coord
                                position = position.Remove(position.LastIndexOf(','));
                                //Add a space after ,
                                position = position.Replace(",", ", ");
                                type = reader.GetAttribute("Type");
                                ObjectList[(int) ObjectType.Doodads].Add(new MapObject("DoodadFromId(" + id + ")", type,
                                                                                       id, position));
                                break;
                            case "ObjectPoint":
                                id = reader.GetAttribute("Id");
                                position = reader.GetAttribute("Position");
                                //Remove z coord
                                position = position.Remove(position.LastIndexOf(','));
                                //Add a space after ,
                                position = position.Replace(",", ", ");
                                type = reader.GetAttribute("Type");
                                string name = reader.GetAttribute("Name");
                                ObjectList[(int) ObjectType.Points].Add(new MapObject("PointFromId(" + id + ")", name,
                                                                                      type, id, position));
                                break;
                            case "ObjectCamera":
                                id = reader.GetAttribute("Id");
                                position = reader.GetAttribute("Position");
                                //Remove z coord
                                position = position.Remove(position.LastIndexOf(','));
                                //Add a space after ,
                                position = position.Replace(",", ", ");
                                name = reader.GetAttribute("Name");
                                ObjectList[(int) ObjectType.Cameras].Add(new MapObject("CameraInfoFromId(" + id + ")", name,
                                                                                       id, position));
                                break;
                        }
                    }
                }
            }
            catch (XmlException)
            {
                reader.Close();
            }

            foreach (List<MapObject> t in ObjectList)
            {
                t.Sort(new MapObjectComparer());
            }
        }

        private static TextReader OpenRead(FileSystemInfo mapFile, string filename)
        {
            if (mapFile is FileInfo)
            {
                return new StreamReader(MpqEditor.OpenFileRead((FileInfo) mapFile, filename));
            }
            else//Dir info
            {
                DirectoryInfo dir = (DirectoryInfo) mapFile;
                foreach (FileInfo file in dir.GetFiles())
                {
                    if (file.Name == filename)
                    {
                        return new StreamReader(file.OpenRead());
                    }
                }
                return new StringReader("");
            }
        }
    }
}

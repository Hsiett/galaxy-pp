using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SharedClasses
{
    [Serializable]
    public class LibraryDescription
    {
        public string Name;
        public string Version;
        public string Author;

        public override string ToString()
        {
            return Name + " " + Version + " by " + Author;
        }
    }

    [Serializable]
    public class Library
    {
        public string Name;
        public string Version;
        public DateTime UploadDate;
        public string Author;
        public string Description;
        public string ChangeLog;
        //Library name, version
        public List<LibraryDescription> Dependancies = new List<LibraryDescription>();
        public List<Item> Items;

        [Serializable]
        [XmlInclude(typeof(Folder)), XmlInclude(typeof(File))]  
        public class Item
        {
            
        }

        [Serializable]
        public class Folder : Item
        {
            public string Name;
            public List<Item> Items;
        }

        [Serializable]
        public class File : Item
        {
            public string Name;
            public string Text;
        }

        public void Save()
        {
            if (!Directory.Exists("Libraries\\" + Name))
                Directory.CreateDirectory("Libraries\\" + Name);
            string fileName = "Libraries\\" + Name + "\\" + Version + ".xml";
            if (System.IO.File.Exists(fileName))
                System.IO.File.Delete(fileName);
            using (StreamWriter writer = System.IO.File.CreateText(fileName))
            {
                XmlSerializer serializer = new XmlSerializer(GetType());
                serializer.Serialize(writer, this);
                writer.Close();
            }
        }

        public void Delete(bool isLast)
        {
            System.IO.File.Delete("Libraries\\" + Name + "\\" + Version + ".xml");
            if (isLast)
                Directory.Delete("Libraries\\" + Name, true);
        }

        public static Library Load(string path)
        {
            Library user;
            using (StreamReader reader = System.IO.File.OpenText(path))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Library));
                user = (Library)serializer.Deserialize(reader);
                reader.Close();
            }
            return user;
        }

        public override string ToString()
        {
            return Name + " " + Version + " by " + Author;
        }

        public List<KeyValuePair<File, string>> GetFiles()
        {
            List<KeyValuePair<File, string>> files = new List<KeyValuePair<File, string>>();
            foreach (Item item in Items)
            {
                AddFiles(item, ToString(), files);
            }
            return files;
        }

        private void AddFiles(Item item, string path, List<KeyValuePair<File, string>> list)
        {
            if (item is File)
                list.Add(new KeyValuePair<File, string>((File) item, Path.Combine(path, ((File)item).Name)));
            else if (item is Folder)
                foreach (Item i in ((Folder)item).Items)
                {
                    AddFiles(i, Path.Combine(path, ((Folder)item).Name), list);
                }
        }
    }
}

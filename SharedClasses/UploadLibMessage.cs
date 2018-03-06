using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SharedClasses
{
    [Serializable]
    public class UploadLibMessage
    {
        private byte[] serializedLib;

        public Library Library
        {
            get
            {
                MemoryStream stream = new MemoryStream(serializedLib);
                XmlSerializer serializer = new XmlSerializer(typeof(Library));
                Library lib = (Library) serializer.Deserialize(stream);
                stream.Close();
                stream.Dispose();
                return lib;
            }
            set
            {
                MemoryStream stream = new MemoryStream();
                XmlSerializer serializer = new XmlSerializer(typeof(Library));
                serializer.Serialize(stream, value);
                serializedLib = stream.ToArray();
                stream.Close();
                stream.Dispose();
            }
        }


        public EncryptedMessage Password;
        public bool Overwrite;
    }
}

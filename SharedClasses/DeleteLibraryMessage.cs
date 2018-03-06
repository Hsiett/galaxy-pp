using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedClasses
{
    [Serializable]
    public class DeleteLibraryMessage
    {
        public string LibName;
        public string LibVersion;
        public string Username;
        public string Password;
    }
}

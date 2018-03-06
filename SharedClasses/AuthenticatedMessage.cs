using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SharedClasses
{
    [Serializable]
    class AuthenticatedMessage
    {
        public string Username;
        public string Password;
        public ISerializable Data;
    }
}

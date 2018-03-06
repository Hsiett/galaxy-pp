using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedClasses
{
    [Serializable]
    public class CheckPasswordMessage
    {
        public string Username;
        public string Password;
    }
}

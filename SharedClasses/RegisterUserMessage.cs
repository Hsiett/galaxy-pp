using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedClasses
{
    [Serializable]
    public class RegisterUserMessage
    {
        public string Name;
        public string Email;
        public string Password;
    }
}

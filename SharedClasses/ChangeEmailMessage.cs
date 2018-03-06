using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedClasses
{
    [Serializable]
    public class ChangeEmailMessage
    {
        public string Username;
        public string Password;
        public string NewEmail;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedClasses
{
    [Serializable]
    public class ChangePasswordMessage
    {
        public string Username;
        public string OldPassword;
        public string NewPassword;
    }
}

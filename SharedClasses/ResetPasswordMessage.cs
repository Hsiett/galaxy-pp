using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedClasses
{
    [Serializable]
    public class ResetPasswordMessage
    {
        public string Username;
        public string ResetCode;
        public string NewPassword;
    }
}

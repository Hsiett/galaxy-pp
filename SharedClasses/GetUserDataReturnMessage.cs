using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedClasses
{
    [Serializable]
    public class GetUserDataReturnMessage
    {
        public string Email;
        public Dictionary<string, List<string>> Libraries = new Dictionary<string, List<string>>();
    }
}

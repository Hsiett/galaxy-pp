using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Galaxy_Editor_2.Dialog_Creator.util
{
    class Util
    {
        public static byte[] ReadAllBytes(Stream fs)
        {
            byte[] result = new byte[fs.Length];
            fs.Position = 0;
            int cur = 0;
            while (cur < fs.Length)
            {
                int read = fs.Read(result, cur, result.Length - cur);
                cur += read;
            }

            return result;
        }

    }
}

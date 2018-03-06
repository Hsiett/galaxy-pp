using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class GenerateBankPreloadFile
    {
        public static void Generate(SharedData data, DirectoryInfo dir)
        {
            //Remove dublicates
            for (int i = 0; i < data.BankPreloads.Count; i++)
            {
                for (int j = i + 1; j < data.BankPreloads.Count; j++)
                {
                    if (data.BankPreloads[i].Key == data.BankPreloads[j].Key &&
                        data.BankPreloads[i].Value == data.BankPreloads[j].Value)
                    {
                        data.BankPreloads.RemoveAt(j);
                        j = i;
                        continue;
                    }
                }
            }


            StringBuilder xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"us-ascii\"?>");
            xml.AppendLine("<BankList>");
            foreach (KeyValuePair<string, int> pair in data.BankPreloads)
            {
                xml.AppendLine("    <Bank Name=" + pair.Key + " Player=\"" + pair.Value + "\"/>");
            }
            xml.AppendLine("</BankList>");

            string s = xml.ToString();
            byte[] bytes = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                bytes[i] = (byte) s[i];
            }

            FileInfo file = new FileInfo(dir.FullName + "\\" + "BankList.xml");
            FileStream stream = file.Open(FileMode.Create);
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
        }
    }
}

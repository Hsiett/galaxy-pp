using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.analysis;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Compiler.Phases.Transformations
{
    class RenameUnicode : DepthFirstAdapter
    {
        private SharedData data;

        public RenameUnicode(SharedData data)
        {
            this.data = data;
        }

        public override void CaseTIdentifier(Generated.node.TIdentifier node)
        {
            StringBuilder name = new StringBuilder("");
            foreach (char c in node.Text)
            {
                if (c > 0xFF)
                {
                    byte[] utf8Bytes = Encoding.UTF8.GetBytes(c.ToString());
                    foreach (byte b in utf8Bytes)
                    {
                        name.Append("U");
                        name.AppendFormat("{0:x2}", b);
                    }
                }
                else
                    name.Append(c);
            }
            node.Text = name.ToString();
        }

        public override void CaseAStringConstExp(AStringConstExp node)
        {
            List<string> strings = new List<string>();
            StringBuilder name = new StringBuilder("");
            bool previousWasUnicode = false;
            foreach (char c in node.GetStringLiteral().Text)
            {
                if (c > 0xFF)
                {
                    byte[] utf8Bytes = Encoding.UTF8.GetBytes(c.ToString());
                    foreach (byte b in utf8Bytes)
                    {
                        name.Append("\\x");
                        name.AppendFormat("{0:x2}", b);
                    }
                    previousWasUnicode = true;
                }
                else
                {
                    if (previousWasUnicode && ((c >= '0' && c <= '9') || (char.ToLower(c) >= 'a' && char.ToLower(c) <= 'f')))
                    {
                        strings.Add(name.ToString());
                        name.Clear();
                    }

                    name.Append(c);

                    previousWasUnicode = false;
                }
            }
            strings.Add(name.ToString());

            if (strings.Count == 1)
                node.GetStringLiteral().Text = name.ToString();
            else
            {
                strings[0] = strings[0].Remove(0, 1);
                strings[strings.Count - 1] = strings[strings.Count - 1].Substring(0, strings[strings.Count - 1].Length - 1);
                AStringConstExp left = new AStringConstExp(new TStringLiteral("\"" + strings[0] + "\""));
                strings.RemoveAt(0);
                data.StringsDontJoinRight.Add(left);
                data.ExpTypes[left] = new ANamedType(new TIdentifier("string"), null);
                node.ReplaceBy(Combine(left, strings));
            }
        }

        private PExp Combine(PExp left, List<string> strings)
        {
            AStringConstExp right = new AStringConstExp(new TStringLiteral("\"" + strings[0] + "\""));
            strings.RemoveAt(0);
            ABinopExp binop = new ABinopExp(left, new APlusBinop(new TPlus("+")), right);
            data.StringsDontJoinRight.Add(right);
            data.ExpTypes[right] =
                data.ExpTypes[binop] = new ANamedType(new TIdentifier("string"), null);
            if (strings.Count > 0)
                return Combine(binop, strings);
            return binop;
        }
    }
}

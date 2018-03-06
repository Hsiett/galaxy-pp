using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Generated.node;

namespace Galaxy_Editor_2.Editor_control
{
    public struct TextPoint
    {
        public int Line, Pos;

        public TextPoint(int line, int pos)
        {
            Line = line;
            Pos = pos;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TextPoint))
                return false;
            TextPoint other = (TextPoint)obj;
            return other.Line == Line && other.Pos == Pos;
        }

        public static TextPoint FromCompilerCoords(Token token)
        {
            return FromCompilerCoords(token.Line, token.Pos);
        }

        public static TextPoint FromCompilerCoords(int line, int pos)
        {
            return new TextPoint(line - 1, pos);
        }

        public static bool operator ==(TextPoint tp1, TextPoint tp2)
        {
            return tp1.Equals(tp2);
        }

        public static bool operator !=(TextPoint tp1, TextPoint tp2)
        {
            return !(tp1 == tp2);
        }

        public static bool operator <(TextPoint tp1, TextPoint tp2)
        {
            return tp1.Line < tp2.Line || (tp1.Line == tp2.Line && tp1.Pos < tp2.Pos);
        }

        public static bool operator >(TextPoint tp1, TextPoint tp2)
        {
            return tp1.Line > tp2.Line || (tp1.Line == tp2.Line && tp1.Pos > tp2.Pos);
        }

        public static bool operator <=(TextPoint tp1, TextPoint tp2)
        {
            return tp1 == tp2 || tp1 < tp2;
        }

        public static bool operator >=(TextPoint tp1, TextPoint tp2)
        {
            return tp1 == tp2 || tp1 > tp2;
        }

        public static TextPoint Min(TextPoint tp1, TextPoint tp2)
        {
            return tp1 < tp2 ? tp1 : tp2;
        }

        public static TextPoint Max(TextPoint tp1, TextPoint tp2)
        {
            return tp1 > tp2 ? tp1 : tp2;
        }
    }
}

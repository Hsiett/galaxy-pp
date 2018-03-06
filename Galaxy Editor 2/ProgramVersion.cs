using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Galaxy_Editor_2
{
    public class ProgramVersion
    {
        public static ProgramVersion CurrentVersion
        {
            get
            {
                return new ProgramVersion(Application.ProductVersion);
            }
        }

        private int[] versionInts = new int[3];

        public ProgramVersion(string ver)
        {
            string[] strings = ver.Split('.');
            for (int i = 0; i < 3; i++)
            {
                versionInts[i] = int.Parse(strings[i]);
            }
        }

        public static bool operator ==(ProgramVersion v1, ProgramVersion v2)
        {
            for (int i = 0; i < 3; i++)
            {
                if (v1.versionInts[i] != v2.versionInts[i])
                    return false;
            }
            return true;
        }

        public static bool operator !=(ProgramVersion v1, ProgramVersion v2)
        {
            return !(v1 == v2);
        }

        public static bool operator <(ProgramVersion v1, ProgramVersion v2)
        {
            for (int i = 0; i < 3; i++)
            {
                if (v1.versionInts[i] < v2.versionInts[i])
                    return true;
                if (v1.versionInts[i] > v2.versionInts[i])
                    return false;
            }
            return false;
        }

        public static bool operator >(ProgramVersion v1, ProgramVersion v2)
        {
            for (int i = 0; i < 3; i++)
            {
                if (v1.versionInts[i] > v2.versionInts[i])
                    return true;
                if (v1.versionInts[i] < v2.versionInts[i])
                    return false;
            }
            return false;
        }

        public static bool operator >=(ProgramVersion v1, ProgramVersion v2)
        {
            for (int i = 0; i < 3; i++)
            {
                if (v1.versionInts[i] > v2.versionInts[i])
                    return true;
                if (v1.versionInts[i] < v2.versionInts[i])
                    return false;
            }
            return true;
        }

        public static bool operator <=(ProgramVersion v1, ProgramVersion v2)
        {
            for (int i = 0; i < 3; i++)
            {
                if (v1.versionInts[i] < v2.versionInts[i])
                    return true;
                if (v1.versionInts[i] > v2.versionInts[i])
                    return false;
            }
            return true;
        }
    }
}

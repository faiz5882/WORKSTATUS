using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Utility
{
   public static class ExtensionMethod
    {

        public static string GetShortDescription(this string s, int i)
        {
            if (s.Length <= i)
            {
                return s;
            }
            else
            {
                return s.Substring(0, i) + "...";
            }
        }

        //public string ShortDescription
        //{
        //    get { return this.Description.Take(25); }
        //}
        public static bool SessionManager(string session)
        {
            if (string.IsNullOrEmpty(session))
                return false;
            else return true;
        }
        public static string GetAssemblyVersion()
        {
            string version = Assembly.GetEntryAssembly().GetName().Version.ToString();
            return version;
        }

        public static int FindIndex<T>(this ObservableCollection<T> ts, Predicate<T> match)
        {
            return ts.FindIndex(0, ts.Count, match);
        }
        public static int FindIndex<T>(this ObservableCollection<T> ts, int startIndex, int count, Predicate<T> match)
        {
            if (startIndex < 0) startIndex = 0;
            if (count > ts.Count) count = ts.Count;

            for (int i = startIndex; i < count; i++)
            {
                if (match(ts[i])) return i;
            }

            return -1;
        }
        public static string DbNullToString(this object obj)
        {
            if (Convert.IsDBNull(obj))
            {
                return "";
            }
            else
            {
                return obj.ToString();
            }
        }
        public static short ToInt16(this Object val)
        {
            if (val == null || Convert.ToString(val) == "")
            {
                val = 0;
            }
            return Convert.ToInt16(val);
        }
        public static short ToInt16(this string val)
        {
            if (val == null || Convert.ToString(val) == "")
            {
                val = "0";
            }
            return Convert.ToInt16(val);
        }


        public static int ToInt32(this Object val)
        {
            if (val == null || Convert.ToString(val) == "")
            {
                val = 0;
            }
            return Convert.ToInt32(val);
        }
        public static int ToInt32(this string val)
        {
            if (val == null || Convert.ToString(val) == "")
            {
                val = "0";
            }
            return Convert.ToInt32(val);
        }
        public static long ToInt64(this Object val)
        {
            if (val == null || Convert.ToString(val) == "")
            {
                val = 0;
            }
            return Convert.ToInt64(val);
        }
        public static long ToInt64(this string val)
        {
            if (val == null || Convert.ToString(val) == "")
            {
                val = "0";
            }
            return Convert.ToInt64(val);
        }
        public static string ToStrVal(this Object val)
        {
            return Convert.ToString(val);
        }
        public static string ToStrVal(this Object val, string IFormatProvider)
        {
            string strVal = Convert.ToString(val);
            return string.Format(IFormatProvider, val);
        }
        public static bool ToBlnVal(this Object val)
        {
            if (val == null || Convert.ToString(val) == "")
            {
                val = 0;
            }
            return Convert.ToBoolean(val);
        }
        public static char ToChar(this Object val)
        {
            if (val == null)
            {
                val = "";
            }
            return Convert.ToChar(val);
        }
        public static float ToFloat(this object val)
        {
            if (val == null || Convert.ToString(val) == "")
            {
                val = 0.0;
            }
            return float.Parse(Convert.ToString(val));
        }
        public static float ToFloat(this string val)
        {
            if (val == null || Convert.ToString(val) == "")
            {
                val = "0.0";
            }
            return float.Parse(val);
        }
        public static string ApplyNameValidation(this string val)
        {
            if (val.Length == 29 && val.Length == 58)
            {
                val += " ";
            }
            return val.ToUpper();
        }
    }
}

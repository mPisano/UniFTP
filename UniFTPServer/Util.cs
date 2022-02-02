using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniFTPServer
{
    static class Util
    {
        ///<summary>
        ///The string array is converted to a single string
        ///</summary>
        ///<param name="strings"></param>
        ///<returns></returns>
        public static string ToSingleString(this string[] strings)
        {
            if (strings == null)
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            foreach (var s in strings)
            {
                sb.Append(s).AppendLine();
            }
            return sb.ToString();
        }

        ///<summary>
        ///Strings are converted to arrays of strings on a line-by-line basis
        ///</summary>
        ///<param name="s"></param>
        ///<returns></returns>
        public static string[] ToArrayStrings(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }
            List<string> list = new List<string>();
            string[] tmp = s.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in tmp)
            {
                if (string.IsNullOrEmpty(t))
                {
                    continue;
                }
                list.Add(t);
            }
            return list.ToArray();
        }

        const int GB = 1024 * 1024 * 1024;//Defines the compute constant of gb
        const int MB = 1024 * 1024;//Defines the computation constant of mb
        const int KB = 1024;//Defines the computation constant for kb

        ///<summary>
        ///Converts the representation of Byte
        ///</summary>
        ///<param name="kSize"></param>
        ///<returns></returns>
        public static string ByteConvert(Int64 kSize)
        {
            if (kSize / GB >= 1)//If the value of the current byte is greater than or equal to 1 GB
                return (Math.Round(kSize / (float)GB, 2)).ToString() + "GB";//将其转换成GB
            else if (kSize / MB >= 1)//If the value of the current byte is greater than or equal to 1 MB
                return (Math.Round(kSize / (float)MB, 2)).ToString() + "MB";//将其转换成MB
            else if (kSize / KB >= 1)//If the current byte value is greater than or equal to 1 kb
                return (Math.Round(kSize / (float)KB, 2)).ToString() + "KB";//将其转换成KGB
            else
                return kSize.ToString() + "Byte";//Displays the byte value
        }

        ///<summary>
        ///Converts the representation of Byte
        ///</summary>
        ///<param name="kSize"></param>
        ///<returns></returns>
        public static string ByteConvert(float kSize)
        {
            if (kSize / GB >= 1)//If the value of the current byte is greater than or equal to 1 GB
                return (Math.Round(kSize / (float)GB, 2)).ToString() + "GB";//将其转换成GB
            else if (kSize / MB >= 1)//If the value of the current byte is greater than or equal to 1 MB
                return (Math.Round(kSize / (float)MB, 2)).ToString() + "MB";//将其转换成MB
            else if (kSize / KB >= 1)//If the current byte value is greater than or equal to 1 kb
                return (Math.Round(kSize / (float)KB, 2)).ToString() + "KB";//将其转换成KGB
            else
                return kSize.ToString() + "Byte";//Displays the byte value
        }
    }
}

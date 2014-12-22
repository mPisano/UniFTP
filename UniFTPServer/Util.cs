using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniFTPServer
{
    static class Util
    {
        /// <summary>
        /// 字符串数组转为单字符串
        /// </summary>
        /// <param name="strings"></param>
        /// <returns></returns>
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

        public static string[] ToArrayStrings(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }
            List<string> list = new List<string>();
            string[] tmp = s.Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
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
    }
}

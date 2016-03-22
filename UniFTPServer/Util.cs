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

        /// <summary>
        /// 字符串按行转为字符串数组
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
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

        const int GB = 1024 * 1024 * 1024;//定义GB的计算常量
        const int MB = 1024 * 1024;//定义MB的计算常量
        const int KB = 1024;//定义KB的计算常量

        /// <summary>
        /// 转换Byte的表示方式
        /// </summary>
        /// <param name="kSize"></param>
        /// <returns></returns>
        public static string ByteConvert(Int64 kSize)
        {
            if (kSize / GB >= 1)//如果当前Byte的值大于等于1GB
                return (Math.Round(kSize / (float)GB, 2)).ToString() + "GB";//将其转换成GB
            else if (kSize / MB >= 1)//如果当前Byte的值大于等于1MB
                return (Math.Round(kSize / (float)MB, 2)).ToString() + "MB";//将其转换成MB
            else if (kSize / KB >= 1)//如果当前Byte的值大于等于1KB
                return (Math.Round(kSize / (float)KB, 2)).ToString() + "KB";//将其转换成KGB
            else
                return kSize.ToString() + "Byte";//显示Byte值
        }

        /// <summary>
        /// 转换Byte的表示方式
        /// </summary>
        /// <param name="kSize"></param>
        /// <returns></returns>
        public static string ByteConvert(float kSize)
        {
            if (kSize / GB >= 1)//如果当前Byte的值大于等于1GB
                return (Math.Round(kSize / (float)GB, 2)).ToString() + "GB";//将其转换成GB
            else if (kSize / MB >= 1)//如果当前Byte的值大于等于1MB
                return (Math.Round(kSize / (float)MB, 2)).ToString() + "MB";//将其转换成MB
            else if (kSize / KB >= 1)//如果当前Byte的值大于等于1KB
                return (Math.Round(kSize / (float)KB, 2)).ToString() + "KB";//将其转换成KGB
            else
                return kSize.ToString() + "Byte";//显示Byte值
        }
    }
}

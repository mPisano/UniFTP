using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UniFTP.Server.Virtual
{
    /// <summary>
    /// 虚拟路径
    /// <para>提供处理虚拟路径的静态方法.</para>
    /// </summary>
    public static class VPath
    {
        private static readonly Regex InvalidPathChars = new Regex(string.Join("|", Path.GetInvalidPathChars().Select(c => string.Format(CultureInfo.InvariantCulture, "\\u{0:X4}", (int)c))), RegexOptions.Compiled);

        /// <summary>
        /// 是否含非法路径字符
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool ContainsInvalidPathChars(string path)
        {
            return InvalidPathChars.IsMatch(path);
        }

        /// <summary>
        /// 将所有路径非法字符转为空格
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveInvalidPathChars(string str)
        {
            foreach (var invalidPathChar in Path.GetInvalidPathChars())
            {
                str = str.Replace(invalidPathChar, ' ');
            }
            if (str.Trim() == "")
            {
                str = new Guid().ToString();    //MARK:非要捣乱全用非法字符，只能给你换成GUID了
            }
            return str;
        }

        /// <summary>
        /// 取得路径中文件的名字，不含其路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileName(string path)
        {
            string pre = path.Trim();
            while (pre.EndsWith("\\") || pre.EndsWith("/")) //去除尾部/
            {
                pre = pre.Remove(path.Length - 1);
            }
            int pos = pre.LastIndexOfAny(new char[] {'/', '\\'});
            if (pos < 0)
            {
                return RemoveInvalidPathChars(pre); //FIXED:逻辑修正
            }
            return RemoveInvalidPathChars(pre.Remove(0, pos + 1));
        }

        /// <summary>
        /// 取得一个路径的上一级
        /// </summary>
        /// <param name="vdir"></param>
        /// <returns></returns>
        public static string GetParentPath(string vdir)
        {
            string pre = NormalizeFilename(vdir);
            if (!pre.Contains("/")) //目录中无/，返回..
            {
                return "..";
            }
            else
            {
                pre = pre.Remove(pre.LastIndexOf('/')); //去除最后一个子目录（文件名）
                if (!pre.Contains("/")) //目录中无/，返回根目录
                {
                    return "/";
                }
            }
            return pre;
        }

        /// <summary>
        /// 连接多个目录
        /// <para>不提供路径有效性检验</para>
        /// </summary>
        /// <param name="vdirs"></param>
        /// <returns>合成的目录字符串</returns>
        public static string Combine(params string[] vdirs)
        {
            StringBuilder dirBuilder = new StringBuilder();
            foreach (var dir in vdirs)
            {
                string pre = dir.Replace("\\\\", "/").Replace("\\", "/").Trim();    //确保此段路径分隔符均为/
                if (!pre.StartsWith("/"))
                {
                    dirBuilder.Append("/");     //增加头部的/
                }
                dirBuilder.Append(pre);
                if (dirBuilder.Length > 0)
                {
                    if (dirBuilder[dirBuilder.Length - 1] == '/')
                    {
                        dirBuilder.Remove(dirBuilder.Length - 1, 1);  //去除尾部的/
                    }
                }
            }
            return dirBuilder.ToString();
        }

        /// <summary>
        /// 尝试规范化文件名
        /// </summary>
        /// <param name="vpath">虚拟目录</param>
        /// <param name="isDir">是否为文件夹(若为文件夹，结尾为/；否则结尾去掉/)</param>
        /// <returns></returns>
        public static string NormalizeFilename(string vpath,bool isDir = false)
        {
            if (vpath == null)
            {
                return "";
            }
            StringBuilder dirBuilder = new StringBuilder();
            string pre = vpath.Replace("\\\\", "/").Replace("\\", "/").Trim();    //确保此段路径分隔符均为/
            
            if (!pre.StartsWith("/"))
            {
                return pre;
                //dirBuilder.Append("/");     //增加头部的/
            }
            dirBuilder.Append(pre);
            if (!isDir)
            {
                if (dirBuilder[dirBuilder.Length - 1] == '/')
                {
                    dirBuilder.Remove(dirBuilder.Length - 1, 1);  //去除尾部的/
                }
            }
            else
            {
                if (dirBuilder[dirBuilder.Length - 1] != '/')
                {
                    dirBuilder.Append('/'); //尾部确保为/
                }
            }
            return dirBuilder.ToString();
        }

    }
}

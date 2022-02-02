using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UniFTP.Server.Virtual
{
    ///<summary>
    ///Virtual path
    ///<para>Provides static methods for handling virtual paths</para>
    ///</summary>
    public static class VPath
    {
        //MARK: Currently if upload a file with invalid characters in path/filename, upload will fail. Rename these files by remove invalid characters may clear the road for upload but may cause other problems.
        //private static readonly Regex InvalidPathChars = new Regex(string.Join("|", Path.GetInvalidPathChars().Union(Path.GetInvalidFileNameChars()).Select(c => string.Format(CultureInfo.InvariantCulture, "\\u{0:X4}", (int)c))), RegexOptions.Compiled);
        private static readonly Regex InvalidPathChars = new Regex(string.Join("|", Path.GetInvalidPathChars().Select(c => string.Format(CultureInfo.InvariantCulture, "\\u{0:X4}", (int)c))), RegexOptions.Compiled);

        ///<summary>
        ///Whether it contains illegal path characters
        ///</summary>
        ///<param name="path"></param>
        ///<returns></returns>
        public static bool ContainsInvalidPathChars(string path)
        {
            return InvalidPathChars.IsMatch(path);
        }

        ///<summary>
        ///Converts all path illegal characters to spaces
        ///</summary>
        ///<param name="str"></param>
        ///<returns></returns>
        public static string RemoveInvalidPathChars(string str)
        {
            foreach (var invalidPathChar in Path.GetInvalidPathChars())
            {
                str = str.Replace(invalidPathChar, ' ');
            }
            if (str.Trim() == "")
            {
                str = Guid.NewGuid().ToString();    //Mark: If you have to mess with all the illegal characters, you can only change them to guid
            }
            return str;
        }

        ///<summary>
        ///Get the name of the Chinese path, excluding its path
        ///</summary>
        ///<param name="path"></param>
        ///<returns></returns>
        public static string GetFileName(string path)
        {
            string pre = path.Trim();
            while (pre.EndsWith("\\") || pre.EndsWith("/")) //Removal of tail/
            {
                pre = pre.Remove(path.Length - 1);
            }
            int pos = pre.LastIndexOfAny(new char[] { '/', '\\' });
            if (pos < 0)
            {
                return RemoveInvalidPathChars(pre); //Fixed: Logic fix
            }
            return RemoveInvalidPathChars(pre.Remove(0, pos + 1));
        }

        ///<summary>
        ///Take the upper level of a path
        ///</summary>
        ///<param name="vdir"></param>
        ///<returns></returns>
        public static string GetParentPath(string vdir)
        {
            string pre = NormalizeFilename(vdir);
            if (!pre.Contains("/")) //No // in the directory, returns:
            {
                return "..";
            }
            else
            {
                pre = pre.Remove(pre.LastIndexOf('/')); //Remove the last subdirectory (file name)
                if (!pre.Contains("/")) //There is no /in the directory, returns to the root directory
                {
                    return "/";
                }
            }
            return pre;
        }

        ///<summary>
        ///Connect multiple directories
        ///<para>Path validity tests are not provided</para>
        ///</summary>
        ///<param name="vdirs"></param>
        ///<returns>The synthesized directory string</returns>
        public static string Combine(params string[] vdirs)
        {
            StringBuilder dirBuilder = new StringBuilder();
            foreach (var dir in vdirs)
            {
                if (string.IsNullOrWhiteSpace(dir))
                {
                    continue;
                }
                string pre = dir.Replace("\\\\", "/").Replace("\\", "/").Trim();    //Make sure that this segment path separator is /
                if (!pre.StartsWith("/"))
                {
                    dirBuilder.Append("/");     //Increase the / of the head
                }
                dirBuilder.Append(pre);
                if (dirBuilder.Length > 0)
                {
                    if (dirBuilder[dirBuilder.Length - 1] == '/')
                    {
                        dirBuilder.Remove(dirBuilder.Length - 1, 1);  //Remove the tail of the /
                    }
                }
            }
            if (dirBuilder.Length == 0)
            {
                dirBuilder.Append("/");
            }
            return dirBuilder.ToString();
        }

        ///<summary>
        ///An attempt was made to normalize the file name
        ///</summary>
        ///< the virtual directory >param name="vpath"</param>
        ///<param name="isDir" > is a folder (if it is a folder, it ends with /; otherwise the end is removed /).</param>
        ///<returns></returns>
        public static string NormalizeFilename(string vpath, bool isDir = false)
        {
            if (vpath == null)
            {
                return "";
            }
            StringBuilder dirBuilder = new StringBuilder();
            string pre = vpath.Replace("\\\\", "/").Replace("\\", "/").Trim();    //Make sure that this segment path separator is /

            if (!pre.StartsWith("/"))
            {
                return pre;
                //dirBuilder.Append("/");     Increase the / of the head
            }
            dirBuilder.Append(pre);
            if (!isDir)
            {
                if (dirBuilder[dirBuilder.Length - 1] == '/')
                {
                    dirBuilder.Remove(dirBuilder.Length - 1, 1);  //Remove the tail of the /
                }
            }
            else
            {
                if (dirBuilder[dirBuilder.Length - 1] != '/')
                {
                    dirBuilder.Append('/'); //Tail ensured to be /
                }
            }
            return dirBuilder.ToString();
        }

    }
}

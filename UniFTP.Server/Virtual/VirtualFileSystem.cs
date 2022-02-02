using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;

//Delete Rename Append requires group writable permissions
//Store CreateDirectory requires only writable permissions
using UniFTP.Server.Virtual;

namespace UniFTP.Server.Virtual
{
    public enum FileError
    {
        None = 0,
        NotFound = 1,
        AlreadyExist = 2,
        CannotRead = 4,
        CannotWrite = 8,
        ContainsSubFiles = 16
    }

    ///<summary>
    ///Virtual file system
    ///</summary>
    internal class VirtualFileSystem
    {
        private string _rootPath;

        private FtpConfig _config;

        private VDirectory _rootDirectory;

        private VDirectory _currentDirectory;
        private FtpUserGroup _group;

        public VDirectory CurrentDirectory
        {
            get { return _currentDirectory; }
        }

        public VirtualFileSystem(FtpConfig config, FtpUserGroup group, string rootpath = null)
        {
            _config = config;
            _group = group;
            if (!string.IsNullOrEmpty(rootpath))
            {
                _rootPath = rootpath;
            }
            else if (!string.IsNullOrEmpty(group.HomeDir))
            {
                _rootPath = group.HomeDir;
            }
            else
            {
                _rootPath = config.HomeDir;
            }
            _rootDirectory = new VDirectory(null, new FilePermission("r-xr-xr-x"), _rootPath, "");



            _currentDirectory = _rootDirectory;
            _currentDirectory.Refresh();
            AddGroupLinks();    //Fixed: The link should be included after the refresh
            _currentDirectory.Refresh();
            SetPermission(_currentDirectory, true);
        }

        //public VirtualFileSystem()
        //{
        //    _rootDirectory = new VDirectory(null, new FilePermission("r-xr-xr-x"), null, "/");
        //    _currentDirectory = _rootDirectory;
        //}

        #region FileSystem Methods

        ///<summary>
        ///Create a catalog
        ///</summary>
        ///< virtual path >param name="vPath"</param>
        ///<param name="di" > returns folder information if created successfully</param>
        ///<returns>Whether the creation was successful</returns>
        public FileError CreateDirectory(string vPath, out DirectoryInfo di)
        {
            di = null;
            string pre = VPath.NormalizeFilename(vPath, true);
            //FIXED: create sub directory correctly
            VDirectory f;
            if (pre.StartsWith("/", StringComparison.OrdinalIgnoreCase))    //Absolute virtual path
            {
                f = Get(VPath.GetParentPath(vPath), true) as VDirectory;
            }
            else    //Relative virtual paths
            {
                f = CurrentDirectory;
            }
            //var f = Get(VPath.GetParentPath(vPath), true) as VDirectory;
            if (f == null || !f.Permission.CanWrite)
            {
                return FileError.NotFound;
            }
            string name = VPath.GetFileName(vPath);
            if (f.Enumerate().Contains(name)
                || Directory.Exists(Path.Combine(f.RealDirectory.FullName, name))
                || File.Exists(Path.Combine(f.RealDirectory.FullName, name)))
            {
                return FileError.AlreadyExist; //There is already a file with the same name
            }
            try
            {
                di = f.RealDirectory.CreateSubdirectory(name); //This method can accept "/" as dir separator
                f.Refresh();
            }
            catch (Exception)
            {
                return FileError.NotFound;
            }
            return FileError.None;
        }

        ///<summary>
        ///Create a catalog
        ///</summary>
        ///< virtual path >param name="vPath"</param>
        ///<returns>Whether the creation was successful</returns>
        internal FileError CreateDirectory(string vPath)
        {
            DirectoryInfo di;
            return CreateDirectory(vPath, out di);
        }

        ///<summary>
        ///The virtual file exists
        ///</summary>
        ///< virtual path >param name="vPath"</param>
        ///<returns></returns>
        public bool ExistsFile(string vPath)
        {
            string pre = VPath.NormalizeFilename(vPath);

            if (pre.StartsWith("/", StringComparison.OrdinalIgnoreCase))    //绝对虚拟路径
            {
                if (Get(pre) != null)
                {
                    return true;
                }
            }
            else    //相对虚拟路径
            {
                if (_currentDirectory.SubFiles.Find((t) => t.Name == vPath && !t.IsDirectory) != null)
                {
                    return true;
                }
            }
            return false;
        }

        ///<summary>
        ///Whether a virtual entity exists
        ///</summary>
        ///< virtual path >param name="vPath"</param>
        ///<returns></returns>
        public bool ExistsEntity(string vPath)
        {
            vPath = vPath.Trim();
            if (string.IsNullOrEmpty(vPath))
            {
                return true;
            }
            if (vPath.StartsWith("-a") || vPath.StartsWith("-l"))
            {
                vPath = vPath.Remove(0, 2).Trim();
            }
            if (vPath == "/" || vPath == "." || vPath == "")
            {
                return true;
            }
            string pre = VPath.NormalizeFilename(vPath, true);

            if (pre.StartsWith("/", StringComparison.OrdinalIgnoreCase))    //Absolute virtual path
            {
                if (Get(pre) != null)
                {
                    return true;
                }
            }
            else    //Relative virtual paths
            {
                if (_currentDirectory.SubFiles.Find((t) => t.Name == vPath) != null)
                {
                    return true;
                }
            }
            return false;
        }

        public bool ExistsDirectory(string vPath)
        {
            if (string.IsNullOrEmpty(vPath) || vPath == ".")
            {
                return true;
            }
            if (vPath.Trim() == "/")
            {
                return true;
            }
            string pre = VPath.NormalizeFilename(vPath, true);

            if (pre.StartsWith("/", StringComparison.OrdinalIgnoreCase))    //Absolute virtual path
            {
                if (Get(pre, true) != null)
                {
                    return true;
                }
            }
            else    //Relative virtual paths
            {
                if (_currentDirectory.SubFiles.Find((t) => t.Name == vPath) != null)
                {
                    return true;
                }
            }
            return false;
        }
        ///<summary>
        ///rename
        ///</summary>
        ///<param name="vPath"></param>
        ///<param name="targetPath"></param>
        ///<returns></returns>
        public FileError Rename(string vPath, string targetPath)
        {
            var src = Get(vPath);
            var dst = Get(VPath.GetParentPath(targetPath)) as VDirectory;
            string dstName = VPath.GetFileName(targetPath);
            if (src == null || dst == null || !dst.Permission.CanWrite)
            {
                return FileError.NotFound;  //The destination file could not be found
            }
            if (src.IsDirectory)
            {
                if (dst.Enumerate().Contains(dstName)
                    || Directory.Exists(Path.Combine(dst.RealDirectory.FullName, dstName)))
                {
                    return FileError.AlreadyExist; //There is already a file with the same name
                }
            }
            else
            {
                if (dst.Enumerate().Contains(dstName)
                    || File.Exists(Path.Combine(dst.RealDirectory.FullName, dstName)))
                {
                    return FileError.AlreadyExist; //There is already a file with the same name
                }
            }
            if (!src.Permission.GroupCanWrite || !dst.Permission.GroupCanWrite)
            {
                return FileError.CannotWrite;  //Insufficient permissions
            }
            var tmp = src.ParentDirectory;
            src.ParentDirectory = dst;
            tmp.Refresh();
            try
            {
                if (src.IsDirectory)
                {
                    Directory.Move(src.RealPath, Path.Combine(dst.RealPath, dstName));
                }
                else
                {
                    File.Move(src.RealPath, Path.Combine(dst.RealPath, dstName));
                }
            }
            catch (Exception)
            {
                return FileError.NotFound;
            }
            return FileError.None;
        }

        ///<summary>
        ///Switch the current directory
        ///</summary>
        ///< virtual path >param name="vPath"</param>
        ///<param name="createDirIfNotExists" > directory does not exist/</param> MARK: WARNING: may cause Security Issue!
        ///<returns></returns>
        public bool ChangeCurrentDirectory(string vPath, bool createDirIfNotExists = false)
        {
            string pre = VPath.NormalizeFilename(vPath, true);
            if (pre == "..")
            {
                _currentDirectory = _currentDirectory.ParentDirectory;
            }
            else if (pre == "/") //FIXED:
            {
                _currentDirectory = _rootDirectory;
            }
            else
            {
                VDirectory v;
                if (pre.StartsWith("/", StringComparison.OrdinalIgnoreCase))    //Absolute virtual path
                {
                    v = Get(pre, true) as VDirectory;
                }
                else    //Relative virtual paths
                {
                    v = _currentDirectory.SubFiles.Find((t) => t.IsDirectory && t.Name == pre) as VDirectory;
                }
                if (v == null)
                {
                    if (!createDirIfNotExists)
                    {
                        return false;
                    }
                    //ADDED:
                    FileError fileError = CreateDirectory(pre);
                    if (fileError == FileError.None)
                    {
                        var dir = Get(pre, true) as VDirectory;
                        if (dir != null)
                        {
                            v = dir;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                _currentDirectory = v;
            }
            _currentDirectory.Refresh();
            SetPermission(_currentDirectory, true);
            return true;
        }

        ///<summary>
        ///Delete
        ///</summary>
        ///< virtual path >param name="vPath"</param>
        ///<param name="delDir" > delete folders, true for only folders deleted, false for deleting only files</param>
        ///<returns>Whether the deletion was successful</returns>
        public FileError Delete(string vPath, bool delDir = false)
        {
            string pre = VPath.NormalizeFilename(vPath);
            IFile f;
            if (delDir)
            {
                f = Get(vPath, true) as VDirectory;
            }
            else
            {
                f = Get(vPath) as VFile;
            }
            if (f == null)
            {
                return FileError.NotFound;
            }
            if (!f.Permission.GroupCanWrite)
            {
                return FileError.CannotWrite;
            }
            try
            {
                if (f.IsDirectory && delDir)
                {
                    var vf = (VDirectory)f;
                    if (vf.RealDirectory.GetFileSystemInfos().Length > 0)
                    {
                        return FileError.ContainsSubFiles;
                    }
                    Directory.Delete(vf.RealPath);
                    vf.ParentDirectory.SubFiles.Remove(vf); //Fixed: Deletes the file to update the corresponding virtual directory
                    f = null;
                }
                else if (!f.IsDirectory && !delDir)
                {
                    var vf = (VFile)f;
                    File.Delete(f.RealPath);
                    vf.ParentDirectory.SubFiles.Remove(vf);
                    f = null;
                }
                else
                {
                    return FileError.NotFound;
                }
            }
            catch (Exception)
            {
                return FileError.NotFound;
            }
            return FileError.None;
        }

        ///<summary>
        ///Add a link
        ///</summary>
        ///<param name="realPath" > the real path of the link</param>
        ///<param name="vPath" > the virtual path to which the link is to be added</param>
        ///< alias >param name="name", defaults to the original name</param>
        ///<param name="permission" > permissions, and setting null inherits the parent directory permissions</param>
        public bool AddLink(string realPath, string vPath, FilePermission permission = null, string name = null)
        {

            var folder = Get(vPath) as VDirectory;
            if (folder == null)
            {
                return false;
            }
            if (File.Exists(realPath))
            {
                return folder.AddLink(new VFile(folder, permission, realPath, name));
            }
            else
            {
                if (Directory.Exists(realPath))
                {
                    return folder.AddLink(new VDirectory(folder, permission, realPath, name));
                }
            }
            return false;
        }

        ///<summary>
        ///Get a real path that can be used for uploading
        ///</summary>
        ///< virtual path >param name="vPath"</param>
        ///<param name="rename" > rename, null is not renamed</param>
        ///< whether the > of the param name="overwrite" can be overwritten</param>
        ///<returns></returns>
        public string GetRealPathOfFile(string vPath, bool overwrite = false, string rename = null)
        {
            var file = Get(vPath);
            if (file != null)
            {
                if (!overwrite) //Cannot be overwritten, directly return empty
                {
                    return null;
                }
                else if (!file.Permission.GroupCanWrite) //Permissions are not sufficient to override
                {
                    return null;
                }
            }
            string pre = VPath.NormalizeFilename(vPath);
            VDirectory dir;
            if (!pre.StartsWith("/"))
            {
                dir = _currentDirectory;
                SetPermission(dir, true, true);
            }
            else
            {
                dir = Get(VPath.GetParentPath(pre), true) as VDirectory;
                if (dir == null)
                {
                    return null;
                }
            }
            if (!dir.Permission.CanWrite)
            {
                return null;
            }
            if (rename != null)
            {
                pre = rename;
            }
            string path = Path.Combine(dir.RealPath, VPath.GetFileName(pre));

            return path;
        }

        ///<summary>
        ///Lists the file names
        ///</summary>
        ///< virtual path >param name="vPath"</param>
        ///<returns></returns>
        public List<string> ListFileNames(string vPath = null)
        {
            VDirectory dir;
            if (string.IsNullOrEmpty(vPath))
            {
                dir = _currentDirectory;
                SetPermission(dir, true);
            }
            else
            {
                dir = Get(VPath.NormalizeFilename(vPath), true) as VDirectory ?? _currentDirectory;
            }
            List<string> fileList = new List<string>();
            foreach (var f in dir.SubFiles)
            {
                if (!f.Permission.CanRead)
                {
                    continue;
                }
                fileList.Add(f.Name);
            }
            return fileList;
        }
        ///<summary>
        ///Get the formatted file information
        ///<para>MLST in RFC 3659</para>
        ///</summary>
        ///<param name="vPath"></param>
        ///<returns></returns>
        public string MachineFileInfo(string vPath = null)
        {
            IFile f;
            bool isDir = false;
            StringBuilder sb = new StringBuilder();
            DateTime editDate;
            if (string.IsNullOrEmpty(vPath))
            {
                f = _currentDirectory;
                isDir = true;
            }
            else
            {
                if (ExistsFile(vPath))
                {
                    f = Get(vPath) as VFile;
                    isDir = false;
                }
                else if (ExistsDirectory(vPath))
                {
                    f = Get(vPath, true) as VDirectory;
                    isDir = true;
                }
                else
                {
                    return "";
                }
            }
            if (f == null)
            {
                return "";
            }
            if (isDir)
            {
                var vf = (VDirectory)f;
                editDate = vf.RealDirectory.LastWriteTime.ToUniversalTime(); //Fixed: UTC time is required

                sb.Append("type=dir;");     //Type type
                sb.Append("modify=").Append(editDate.ToString("yyyyMMddHHmmss")).Append(';');   //Modify modification time
                sb.Append("perm=el");   //Perm permissions
                if (vf.Permission.CanWrite)
                {
                    sb.Append("cm");
                }
                if (vf.Permission.GroupCanWrite)
                {
                    sb.Append("dfp");
                }
                sb.Append("; ");
                if (vf == _rootDirectory)
                {
                    sb.Append("/");
                }
                else
                {
                    sb.Append(f.Name);
                }
            }
            else
            {
                VFile vf = (VFile)f;
                editDate = vf.RealFile.LastWriteTime.ToUniversalTime(); //Fixed: UTC time is required

                //Reference formats
                //type=file; modify=20140628041312; size=761344; Server.exe
                sb.Append("type=file;");     //type类型
                sb.Append("modify=").Append(editDate.ToString("yyyyMMddHHmmss")).Append(';');   //Modify modification time
                sb.Append("size=").Append(vf.RealFile.Length).Append(';');   //Size size
                sb.Append("perm=r");   //Perm permissions
                if (vf.Permission.CanWrite)
                {
                    //sb.Append("cm");
                }
                if (vf.Permission.GroupCanWrite)
                {
                    sb.Append("adfw");
                }
                sb.Append("; ");
                sb.Append(f.Name);
            }
            return sb.ToString();
        }

        /*Perm permission settings
perm="a" / "c" / "d" / "e" / "f" / "l" / "m" / "p" / "r" / "w"
a: File, APPE additional license
c: Folder, STOR STOU APPE storage license (cannot already exist)
d: All, RMD DELE removes the license
e: Folder, CWD PWD CDUP directory switch license
f: All, RNFR rename permission
l: Folder, LIST NLST MLSD column directory license
m: Folder, MKD build directory permission
p: Folder, DELE folder content deletion permission
r: File, RTER obtained permission
w: File, STOR storage license
*/

        /// <summary>
        /// 取得格式化的文件列表
        /// <para>MLSD in RFC 3659</para>
        /// </summary>
        /// <param name="vPath"></param>
        /// <returns></returns>
        public List<string> MachineListFiles(string vPath = null)
        {
            VDirectory dir;
            if (string.IsNullOrEmpty(vPath))
            {
                dir = _currentDirectory;
                SetPermission(dir, true);
            }
            else
            {
                dir = Get(VPath.NormalizeFilename(vPath), true) as VDirectory ?? _currentDirectory;
            }

            dir.Refresh();  //Fixed: The catalog is updated in a timely manner

            List<string> fileList = new List<string>();

            DateTime now = DateTime.Now;

            foreach (var f in dir.SubFiles)
            {
                if (!f.Permission.CanRead)
                {
                    continue;
                }
                //Reference formats
                //type=dir; modify=20141218151753; perm=el; Create a new folder
                DateTime editDate;
                StringBuilder sb = new StringBuilder();

                if (f.IsDirectory)
                {
                    VDirectory vf = (VDirectory)f;
                    editDate = vf.RealDirectory.LastWriteTime.ToUniversalTime();

                    sb.Append("Type=dir;");     //Type type
                    sb.Append("modify=").Append(editDate.ToString("yyyyMMddHHmmss")).Append(';');   //modify Modification time
                                                                                                    //sb. Append("size="). Append("0");   Folders do not have size
                                                                                                    //-RFC 3659- -7.5.5- Its value is always an unordered sequence of alphabetic characters. This means that the permission letters are not sorted
                    sb.Append("perm=el");   //perm权限，具体见上
                    if (vf.Permission.CanWrite)
                    {
                        sb.Append("cm");
                    }
                    if (vf.Permission.GroupCanWrite)
                    {
                        sb.Append("dfp");
                    }
                    sb.Append("; ");
                    sb.Append(f.Name);
                }
                else
                {
                    VFile vf = (VFile)f;
                    editDate = vf.RealFile.LastWriteTime.ToUniversalTime();

                    //Reference formats
                    //type=file; modify=20140628041312; size=761344; Server.exe

                    sb.Append("type=file;");     //Type type
                    sb.Append("modify=").Append(editDate.ToString("yyyyMMddHHmmss")).Append(';');   //Modify modification time
                    sb.Append("size=").Append(vf.RealFile.Length).Append(';');   //Size size
                                                                                 //-RFC 3659- -7.5.5- Its value is always an unordered sequence of alphabetic characters. This means that the permission letters are not sorted
                    sb.Append("perm=r");   //Perm permissions, see above
                    if (vf.Permission.CanWrite)
                    {
                        //sb.Append("cm");
                    }
                    if (vf.Permission.GroupCanWrite)
                    {
                        sb.Append("adfw");
                    }
                    sb.Append("; ");
                    sb.Append(f.Name);
                }
                fileList.Add(sb.ToString());
            }
            return fileList;
        }

        ///<summary>
        ///Lists the file information
        ///</summary>
        ///< virtual path >param name="vPath"</param>
        ///<returns></returns>
        public List<string> ListFiles(string vPath = null)
        {
            VDirectory dir;
            if (string.IsNullOrEmpty(vPath))
            {
                dir = _currentDirectory;
                SetPermission(dir, true);
                vPath = "";
            }
            else
            {
                dir = Get(VPath.NormalizeFilename(vPath), true) as VDirectory;
            }
            if (vPath.StartsWith("-a")) //-a
            {
                if (vPath.Trim() == "-a" || vPath.Trim() == "-a .")
                {
                    return ListFiles();
                }
                var vf = Get(VPath.NormalizeFilename(vPath.Remove(0, 2))) as IFile;
                if (vf != null)
                {
                    return new List<string>() { vf.Name ?? VPath.GetFileName(vf.RealPath) };
                }
            }
            if (vPath.StartsWith("-l")) //-l
            {
                if (vPath.Trim() != "-l" && vPath.Trim() != "-l .")
                {
                    var vf = Get(VPath.NormalizeFilename(vPath.Remove(0, 2))) as IFile;
                    if (vf != null)
                    {
                        return GenerateList(new List<IFile>() { vf });
                    }
                }
            }
            //Standard way
            if (dir == null)
            {

                dir = _currentDirectory;
                SetPermission(dir, true);
            }

            dir.Refresh();  //Fixed: The catalog is updated in a timely manner

            return GenerateList(dir.SubFiles);
        }

        private List<string> GenerateList(IEnumerable<IFile> list)
        {
            List<string> fileList = new List<string>();

            DateTime now = DateTime.Now;
            foreach (var f in list)
            {
                if (!f.Permission.CanRead)
                {
                    continue;
                }
                //Reference formats
                //drwxrwxrwx 1 user group 0 Nov 27 00:13 Uploaded

                StringBuilder sb = new StringBuilder();
                DateTime editDate;
                if (f.IsDirectory)
                {
                    VDirectory vf = (VDirectory)f;
                    editDate = vf.RealDirectory.LastWriteTime;

                    sb.Append('d').Append(f.Permission.ToString()); //File permissions 10 bits
                    sb.Append("   1 "); //1 space The number of subfiles is 2 digits and 1 space
                    sb.Append(string.Format("{0,-8}", _config.Owner.PadRight(8))).Append(' '); //The file owner has 8 bits and 1 space
                    sb.Append(string.Format("{0,-8}", _config.OwnerGroup.PadRight(8))).Append(' '); //The file owner has 8 bits and 1 space
                                                                                                    //sb. Append(_config. Owner.Substring(0, 8). PadRight(8)). Append(' ');       The file owner has 8 bits and 1 space
                                                                                                    //sb. Append(_config. OwnerGroup.Substring(0, 8). PadRight(8)). Append(' ');  All groups of files have 8 bits and 1 spaces
                    sb.Append("       0 "); //The file size > = 8 bits, and the folder is usually 0 or 4096 1 space

                    sb.Append(editDate < now.Subtract(TimeSpan.FromDays(180)) //File modification date 5 digits, 1 space, time 5 digits, 1 space
                        ? editDate.ToString("MMM dd  yyyy", CultureInfo.InvariantCulture)
                        : editDate.ToString("MMM dd HH:mm", CultureInfo.InvariantCulture))
                        .Append(' ');
                    sb.Append(f.Name);
                }
                else
                {
                    VFile vf = (VFile)f;
                    editDate = vf.RealFile.LastWriteTime;

                    //string. Format("{0,-50}", theObj; // formatted as 50 characters, with the original characters left-aligned and spaces filled in if insufficient
                    //string. Format("{0,50}", theObj; // formatted as 50 characters, with the original characters right-aligned and spaces filled in if insufficient
                    sb.Append('-').Append(f.Permission.ToString()); //File permissions 10 bits
                    sb.Append("   1 "); //1 space The number of subfiles is 2 digits and 1 space
                    sb.Append(string.Format("{0,-8}", _config.Owner.PadRight(8))).Append(' '); //The file owner has 8 bits and 1 space
                    sb.Append(string.Format("{0,-8}", _config.OwnerGroup.PadRight(8))).Append(' '); //The file owner has 8 bits and 1 space
                                                                                                    //sb. Append(_config. OwnerGroup.Substring(0, 8). PadRight(8)). Append(' ');  All groups of files have 8 bits and 1 spaces
                    string length = vf.RealFile.Length.ToString(CultureInfo.InvariantCulture);
                    if (length.Length < 8)
                    {
                        length = length.PadLeft(8);
                    }
                    sb.Append(length).Append(' '); //File size>=8 bits 1 space
                    sb.Append(editDate < now.Subtract(TimeSpan.FromDays(180)) //File modification date 5 digits, 1 space, time 5 digits, 1 space
                        ? editDate.ToString("MMM dd  yyyy", CultureInfo.InvariantCulture)
                        : editDate.ToString("MMM dd HH:mm", CultureInfo.InvariantCulture))
                        .Append(' ');
                    sb.Append(f.Name);
                }
                fileList.Add(sb.ToString());
            }
            return fileList;
        }

        ///<summary>
        ///Gets a file
        ///</summary>
        ///< the virtual path >param name="vdir"</param>
        ///<param name="modify" > requires permission to modify</param>
        ///<returns>File information, null if not present</returns>
        public FileInfo GetFile(string vdir, bool modify = false)
        {
            string pre = VPath.NormalizeFilename(vdir);
            VFile file = Get(pre) as VFile;
            if (file == null)
            {
                return null;
            }
            if (modify && !file.Permission.GroupCanWrite)
            {
                return null;
            }
            return file.RealFile;
        }

        ///<summary>
        ///Gets the folder
        ///</summary>
        ///< the virtual path >param name="vdir"</param>
        ///<returns>Folder information, null if not present</returns>
        public DirectoryInfo GetDirectory(string vdir)
        {
            string pre = VPath.NormalizeFilename(vdir);
            VDirectory dir = Get(pre, true) as VDirectory;
            return dir == null ? null : dir.RealDirectory;
        }

        ///<summary>
        ///After performing the file operation, refresh the current directory
        ///</summary>
        public void RefreshCurrentDirectory()
        {
            _currentDirectory.Refresh();
            SetPermission(_currentDirectory, true);
        }

        #endregion

        #region Private Methods
        ///<summary>
        ///Gets a virtual file entity
        ///</summary>
        ///< the virtual path >param name="vdir"</param>
        ///<param name="onlyFindDir" > look for folders only</param>
        ///<returns>A virtual file, null if it does not exist</returns>
        private IFile Get(string vdir, bool onlyFindDir = false)
        {
            IFile currentPosition = _rootDirectory;
            if (!vdir.Trim().StartsWith("/"))
            {
                currentPosition = _currentDirectory;
            }
            if (vdir == "")
            {
                return currentPosition;
            }
            string pre;
            string[] dirs = vdir.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < dirs.Length; i++)
            {
                pre = dirs[i].Trim();
                if (pre == "..")
                {
                    currentPosition = currentPosition.ParentDirectory;
                }
                else
                {
                    if (currentPosition.IsDirectory)
                    {
                        VDirectory temp = currentPosition as VDirectory;
                        if (temp == null || !temp.Enumerate().Contains(pre))
                        {
                            return null;
                        }
                        if (i >= dirs.Length - 1)   //Found the last layer, looking for files
                        {
                            if (onlyFindDir)
                            {
                                currentPosition = temp.SubFiles.Find((t) => (t.Name == pre) && (t.IsDirectory));
                                return currentPosition ?? _rootDirectory;
                            }
                            //Find: The first element found that matches the criteria defined by the specified predicate; otherwise, the default value of type T.
                            currentPosition = temp.SubFiles.Find((t) => (t.Name == pre) && (!t.IsDirectory)) ??
                                              temp.SubFiles.Find((t) => (t.Name == pre) && (t.IsDirectory));
                        }
                        else //Not the last layer, look for subdirectories
                        {
                            currentPosition = temp.SubFiles.Find((t) => (t.Name == pre) && (t.IsDirectory));
                        }
                        if (currentPosition == null)
                        {
                            return null;
                        }
                    }
                }
                SetPermission(currentPosition);
                currentPosition.Refresh();
            }
            SetPermission(currentPosition, true, true);
            return !currentPosition.Permission.CanRead ? null : currentPosition;
        }

        private void SetPermission(IFile file, bool setSubFiles = false, bool setParentFiles = false)
        {
            if (setParentFiles)
            {
                file.ClearPermission();
                SetPermission(file);
            }
            if (_group.Rules.ContainsKey(file.VirtualPath))
            {
                file.Permission = _group.Rules[file.VirtualPath];
            }
            else
            {
                file.Permission = file.ParentDirectory.Permission;
            }
            if (setSubFiles)
            {
                VDirectory f = file as VDirectory;
                if (f == null)
                    return;
                foreach (var s in f.SubFiles)
                {
                    if (_group.Rules.ContainsKey(s.VirtualPath))
                    {
                        s.Permission = _group.Rules[s.VirtualPath];
                    }
                    else
                    {
                        s.Permission = f.Permission;
                    }
                }
            }

        }

        private void AddGroupLinks()
        {
            foreach (var link in _group.Links)
            {
                try
                {
                    AddLink(link.Key, link.Value);
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        #endregion
    }
}

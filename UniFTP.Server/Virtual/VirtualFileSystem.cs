using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;

namespace UniFTP.Server.Virtual
{
    public enum FileError
    {
        None = 0,
        NotFound = 1,
        AlreadyExist = 2,
        CannotRead = 4,
        CannotWrite = 8
    }

    /// <summary>
    /// 虚拟文件系统
    /// </summary>
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
            _rootPath = rootpath ?? group.HomeDir?? config.HomeDir;
            _rootDirectory = new VDirectory(null, new FilePermission("r-xr-xr-x"), _rootPath, "");

            AddGroupLinks();

            _currentDirectory = _rootDirectory;
            _currentDirectory.Refresh();
            SetPermission(_currentDirectory,true);
        }

        //public VirtualFileSystem()
        //{
        //    _rootDirectory = new VDirectory(null, new FilePermission("r-xr-xr-x"), null, "/");
        //    _currentDirectory = _rootDirectory;
        //}

        #region FileSystem Methods

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="vPath">虚拟路径</param>
        /// <returns>创建是否成功</returns>
        public FileError CreateDirectory(string vPath)
        {
            string pre = VPath.NormalizeFilename(vPath);
            var f = Get(VPath.GetParentPath(vPath), true) as VDirectory;
            if (f == null || !f.Permission.CanWrite)
            {
                return FileError.NotFound;
            }
            string name = VPath.GetFileName(vPath);
            if (f.Enumerate().Contains(name)
                || Directory.Exists(Path.Combine(f.RealDirectory.FullName, name)) 
                || File.Exists(Path.Combine(f.RealDirectory.FullName, name)))
            {
                return FileError.AlreadyExist; //已有重名文件
            }
            try
            {
                f.RealDirectory.CreateSubdirectory(name);
                f.Refresh();
            }
            catch (Exception)
            {
                return FileError.NotFound;
            }
            return FileError.None;
        }

        /// <summary>
        /// 虚拟文件是否存在
        /// </summary>
        /// <param name="vPath">虚拟路径</param>
        /// <returns></returns>
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

        /// <summary>
        /// 虚拟目录是否存在
        /// </summary>
        /// <param name="vPath">虚拟路径</param>
        /// <returns></returns>
        public bool ExistsDirectory(string vPath)
        {
            if (string.IsNullOrEmpty(vPath))
            {
                return true;
            }
            string pre = VPath.NormalizeFilename(vPath, true);

            if (pre.StartsWith("/", StringComparison.OrdinalIgnoreCase))    //绝对虚拟路径
            {
                if (Get(pre,true) != null)
                {
                    return true;
                }
            }
            else    //相对虚拟路径
            {
                if (_currentDirectory.SubFiles.Find((t) => t.Name == vPath) != null)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 重命名
        /// </summary>
        /// <param name="vPath"></param>
        /// <param name="targetPath"></param>
        /// <returns></returns>
        public FileError Rename(string vPath, string targetPath)
        {
            var src = Get(vPath);
            var dst = Get(VPath.GetParentPath(targetPath)) as VDirectory;
            string dstName = VPath.GetFileName(targetPath);
            if (src == null || dst == null || !dst.Permission.CanWrite)
            {
                return FileError.NotFound;  //找不到目标文件
            }
            if (src.IsDirectory)
            {
                if (dst.Enumerate().Contains(dstName)
                    || Directory.Exists(Path.Combine(dst.RealDirectory.FullName, dstName)))
                {
                    return FileError.AlreadyExist; //已有重名文件
                }
            }
            else
            {
                if (dst.Enumerate().Contains(dstName)
                    || File.Exists(Path.Combine(dst.RealDirectory.FullName, dstName)))
                {
                    return FileError.AlreadyExist; //已有重名文件
                }
            }
            if (!src.Permission.CanWrite || !dst.Permission.CanWrite)
            {
                return FileError.CannotWrite;  //权限不够
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

        /// <summary>
        /// 切换当前目录
        /// </summary>
        /// <param name="vPath">虚拟路径</param>
        /// <returns></returns>
        public bool ChangeCurrentDirectory(string vPath)
        {
            string pre = VPath.NormalizeFilename(vPath, true);
            if (pre == "..")
            {
                _currentDirectory = _currentDirectory.ParentDirectory;
            }
            if (pre == "/")
            {
                _currentDirectory = _rootDirectory;
            }
            else
            {
                VDirectory v;
                if (pre.StartsWith("/", StringComparison.OrdinalIgnoreCase))    //绝对虚拟路径
                {
                    v = Get(pre, true) as VDirectory;
                }
                else    //相对虚拟路径
                {
                    v = _currentDirectory.SubFiles.Find((t) => t.IsDirectory && t.Name == pre) as VDirectory;
                }
                if (v == null)
                {
                    return false;
                }
                _currentDirectory = v;
            }
            _currentDirectory.Refresh();
            SetPermission(_currentDirectory,true);
            return true;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="vPath">虚拟路径</param>
        /// <param name="delDir">删除文件夹,true为只删除文件夹,false为只删除文件</param>
        /// <returns>删除是否成功</returns>
        public bool Delete(string vPath, bool delDir = false)
        {
            string pre = VPath.NormalizeFilename(vPath);
            IFile f;
            if (delDir)
            {
                f = Get(vPath, true);
            }
            else
            {
                f = Get(vPath);
            }
            if (f == null || !f.Permission.CanWrite)
            {
                return false;
            }
            try
            {
                if (f.IsDirectory && delDir)
                {
                    var vf = (VDirectory) f;
                    if (vf.RealDirectory.GetFileSystemInfos().Length > 0)
                    {
                        return false;
                    }
                    Directory.Delete(vf.RealPath);
                    vf.ParentDirectory.SubFiles.Remove(vf); //FIXED:删除文件更新相应虚拟目录
                    f = null;
                }
                else if(!f.IsDirectory && !delDir)
                {
                    var vf = (VFile) f;
                    File.Delete(f.RealPath);
                    vf.ParentDirectory.SubFiles.Remove(vf);
                    f = null;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 添加一个链接
        /// </summary>
        /// <param name="realPath">链接的真实路径</param>
        /// <param name="vPath">链接所要添加到的虚拟路径</param>
        /// <param name="name">别名，默认为原名</param>
        /// <param name="permission">权限，设为null则继承父目录权限</param>
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

        /// <summary>
        /// 取得可以用于上传的真实路径
        /// </summary>
        /// <param name="vPath">虚拟路径</param>
        /// <param name="rename">重命名，null为不重命名</param>
        /// <returns></returns>
        public string GetRealPathOfFile(string vPath, string rename = null)
        {
            string pre = VPath.NormalizeFilename(vPath);
            VDirectory dir;
            if (!pre.StartsWith("/"))
            {
                dir = _currentDirectory;
                SetPermission(dir, true,true);
            }
            else
            {
                dir = Get(VPath.GetParentPath(vPath), true) as VDirectory;
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
            return Path.Combine(dir.RealPath, VPath.GetFileName(pre));
        }

        /// <summary>
        /// 列出文件名
        /// </summary>
        /// <param name="vPath">虚拟路径</param>
        /// <returns></returns>
        public IEnumerable<string> ListFileNames(string vPath = null)
        {
            VDirectory dir;
            if (string.IsNullOrEmpty(vPath))
            {
                dir = _currentDirectory;
                SetPermission(dir,true);
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

        /// <summary>
        /// 列出文件信息
        /// </summary>
        /// <param name="vPath">虚拟路径</param>
        /// <returns></returns>
        public List<string> ListFiles(string vPath = null)
        {
            VDirectory dir;
            if (string.IsNullOrEmpty(vPath))
            {
                dir = _currentDirectory;
                SetPermission(dir,true);
            }
            else
            {
                dir = Get(VPath.NormalizeFilename(vPath), true) as VDirectory ?? _currentDirectory;
            }

            dir.Refresh();  //FIXED:目录及时更新

            List<string> fileList = new List<string>();

            DateTime now = DateTime.Now;

            //dataWriter.WriteLine(); //MARK:必须加一空行 否则FileZilla会吃掉第一个文件

            foreach (var f in dir.SubFiles)
            {
                if (!f.Permission.CanRead)
                {
                    continue;
                }
                //参考格式
                //drwxrwxrwx   1 user     group           0 Nov 27 00:13 上传
                DateTime editDate;
                StringBuilder sb = new StringBuilder();

                if (f.IsDirectory)
                {
                    VDirectory vf = (VDirectory)f;
                    editDate = vf.RealDirectory.LastWriteTime;

                    sb.Append('d').Append(f.Permission.ToString());                         //文件权限10位
                    sb.Append("   1 ");                                                     //1空格  子文件个数2位 1空格
                    sb.Append(string.Format("{0,-8}", _config.Owner.PadRight(8))).Append(' ');       //文件所有者8位 1空格
                    sb.Append(string.Format("{0,-8}", _config.OwnerGroup.PadRight(8))).Append(' ');       //文件所有者8位 1空格
                    //sb.Append(_config.Owner.Substring(0, 8).PadRight(8)).Append(' ');       //文件所有者8位 1空格
                    //sb.Append(_config.OwnerGroup.Substring(0, 8).PadRight(8)).Append(' ');  //文件所有组8位 1空格
                    sb.Append("       0 ");                                                 //文件大小>=8位，文件夹通常为0或4096  1空格

                    sb.Append(editDate < now.Subtract(TimeSpan.FromDays(180))               //文件修改日期5位，1空格，时间5位，1空格
                        ? editDate.ToString("MMM dd  yyyy", CultureInfo.InvariantCulture)
                        : editDate.ToString("MMM dd HH:mm", CultureInfo.InvariantCulture))
                        .Append(' ');
                    sb.Append(f.Name);
                }
                else
                {
                    VFile vf = (VFile)f;
                    editDate = vf.RealFile.LastWriteTime;

                    //string.Format("{0,-50}", theObj);//格式化成50个字符，原字符左对齐，不足则补空格 
                    //string.Format("{0,50}", theObj);//格式化成50个字符，原字符右对齐，不足则补空格
                    sb.Append('-').Append(f.Permission.ToString());       //文件权限10位
                    sb.Append("   1 ");                 //1空格  子文件个数2位 1空格
                    sb.Append(string.Format("{0,-8}", _config.Owner.PadRight(8))).Append(' ');       //文件所有者8位 1空格
                    sb.Append(string.Format("{0,-8}", _config.OwnerGroup.PadRight(8))).Append(' ');       //文件所有者8位 1空格
                    //sb.Append(_config.OwnerGroup.Substring(0, 8).PadRight(8)).Append(' ');  //文件所有组8位 1空格
                    string length = vf.RealFile.Length.ToString(CultureInfo.InvariantCulture);
                    if (length.Length < 8)
                    {
                        length = length.PadLeft(8);
                    }
                    sb.Append(length).Append(' ');                  //文件大小>=8位  1空格
                    sb.Append(editDate < now.Subtract(TimeSpan.FromDays(180))   //文件修改日期5位，1空格，时间5位，1空格
                        ? editDate.ToString("MMM dd  yyyy", CultureInfo.InvariantCulture)
                        : editDate.ToString("MMM dd HH:mm", CultureInfo.InvariantCulture))
                        .Append(' ');
                    sb.Append(f.Name);
                }
                fileList.Add(sb.ToString());
            }
            return fileList;
        }

        /// <summary>
        /// 获取一个文件
        /// </summary>
        /// <param name="vdir">虚拟路径</param>
        /// <returns>文件信息，如不存在则为null</returns>
        public FileInfo GetFile(string vdir)
        {
            string pre = VPath.NormalizeFilename(vdir);
            VFile file = Get(pre) as VFile;
            return file == null ? null : file.RealFile;
        }

        /// <summary>
        /// 获取文件夹
        /// </summary>
        /// <param name="vdir">虚拟路径</param>
        /// <returns>文件夹信息，如不存在则为null</returns>
        public DirectoryInfo GetDirectory(string vdir)
        {
            string pre = VPath.NormalizeFilename(vdir);
            VDirectory dir = Get(pre, true) as VDirectory;
            return dir == null ? null : dir.RealDirectory;
        }
        #endregion
        #region Private Methods
        /// <summary>
        /// 获取一个虚拟文件实体
        /// </summary>
        /// <param name="vdir">虚拟路径</param>
        /// <param name="onlyFindDir">只寻找文件夹</param>
        /// <returns>虚拟文件，如不存在则为null</returns>
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
                        if (i >= dirs.Length - 1)   //找到了最后一层，寻找文件
                        {
                            if (onlyFindDir)
                            {
                                currentPosition = temp.SubFiles.Find((t) => (t.Name == pre) && (t.IsDirectory));
                                return currentPosition??_rootDirectory;
                            }
                            //Find:如果找到与指定谓词定义的条件匹配的第一个元素，则为该元素；否则为类型 T 的默认值。
                            currentPosition = temp.SubFiles.Find((t) => (t.Name == pre) && (!t.IsDirectory)) ??
                                              temp.SubFiles.Find((t) => (t.Name == pre) && (t.IsDirectory));
                        }
                        else //不是最后一层，找子目录
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
            SetPermission(currentPosition,true,true);
            return !currentPosition.Permission.CanRead ? null : currentPosition;
        }

        private void SetPermission(IFile file , bool setSubFiles = false,bool setParentFiles = false)
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
                if (f == null) return;
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

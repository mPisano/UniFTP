using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net.Layout.Pattern;

namespace UniFTP.Server.Virtual
{
    /// <summary>
    /// 虚拟目录
    /// </summary>
    internal class VDirectory : IFile
    {
        private VDirectory _parent;
        private List<IFile> _realSub = new List<IFile>();
        private List<IFile> _linkedSub = new List<IFile>();
        private FilePermission _permission;
        public List<IFile> SubFiles
        {
            get
            {
                List<IFile> files = new List<IFile>(_realSub.Count + _linkedSub.Count);
                files.AddRange(_realSub);
                files.AddRange(_linkedSub);
                return files;
            }
        }

        public bool AddLink(IFile file)
        {
            if (_linkedSub.Exists(t => t.Name == file.Name))
            {
                return false;
            }
            _linkedSub.Add(file);
            return true;
        }

        /// <summary>
        /// 父文件夹
        /// </summary>
        public VDirectory ParentDirectory
        {
            get { return _parent ?? this; } //_parent为NULL则返回后面
            set { _parent = value; }
        }

        /// <summary>
        /// 真实目录
        /// </summary>
        public DirectoryInfo RealDirectory { get; set; }

        /// <summary>
        /// 真实路径
        /// </summary>
        public string RealPath
        {
            get
            {
                if (RealDirectory != null)
                {
                    return RealDirectory.FullName;
                }
                return "";
            }
        }

        /// <summary>
        /// 虚拟路径
        /// </summary>
        public string VirtualPath { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 虚拟文件夹
        /// </summary>
        /// <param name="permission">权限</param>
        /// <param name="parent">父目录</param>
        /// <param name="name">文件夹名</param>
        /// <param name="realPath">真实路径</param>
        public VDirectory(VDirectory parent, FilePermission permission, string realPath, string name = null)
        {
            ParentDirectory = parent;

            _permission = permission ?? ParentDirectory.Permission;
            try
            {
                Name = name ?? VPath.GetFileName(realPath);
                if (ParentDirectory == null || ParentDirectory == this)
                {
                    VirtualPath = "/";
                }
                else
                {
                    VirtualPath = VPath.Combine(ParentDirectory.VirtualPath, Name);
                }
                RealDirectory = new DirectoryInfo(realPath);
            }
            catch (Exception e)
            {
                //throw e;
                RealDirectory = null;
            }
        }

        public List<string> Enumerate()
        {
            List<string> result = new List<string>();
            foreach (var sub in SubFiles)
            {
                result.Add(sub.Name);
            }
            return result;
        }

        /// <summary>
        /// 刷新子目录与子文件
        /// MARK:改善刷新逻辑，减少新建对象次数
        /// </summary>
        public void Refresh()
        {
            //_realSub.Clear();
            if (!RealDirectory.Exists || !Directory.Exists(RealDirectory.FullName))
            {
                return;
            }
            var infos = RealDirectory.GetFileSystemInfos();
            foreach (var fileSystemInfo in infos)
            {
                var exist = _realSub.FirstOrDefault(t => t.RealPath == fileSystemInfo.FullName);
                if (exist != null)
                {
                    continue;
                }
                if ((fileSystemInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory) //文件夹
                {
                    _realSub.Add(new VDirectory(this, this.Permission, fileSystemInfo.FullName, fileSystemInfo.Name));
                }
                else
                {
                    _realSub.Add(new VFile(this, this.Permission, fileSystemInfo.FullName, fileSystemInfo.Name));
                }
            }
            //ADDED:Linq搜索已被消灭的文件
            _realSub.RemoveAll(s => infos.FirstOrDefault(t => t.FullName == s.RealPath) == null);
        }

        public bool IsDirectory
        {
            get { return true; }
        }

        public void ClearPermission()
        {
            if (ParentDirectory == this)
            {
                return;
            }
            ParentDirectory.ClearPermission();
            this.Permission = ParentDirectory.Permission;
        }

        public FilePermission Permission
        {
            get { return _permission; }
            set
            {
                _permission = value;
                PermissionSetted = true;
            }
        }

        public bool PermissionSetted { get; set; }
    }
}

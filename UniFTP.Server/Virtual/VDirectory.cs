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

        public VDirectory ParentDirectory
        {
            get { return _parent ?? this; } //_parent为NULL则返回后面
            set { _parent = value; }
        }

        public DirectoryInfo RealDirectory { get; set; }

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

        public string VirtualPath { get; set; }

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
                RealDirectory = new DirectoryInfo(realPath);
                if (ParentDirectory == null || ParentDirectory == this)
                {
                    VirtualPath = "/";
                }
                else
                {
                    VirtualPath = VPath.Combine(ParentDirectory.VirtualPath, Name);
                }
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

        public void Refresh()
        {
            _realSub.Clear();
            var infos = RealDirectory.GetFileSystemInfos();
            foreach (var fileSystemInfo in infos)
            {
                if ((fileSystemInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory) //文件夹
                {
                    _realSub.Add(new VDirectory(this, this.Permission, fileSystemInfo.FullName, fileSystemInfo.Name));
                }
                else
                {
                    _realSub.Add(new VFile(this, this.Permission, fileSystemInfo.FullName, fileSystemInfo.Name));
                }
            }
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

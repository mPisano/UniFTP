using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net.Layout.Pattern;

namespace UniFTP.Server.Virtual
{
    ///<summary>
    ///virtical list
    ///</summary>
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

        ///<summary>
        ///parent folder
        ///</summary>
        public VDirectory ParentDirectory
        {
            get { return _parent ?? this; } //If Parent is null, return to the back
            set { _parent = value; }
        }

        ///<summary>
        ///real directory
        ///</summary>
        public DirectoryInfo RealDirectory { get; set; }

        ///<summary>
        ///real path
        ///</summary>
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

        ///<summary>
        ///virtual path
        ///</summary>
        public string VirtualPath { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        ///<summary>
        ///virtual folder
        ///</summary>
        ///<param name="permission">Permission</param>
        ///<param name="parent">parent directory</param>
        ///<param name="name">folder name</param>
        ///<param name="realPath">real path</param>
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
            return SubFiles.Select(sub => sub.Name).ToList();
        }

        ///<summary>
        ///Refresh subdirectories and subfiles
        ///MARK: Improve refresh logic and reduce the number of new objects
        ///</summary>
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
                if ((fileSystemInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory) //folder
                {
                    _realSub.Add(new VDirectory(this, this.Permission, fileSystemInfo.FullName, fileSystemInfo.Name));
                }
                else
                {
                    _realSub.Add(new VFile(this, this.Permission, fileSystemInfo.FullName, fileSystemInfo.Name));
                }
            }
            //Added: linq search for wiped files
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

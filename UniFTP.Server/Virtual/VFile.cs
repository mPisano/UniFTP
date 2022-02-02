using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UniFTP.Server.Virtual
{
    ///<summary>
    ///dummy file
    ///</summary>
    internal class VFile : IFile
    {
        private FilePermission _permission;
        public VDirectory ParentDirectory { get; set; }

        public FileInfo RealFile { get; set; }

        public string RealPath
        {
            get
            {
                if (RealFile != null)
                {
                    return RealFile.FullName;
                }
                return "";
            }
        }
        public VFile(VDirectory parent, FilePermission permission, string realPath, string name)
        {
            ParentDirectory = parent;
            _permission = permission ?? ParentDirectory.Permission;
            try
            {
                Name = name ?? Path.GetFileName(realPath);
                RealFile = new FileInfo(realPath);
            }
            catch (Exception e)
            {
                //throw e;
                RealFile = null;
            }
        }


        public string VirtualPath
        {
            get { return VPath.Combine(ParentDirectory.VirtualPath, this.Name); }
        }

        public string Name { get; set; }

        public bool IsDirectory
        {
            get { return false; }
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


        public void Refresh()
        {

        }

        public bool PermissionSetted { get; set; }


        public void ClearPermission()
        {
            if (ParentDirectory.VirtualPath == "/")
            {
                return;
            }
            ParentDirectory.ClearPermission();
            this.Permission = ParentDirectory.Permission;
        }
    }
}

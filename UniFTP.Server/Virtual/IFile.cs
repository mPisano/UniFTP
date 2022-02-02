using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UniFTP.Server.Virtual
{
    ///<summary>
    ///Virtual file entity
    ///<para>Includes directories and files</para>
    ///</summary>
    internal interface IFile
    {
        ///<summary>
        ///Permissions have been set
        ///</summary>
        bool PermissionSetted { get; set; }
        ///<summary>
        ///Real path
        ///</summary>
        string RealPath { get; }
        ///<summary>
        ///Virtual path
        ///</summary>
        string VirtualPath { get; }
        ///<summary>
        ///name
        ///</summary>
        string Name { get; set; }
        ///<summary>
        ///Whether it is a folder
        ///</summary>
        bool IsDirectory { get; }
        ///<summary>
        ///Clear permissions
        ///</summary>
        void ClearPermission();

        ///<summary>
        ///flushed
        ///</summary>
        void Refresh();
        ///<summary>
        ///The parent directory
        ///</summary>
        VDirectory ParentDirectory { get; set; }
        ///<summary>
        ///Permissions
        ///</summary>
        FilePermission Permission { get; set; }
    }
}

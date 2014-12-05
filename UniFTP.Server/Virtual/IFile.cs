using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UniFTP.Server.Virtual
{
    /// <summary>
    /// 虚拟文件实体
    /// <para>包括目录和文件</para>
    /// </summary>
    internal interface IFile
    {
        /// <summary>
        /// 已经设置权限
        /// </summary>
        bool PermissionSetted { get; set; }
        string RealPath { get; }
        /// <summary>
        /// 虚拟路径
        /// </summary>
        string VirtualPath { get; }
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// 是否为文件夹
        /// </summary>
        bool IsDirectory { get; }
        /// <summary>
        /// 清空权限
        /// </summary>
        void ClearPermission();

        /// <summary>
        /// 刷新
        /// </summary>
        void Refresh();
        /// <summary>
        /// 父目录
        /// </summary>
        VDirectory ParentDirectory { get; set; }
        /// <summary>
        /// 权限
        /// </summary>
        FilePermission Permission { get; set; }
    }
}

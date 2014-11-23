using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniFTP.Server
{
    /// <summary>
    /// FTP配置
    /// </summary>
    [Serializable]
    class FtpConfig
    {
        /// <summary>
        /// 服务器名
        /// <para>注意：若两个服务器示例采用同样的服务器名，它们可能将被视为同一个服务器而共享日志与计数器。</para>
        /// </summary>
        [XmlAttribute("ServerName")]
        public string ServerName { get; set; }

        [XmlAttribute("AllowAnonymous")]
        public bool AllowAnonymous { get; set; }

        [XmlAttribute("homedir")]
        public string HomeDir { get; set; }

        [XmlAttribute("twofactorsecret")]
        public string TwoFactorSecret { get; set; }

        [XmlIgnore]
        public bool IsAnonymous { get; set; }
    }
}

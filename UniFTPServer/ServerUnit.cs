using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace UniFTPServer
{
    [Serializable]
    class ServerUnit
    {
        //MARK:Xml序列化真心辣鸡

        public string UID { get; private set; }

        public string Name { get; set; }

        public string RootDir { get; set; }
        public string CertificatePath { get; set; }
        public UInt16 Port { get; set; }
        public UInt16 V6Port { get; set; }
        public bool AllowAnonymous { get; set; }
        public string[] Welcome { get; set; }
        public string[] LogInWelcome { get; set; }
        public string[] LogOutWelcome { get; set; }
        //[XmlAttribute]
        //public string UserGroupConfig { get; set; }
        //[XmlAttribute]
        //public string UserConfig { get; set; }

        public ServerUnit()
        {
            UID = new Guid().ToString("N");
            Name = "UniFTP新站点";
            RootDir = "";
            Port = 21;
            V6Port = 0;
            AllowAnonymous = true;
            Welcome = null;
            LogInWelcome = null;
            LogOutWelcome = null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using UniFTP.Server;

namespace UniFTPServer
{
    [Serializable]
    class ServerUnit
    {
        //Mark: xml serializes true heart spicy chicken

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
        public CounterType CounterType { get; set; }
        public bool UseTls { get; set; }

        //[XmlAttribute]
        //public string UserGroupConfig { get; set; }
        //[XmlAttribute]
        //public string UserConfig { get; set; }

        public ServerUnit()
        {
            UID = Guid.NewGuid().ToString("N");
            Name = "UniFTP New site";
            RootDir = "";
            Port = 21;
            V6Port = 0;
            AllowAnonymous = true;
            Welcome = null;
            LogInWelcome = null;
            LogOutWelcome = null;
            UseTls = false;
            CounterType = CounterType.System;
        }
    }
}

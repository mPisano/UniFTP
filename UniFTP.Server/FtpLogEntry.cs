using System;
using System.Globalization;

namespace UniFTP.Server
{
    internal enum LogInfo
    {
        ServerStart,
        ServerStop,
        ConnectionEstablished,
        ConnectionTerminated,
    }


    ///<summary>
    ///Connection information
    ///</summary>
    public class FtpConnectionInfo
    {
        public ulong ID { get; set; }
        public string User { get; set; }
        public string UserGroup { get; set; }
        public string IP { get; set; }
        public string CurrentPosition { get; set; }
        public string CurrentFile { get; set; }
        public string LastCommand { get; set; }

    }

    // Fields: date time c-ip c-port cs-username cs-method cs-args sc-status sc-bytes cs-bytes s-name s-port


    public class FtpLogEntry
    {
        ///<summary>
        ///date
        ///</summary>
        public DateTime Date { get; set; }
        ///<summary>
        ///Client IP
        ///</summary>
        public string CIP { get; set; }
        ///<summary>
        ///Client port
        ///</summary>
        public string CPort { get; set; }
        ///<summary>
        ///Login user name
        ///</summary>
        public string CSUsername { get; set; }
        ///<summary>
        ///operate
        ///</summary>
        public string CSMethod { get; set; }
        ///<summary>
        ///parameter
        ///</summary>
        public string CSArgs { get; set; }
        ///<summary>
        ///Transmit the response
        ///</summary>
        public string SCStatus { get; set; }
        public string SCBytes { get; set; }
        ///<summary>
        ///The number of bytes transferred
        ///</summary>
        public string CSBytes { get; set; }
        ///<summary>
        ///Server-side port
        ///</summary>
        public string SPort { get; set; }
        ///<summary>
        ///Additional Information
        ///</summary>
        public string Info { get; set; }

        public override string ToString()
        {
            return string.Join(" ",
                Date.ToString("MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                CIP,
                CPort ?? "-",
                CSUsername,
                CSMethod,
                CSArgs ?? "-",
                SCStatus,
                SCBytes ?? "-",
                CSBytes ?? "-",
                SPort ?? "-",
                Info ?? "-"
                );
        }
    }
}

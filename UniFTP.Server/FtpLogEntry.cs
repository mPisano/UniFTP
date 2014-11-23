using System;
using System.Globalization;

namespace UniFTP.Server
{
    // Fields: date time c-ip c-port cs-username cs-method cs-uri-stem sc-status sc-bytes cs-bytes s-name s-port

    internal class FtpLogEntry
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// 客户端IP
        /// </summary>
        public string CIP { get; set; }
        /// <summary>
        /// 客户端端口
        /// </summary>
        public string CPort { get; set; }
        public string CSUsername { get; set; }
        public string CSMethod { get; set; }
        public string CSUriStem { get; set; }
        public string SCStatus { get; set; }
        public string SCBytes { get; set; }
        public string CSBytes { get; set; }
        public string SName { get; set; }
        public string SPort { get; set; }

        public override string ToString()
        {
            return string.Join(" ",
                Date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                CIP,
                CPort ?? "-",
                CSUsername,
                CSMethod,
                CSUriStem ?? "-",
                SCStatus,
                SCBytes ?? "-",
                CSBytes ?? "-",
                SName ?? "-",
                SPort ?? "-"
                );
        }
    }
}

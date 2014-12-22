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


    /// <summary>
    /// 连接信息
    /// </summary>
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
        /// <summary>
        /// 登录用户名
        /// </summary>
        public string CSUsername { get; set; }
        /// <summary>
        /// 操作
        /// </summary>
        public string CSMethod { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public string CSArgs { get; set; }
        /// <summary>
        /// 传输响应
        /// </summary>
        public string SCStatus { get; set; }
        public string SCBytes { get; set; }
        /// <summary>
        /// 传输字节数
        /// </summary>
        public string CSBytes { get; set; }
        /// <summary>
        /// 服务端端口
        /// </summary>
        public string SPort { get; set; }
        /// <summary>
        /// 其他信息
        /// </summary>
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

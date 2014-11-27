using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpServer;
using UniFTP.Common.Localization;

namespace UniFTP.Server
{
    /// <summary>
    /// FTP应答
    /// </summary>
    static class FtpResponses
    {
        public static readonly Response QUIT = new Response { Code = "221", Text = FtpReplies.QUIT, ShouldQuit = true };
        public static readonly Response UNABLE_TO_OPEN_DATA_CONNECTION = new Response { Code = "425", Text = FtpReplies.UNABLE_TO_OPEN_DATA_CONNECTION, ShouldQuit = true };

        public static readonly Response SYSTEM = new Response { Code = "215", ResourceManager = FtpReplies.ResourceManager, Text = "SYSTEM" };
        public static readonly Response SERVICE_READY = new Response { Code = "220", ResourceManager = FtpReplies.ResourceManager, Text = "SERVICE_READY" };
        public static readonly Response NOT_IMPLEMENTED = new Response { Code = "502", ResourceManager = FtpReplies.ResourceManager, Text = "NOT_IMPLEMENTED" };
        public static readonly Response NOT_IMPLEMENTED_FOR_PARAMETER = new Response { Code = "504", ResourceManager = FtpReplies.ResourceManager, Text = "NOT_IMPLEMENTED_FOR_PARAMETER" };
        public static readonly Response OK = new Response { Code = "200", ResourceManager = FtpReplies.ResourceManager, Text = "OK" };
        public static readonly Response LOGGED_IN = new Response { Code = "230", ResourceManager = FtpReplies.ResourceManager, Text = "LOGGED_IN" };
        public static readonly Response NEED_VERIFICATION = new Response { Code = "332", ResourceManager = FtpReplies.ResourceManager, Text = "NEED_VERIFICATION" };
        public static readonly Response NOT_LOGGED_IN = new Response { Code = "530", ResourceManager = FtpReplies.ResourceManager, Text = "NOT_LOGGED_IN" };
        public static readonly Response USER_OK = new Response { Code = "331", ResourceManager = FtpReplies.ResourceManager, Text = "USER_OK" };
        public static readonly Response RENAME_FROM = new Response { Code = "350", ResourceManager = FtpReplies.ResourceManager, Text = "RENAME_FROM" };
        public static readonly Response FILE_NOT_FOUND = new Response { Code = "550", ResourceManager = FtpReplies.ResourceManager, Text = "FILE_NOT_FOUND" };
        public static readonly Response DIRECTORY_NOT_FOUND = new Response { Code = "550", ResourceManager = FtpReplies.ResourceManager, Text = "DIRECTORY_NOT_FOUND" };
        public static readonly Response DIRECTORY_EXISTS = new Response { Code = "550", ResourceManager = FtpReplies.ResourceManager, Text = "DIRECTORY_EXISTS" };
        public static readonly Response FILE_ACTION_COMPLETE = new Response { Code = "250", ResourceManager = FtpReplies.ResourceManager, Text = "FILE_ACTION_COMPLETE" };
        public static readonly Response FILE_ACTION_NOT_TAKEN = new Response { Code = "450", ResourceManager = FtpReplies.ResourceManager, Text = "FILE_ACTION_NOT_TAKEN" };
        public static readonly Response ENABLING_TLS = new Response { Code = "234", ResourceManager = FtpReplies.ResourceManager, Text = "ENABLING_TLS" };
        public static readonly Response TRANSFER_ABORTED = new Response { Code = "426", ResourceManager = FtpReplies.ResourceManager, Text = "TRANSFER_ABORTED" };
        public static readonly Response TRANSFER_SUCCESSFUL = new Response { Code = "226", ResourceManager = FtpReplies.ResourceManager, Text = "TRANSFER_SUCCESSFUL" };
        public static readonly Response UTF8_ENCODING_ON = new Response { Code = "200", ResourceManager = FtpReplies.ResourceManager, Text = "UTF8_ENCODING_ON" };

        public static readonly Response ENTERING_PASSIVE_MODE = new Response { Code = "227", ResourceManager = FtpReplies.ResourceManager, Text = "ENTERING_PASSIVE_MODE" };
        public static readonly Response ENTERING_EXTENDED_PASSIVE_MODE = new Response { Code = "229", ResourceManager = FtpReplies.ResourceManager, Text = "ENTERING_EXTENDED_PASSIVE_MODE" };
        public static readonly Response PARAMETER_NOT_RECOGNIZED = new Response { Code = "501", ResourceManager = FtpReplies.ResourceManager, Text = "PARAMETER_NOT_RECOGNIZED" };
        public static readonly Response OPENING_DATA_TRANSFER = new Response { Code = "150", ResourceManager = FtpReplies.ResourceManager, Text = "OPENING_DATA_TRANSFER" };
        public static readonly Response CURRENT_DIRECTORY = new Response { Code = "257", ResourceManager = FtpReplies.ResourceManager, Text = "CURRENT_DIRECTORY" };

        public static readonly Response FEATURES = new Response { Code = "211-", Text = string.Format("{0}:\r\n MDTM\r\n SIZE\r\n UTF8\r\n211 End", FtpReplies.EXTENSIONS_SUPPORTED) };
    
        //ADDED
        /// <summary>
        /// 无法打开数据连接
        /// </summary>
        public static readonly Response CANNOT_OPEN_DATACONNECTION = new Response { Code = "500", ResourceManager = FtpReplies.ResourceManager, Text = "SYNTAX_ERROR" };
    }
}

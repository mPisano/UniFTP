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
        /// <summary>
        /// 退出
        /// </summary>
        public static readonly Response QUIT = new Response { Code = "221", Text = FtpReplies.QUIT, ShouldQuit = true };
        /// <summary>
        /// 无法开启文件传输
        /// </summary>
        public static readonly Response UNABLE_TO_OPEN_DATA_CONNECTION = new Response { Code = "425", Text = FtpReplies.UNABLE_TO_OPEN_DATA_CONNECTION, ShouldQuit = true };
        /// <summary>
        /// 系统类型（统一为UNIX）
        /// </summary>
        public static readonly Response SYSTEM = new Response { Code = "215", ResourceManager = FtpReplies.ResourceManager, Text = "SYSTEM" };
        /// <summary>
        /// 服务器就绪
        /// </summary>
        public static readonly Response SERVICE_READY = new Response { Code = "220", ResourceManager = FtpReplies.ResourceManager, Text = "SERVICE_READY" };
        /// <summary>
        /// 功能尚未实现
        /// </summary>
        public static readonly Response NOT_IMPLEMENTED = new Response { Code = "502", ResourceManager = FtpReplies.ResourceManager, Text = "NOT_IMPLEMENTED" };
        /// <summary>
        /// 该参数尚未实现
        /// </summary>
        public static readonly Response NOT_IMPLEMENTED_FOR_PARAMETER = new Response { Code = "504", ResourceManager = FtpReplies.ResourceManager, Text = "NOT_IMPLEMENTED_FOR_PARAMETER" };
        /// <summary>
        /// 确认
        /// </summary>
        public static readonly Response OK = new Response { Code = "200", ResourceManager = FtpReplies.ResourceManager, Text = "OK" };
        /// <summary>
        /// 用户已登录
        /// </summary>
        public static readonly Response LOGGED_IN = new Response { Code = "230", ResourceManager = FtpReplies.ResourceManager, Text = "LOGGED_IN" };
        /// <summary>
        /// 需要验证
        /// </summary>
        public static readonly Response NEED_VERIFICATION = new Response { Code = "332", ResourceManager = FtpReplies.ResourceManager, Text = "NEED_VERIFICATION" };
        /// <summary>
        /// 用户未登录
        /// </summary>
        public static readonly Response NOT_LOGGED_IN = new Response { Code = "530", ResourceManager = FtpReplies.ResourceManager, Text = "NOT_LOGGED_IN" };
        /// <summary>
        /// 用户名已接受，需要密码
        /// </summary>
        public static readonly Response USER_OK = new Response { Code = "331", ResourceManager = FtpReplies.ResourceManager, Text = "USER_OK" };
        /// <summary>
        /// 等待重命名后续命令
        /// </summary>
        public static readonly Response RENAME_FROM = new Response { Code = "350", ResourceManager = FtpReplies.ResourceManager, Text = "RENAME_FROM" };
        /// <summary>
        /// 等待重新开始后续命令
        /// </summary>
        public static readonly Response RESTART_FROM = new Response { Code = "350", ResourceManager = FtpReplies.ResourceManager, Text = "RESTART_FROM" };
        /// <summary>
        /// 未找到文件
        /// </summary>
        public static readonly Response FILE_NOT_FOUND = new Response { Code = "550", ResourceManager = FtpReplies.ResourceManager, Text = "FILE_NOT_FOUND" };
        /// <summary>
        /// 未找到文件夹
        /// </summary>
        public static readonly Response DIRECTORY_NOT_FOUND = new Response { Code = "550", ResourceManager = FtpReplies.ResourceManager, Text = "DIRECTORY_NOT_FOUND" };
        /// <summary>
        /// 文件夹已经存在
        /// </summary>
        public static readonly Response DIRECTORY_EXISTS = new Response { Code = "550", ResourceManager = FtpReplies.ResourceManager, Text = "DIRECTORY_EXISTS" };
        /// <summary>
        /// 文件操作成功
        /// </summary>
        public static readonly Response FILE_ACTION_COMPLETE = new Response { Code = "250", ResourceManager = FtpReplies.ResourceManager, Text = "FILE_ACTION_COMPLETE" };
        /// <summary>
        /// 文件操作未执行
        /// </summary>
        public static readonly Response FILE_ACTION_NOT_TAKEN = new Response { Code = "450", ResourceManager = FtpReplies.ResourceManager, Text = "FILE_ACTION_NOT_TAKEN" };
        /// <summary>
        /// 启用TLS
        /// </summary>
        public static readonly Response ENABLING_TLS = new Response { Code = "234", ResourceManager = FtpReplies.ResourceManager, Text = "ENABLING_TLS" };
        /// <summary>
        /// 传输中断
        /// </summary>
        public static readonly Response TRANSFER_ABORTED = new Response { Code = "426", ResourceManager = FtpReplies.ResourceManager, Text = "TRANSFER_ABORTED" };
        /// <summary>
        /// 传输成功
        /// </summary>
        public static readonly Response TRANSFER_SUCCESSFUL = new Response { Code = "226", ResourceManager = FtpReplies.ResourceManager, Text = "TRANSFER_SUCCESSFUL" };
        /// <summary>
        /// 启用UTF8
        /// </summary>
        public static readonly Response UTF8_ENCODING_ON = new Response { Code = "200", ResourceManager = FtpReplies.ResourceManager, Text = "UTF8_ENCODING_ON" };
        /// <summary>
        /// 进入被动模式
        /// </summary>
        public static readonly Response ENTERING_PASSIVE_MODE = new Response { Code = "227", ResourceManager = FtpReplies.ResourceManager, Text = "ENTERING_PASSIVE_MODE" };
        /// <summary>
        /// 进入扩展被动模式
        /// </summary>
        public static readonly Response ENTERING_EXTENDED_PASSIVE_MODE = new Response { Code = "229", ResourceManager = FtpReplies.ResourceManager, Text = "ENTERING_EXTENDED_PASSIVE_MODE" };
        /// <summary>
        /// 参数未识别
        /// </summary>
        public static readonly Response PARAMETER_NOT_RECOGNIZED = new Response { Code = "501", ResourceManager = FtpReplies.ResourceManager, Text = "PARAMETER_NOT_RECOGNIZED" };
        /// <summary>
        /// 开启数据传输
        /// </summary>
        public static readonly Response OPENING_DATA_TRANSFER = new Response { Code = "150", ResourceManager = FtpReplies.ResourceManager, Text = "OPENING_DATA_TRANSFER" };
        /// <summary>
        /// 当前目录
        /// </summary>
        public static readonly Response CURRENT_DIRECTORY = new Response { Code = "257", ResourceManager = FtpReplies.ResourceManager, Text = "CURRENT_DIRECTORY" };
        
        private static readonly string ExFeatures = new StringBuilder("{0}:").AppendLine()
            .Append(" MDTM").AppendLine()
            .Append(" SIZE").AppendLine()
            .Append(" UTF8").AppendLine()
            .Append(" REST").AppendLine()
            .Append("211 End").ToString();

        /// <summary>
        /// 支持的扩展命令
        /// </summary>
        public static readonly Response FEATURES = new Response { Code = "211-", Text = string.Format(ExFeatures, FtpReplies.EXTENSIONS_SUPPORTED)};
        //string.Format("{0}:\r\n MDTM\r\n SIZE\r\n UTF8\r\n211 End"


        //ADDED
        /// <summary>
        /// 语法错误
        /// </summary>
        public static readonly Response SYNTAX_ERROR = new Response { Code = "500", ResourceManager = FtpReplies.ResourceManager, Text = "SYNTAX_ERROR" };
    }
}

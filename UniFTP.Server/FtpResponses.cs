using System;
using System.Collections.Generic;
using System.Globalization;
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
        /// 221 退出
        /// </summary>
        public static readonly Response QUIT = new Response { Code = "221", Text = FtpReplies.QUIT, ShouldQuit = true };
        /// <summary>
        /// 425 无法开启文件传输
        /// </summary>
        public static readonly Response UNABLE_TO_OPEN_DATA_CONNECTION = new Response { Code = "425", Text = FtpReplies.UNABLE_TO_OPEN_DATA_CONNECTION, ShouldQuit = true };
        /// <summary>
        /// 215 系统类型（模拟为UNIX）
        /// </summary>
        public static readonly Response SYSTEM = new Response { Code = "215", ResourceManager = FtpReplies.ResourceManager, Text = "SYSTEM" };
        /// <summary>
        /// 220 服务器就绪
        /// </summary>
        public static readonly Response SERVICE_READY = new Response { Code = "220", ResourceManager = FtpReplies.ResourceManager, Text = "SERVICE_READY" };
        /// <summary>
        /// 502 功能尚未实现
        /// </summary>
        public static readonly Response NOT_IMPLEMENTED = new Response { Code = "502", ResourceManager = FtpReplies.ResourceManager, Text = "NOT_IMPLEMENTED" };
        /// <summary>
        /// 504 该参数尚未实现
        /// </summary>
        public static readonly Response NOT_IMPLEMENTED_FOR_PARAMETER = new Response { Code = "504", ResourceManager = FtpReplies.ResourceManager, Text = "NOT_IMPLEMENTED_FOR_PARAMETER" };
        /// <summary>
        /// 200 确认
        /// </summary>
        public static readonly Response OK = new Response { Code = "200", ResourceManager = FtpReplies.ResourceManager, Text = "OK" };
        /// <summary>
        /// 230 用户已登录
        /// </summary>
        public static readonly Response LOGGED_IN = new Response { Code = "230", ResourceManager = FtpReplies.ResourceManager, Text = "LOGGED_IN" };
        /// <summary>
        /// 332 需要验证
        /// </summary>
        public static readonly Response NEED_VERIFICATION = new Response { Code = "332", ResourceManager = FtpReplies.ResourceManager, Text = "NEED_VERIFICATION" };
        /// <summary>
        /// 530 用户未登录
        /// </summary>
        public static readonly Response NOT_LOGGED_IN = new Response { Code = "530", ResourceManager = FtpReplies.ResourceManager, Text = "NOT_LOGGED_IN" };
        /// <summary>
        /// 331 用户名已接受，需要密码
        /// </summary>
        public static readonly Response USER_OK = new Response { Code = "331", ResourceManager = FtpReplies.ResourceManager, Text = "USER_OK" };
        /// <summary>
        /// 350 等待重命名后续命令
        /// </summary>
        public static readonly Response RENAME_FROM = new Response { Code = "350", ResourceManager = FtpReplies.ResourceManager, Text = "RENAME_FROM" };
        /// <summary>
        /// 350 等待重新开始后续命令
        /// </summary>
        public static readonly Response RESTART_FROM = new Response { Code = "350", ResourceManager = FtpReplies.ResourceManager, Text = "RESTART_FROM" };
        /// <summary>
        /// 550 未找到文件
        /// </summary>
        public static readonly Response FILE_NOT_FOUND = new Response { Code = "550", ResourceManager = FtpReplies.ResourceManager, Text = "FILE_NOT_FOUND" };
        /// <summary>
        /// 550 未找到文件夹
        /// </summary>
        public static readonly Response DIRECTORY_NOT_FOUND = new Response { Code = "550", ResourceManager = FtpReplies.ResourceManager, Text = "DIRECTORY_NOT_FOUND" };
        /// <summary>
        /// 550 文件夹已经存在
        /// </summary>
        public static readonly Response DIRECTORY_EXISTS = new Response { Code = "550", ResourceManager = FtpReplies.ResourceManager, Text = "DIRECTORY_EXISTS" };
        /// <summary>
        /// 250 文件操作成功
        /// </summary>
        public static readonly Response FILE_ACTION_COMPLETE = new Response { Code = "250", ResourceManager = FtpReplies.ResourceManager, Text = "FILE_ACTION_COMPLETE" };
        /// <summary>
        /// 257 新建目录成功
        /// </summary>
        public static readonly Response MAKE_DIRECTORY_SUCCESS = new Response { Code = "257", ResourceManager = FtpReplies.ResourceManager, Text = "MAKE_DIRECTORY_SUCCESS" };
        /// <summary>
        /// 450 文件操作未执行
        /// </summary>
        public static readonly Response FILE_ACTION_NOT_TAKEN = new Response { Code = "450", ResourceManager = FtpReplies.ResourceManager, Text = "FILE_ACTION_NOT_TAKEN" };
        
        //ADDED:TLS相关

        /// <summary>
        /// 234 启用TLS
        /// </summary>
        public static readonly Response ENABLING_TLS = new Response { Code = "234", ResourceManager = FtpReplies.ResourceManager, Text = "ENABLING_TLS" };
        /// <summary>
        /// 534 TLS已禁用
        /// </summary>
        public static readonly Response TLS_DISABLED = new Response { Code = "534", ResourceManager = FtpReplies.ResourceManager, Text = "TLS_DISABLED" };
        /// <summary>
        /// 535 TLS认证失败
        /// <para>参见RFC 2228 Line:535</para>
        /// </summary>
        public static readonly Response TLS_AUTH_FAILED = new Response { Code = "535", ResourceManager = FtpReplies.ResourceManager, Text = "TLS_AUTH_FAILED" };
        /// <summary>
        /// 150 开启加密数据传输
        /// </summary>
        //public static readonly Response OPENING_SAFE_DATA_TRANSFER = new Response { Code = "150", ResourceManager = FtpReplies.ResourceManager, Text = "OPENING_SAFE_DATA_TRANSFER" };

        /// <summary>
        /// 426 传输中断
        /// </summary>
        public static readonly Response TRANSFER_ABORTED = new Response { Code = "426", ResourceManager = FtpReplies.ResourceManager, Text = "TRANSFER_ABORTED" };
        /// <summary>
        /// 226 传输成功
        /// </summary>
        public static readonly Response TRANSFER_SUCCESSFUL = new Response { Code = "226", ResourceManager = FtpReplies.ResourceManager, Text = "TRANSFER_SUCCESSFUL" };
        /// <summary>
        /// 200 启用UTF8
        /// </summary>
        public static readonly Response UTF8_ENCODING_ON = new Response { Code = "200", ResourceManager = FtpReplies.ResourceManager, Text = "UTF8_ENCODING_ON" };
        /// <summary>
        /// 227 进入被动模式
        /// </summary>
        public static readonly Response ENTERING_PASSIVE_MODE = new Response { Code = "227", ResourceManager = FtpReplies.ResourceManager, Text = "ENTERING_PASSIVE_MODE" };
        /// <summary>
        /// 229 进入扩展被动模式
        /// </summary>
        public static readonly Response ENTERING_EXTENDED_PASSIVE_MODE = new Response { Code = "229", ResourceManager = FtpReplies.ResourceManager, Text = "ENTERING_EXTENDED_PASSIVE_MODE" };
        /// <summary>
        /// 501 参数未识别
        /// </summary>
        public static readonly Response PARAMETER_NOT_RECOGNIZED = new Response { Code = "501", ResourceManager = FtpReplies.ResourceManager, Text = "PARAMETER_NOT_RECOGNIZED" };
        /// <summary>
        /// 150 开启数据传输
        /// </summary>
        public static readonly Response OPENING_DATA_TRANSFER = new Response { Code = "150", ResourceManager = FtpReplies.ResourceManager, Text = "OPENING_DATA_TRANSFER" };
        /// <summary>
        /// 257 当前目录
        /// </summary>
        public static readonly Response CURRENT_DIRECTORY = new Response { Code = "257", ResourceManager = FtpReplies.ResourceManager, Text = "CURRENT_DIRECTORY" };
        
        /// <summary>
        /// 扩展指令
        /// </summary>
        private static readonly string ExFeatures = new StringBuilder("{0}:").AppendLine()
            .Append(" MDTM").AppendLine()
            .Append(" SIZE").AppendLine()
            .Append(" UTF8").AppendLine()
            .Append(" EPRT").AppendLine()
            .Append(" EPSV").AppendLine()
            .Append(" REST STREAM").AppendLine() //FIXED:按照RFC文档要求修改
            .Append(" AUTH TLS").AppendLine()   //TLS认证
            .Append(" PBSZ").AppendLine()   //保护缓冲区设置
            .Append(" PROT").AppendLine()   //保护级别
            .Append(" MLSD").AppendLine()
            .Append(" MLST").AppendLine()
            .Append("211 End").ToString().ToString(CultureInfo.InvariantCulture);

        //FIXED:未指定ResourcesManager，错误显示中文

        /// <summary>
        /// 211 支持的扩展命令
        /// </summary>
        public static readonly Response FEATURES = new Response { Code = "211-", Text = string.Format(ExFeatures, FtpReplies.ResourceManager.GetString("EXTENSIONS_SUPPORTED",CultureInfo.InvariantCulture))};
        //string.Format("{0}:\r\n MDTM\r\n SIZE\r\n UTF8\r\n211 End"


        //ADDED:新添加的FTP响应
        /// <summary>
        /// 500 语法错误
        /// </summary>
        public static readonly Response SYNTAX_ERROR = new Response { Code = "500", ResourceManager = FtpReplies.ResourceManager, Text = "SYNTAX_ERROR" };
        /// <summary>
        /// 503 命令顺序错误
        /// </summary>
        public static readonly Response BAD_SEQUENCE = new Response { Code = "503", ResourceManager = FtpReplies.ResourceManager, Text = "BAD_SEQUENCE" };
       
    }
}

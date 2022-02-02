using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SharpServer;
using UniFTP.Common.Localization;

namespace UniFTP.Server
{
    ///<summary>
    ///FTP answer
    ///</summary>
    static class FtpResponses
    {
        ///<summary>
        ///221 Exit
        ///</summary>
        public static readonly Response QUIT = new Response { Code = "221", Text = FtpReplies.QUIT, ShouldQuit = true };
        ///<summary>
        ///425 File transfer could not be started
        ///</summary>
        public static readonly Response UNABLE_TO_OPEN_DATA_CONNECTION = new Response { Code = "425", Text = FtpReplies.UNABLE_TO_OPEN_DATA_CONNECTION, ShouldQuit = true };
        ///<summary>
        ///215 System Type (Analog UNIX)
        ///</summary>
        public static readonly Response SYSTEM = new Response { Code = "215", ResourceManager = FtpReplies.ResourceManager, Text = "SYSTEM" };
        ///<summary>
        ///220 Server ready
        ///</summary>
        public static readonly Response SERVICE_READY = new Response { Code = "220", ResourceManager = FtpReplies.ResourceManager, Text = "SERVICE_READY" };
        ///<summary>
        ///The 502 feature has not been implemented
        ///</summary>
        public static readonly Response NOT_IMPLEMENTED = new Response { Code = "502", ResourceManager = FtpReplies.ResourceManager, Text = "NOT_IMPLEMENTED" };
        ///<summary>
        ///504 This parameter has not been implemented
        ///</summary>
        public static readonly Response NOT_IMPLEMENTED_FOR_PARAMETER = new Response { Code = "504", ResourceManager = FtpReplies.ResourceManager, Text = "NOT_IMPLEMENTED_FOR_PARAMETER" };
        ///<summary>
        ///200 Confirmation
        ///</summary>
        public static readonly Response OK = new Response { Code = "200", ResourceManager = FtpReplies.ResourceManager, Text = "OK" };
        ///<summary>
        ///230 Users are logged on
        ///</summary>
        public static readonly Response LOGGED_IN = new Response { Code = "230", ResourceManager = FtpReplies.ResourceManager, Text = "LOGGED_IN" };
        ///<summary>
        ///332 Authentication required
        ///</summary>
        public static readonly Response NEED_VERIFICATION = new Response { Code = "332", ResourceManager = FtpReplies.ResourceManager, Text = "NEED_VERIFICATION" };
        ///<summary>
        ///530 The user is not logged on
        ///</summary>
        public static readonly Response NOT_LOGGED_IN = new Response { Code = "530", ResourceManager = FtpReplies.ResourceManager, Text = "NOT_LOGGED_IN" };
        ///<summary>
        ///331 Username accepted, password required
        ///</summary>
        public static readonly Response USER_OK = new Response { Code = "331", ResourceManager = FtpReplies.ResourceManager, Text = "USER_OK" };
        ///<summary>
        ///350 Wait for subsequent commands to rename
        ///</summary>
        public static readonly Response RENAME_FROM = new Response { Code = "350", ResourceManager = FtpReplies.ResourceManager, Text = "RENAME_FROM" };
        ///<summary>
        ///350 Wait to restart subsequent commands
        ///</summary>
        public static readonly Response RESTART_FROM = new Response { Code = "350", ResourceManager = FtpReplies.ResourceManager, Text = "RESTART_FROM" };
        ///<summary>
        ///550 File not found
        ///</summary>
        public static readonly Response FILE_NOT_FOUND = new Response { Code = "550", ResourceManager = FtpReplies.ResourceManager, Text = "FILE_NOT_FOUND" };
        ///<summary>
        ///550 Folder not found
        ///</summary>
        public static readonly Response DIRECTORY_NOT_FOUND = new Response { Code = "550", ResourceManager = FtpReplies.ResourceManager, Text = "DIRECTORY_NOT_FOUND" };
        ///<summary>
        ///550 Folder already exists
        ///</summary>
        public static readonly Response DIRECTORY_EXISTS = new Response { Code = "550", ResourceManager = FtpReplies.ResourceManager, Text = "DIRECTORY_EXISTS" };
        ///<summary>
        ///250 The file operation succeeded
        ///</summary>
        public static readonly Response FILE_ACTION_COMPLETE = new Response { Code = "250", ResourceManager = FtpReplies.ResourceManager, Text = "FILE_ACTION_COMPLETE" };
        ///<summary>
        ///257 New directory successful
        ///</summary>
        public static readonly Response MAKE_DIRECTORY_SUCCESS = new Response { Code = "257", ResourceManager = FtpReplies.ResourceManager, Text = "MAKE_DIRECTORY_SUCCESS" };
        ///<summary>
        ///450 The file operation was not performed
        ///</summary>
        public static readonly Response FILE_ACTION_NOT_TAKEN = new Response { Code = "450", ResourceManager = FtpReplies.ResourceManager, Text = "FILE_ACTION_NOT_TAKEN" };

        //Added:tls-related

        ///<summary>
        ///234 TLS enabled
        ///</summary>
        public static readonly Response ENABLING_TLS = new Response { Code = "234", ResourceManager = FtpReplies.ResourceManager, Text = "ENABLING_TLS" };
        ///<summary>
        ///534 TLS is disabled
        ///</summary>
        public static readonly Response TLS_DISABLED = new Response { Code = "534", ResourceManager = FtpReplies.ResourceManager, Text = "TLS_DISABLED" };
        ///<summary>
        ///535 TLS authentication failed
        ///<para>See RFC 2228 Line:535</para>
        ///</summary>
        public static readonly Response TLS_AUTH_FAILED = new Response { Code = "535", ResourceManager = FtpReplies.ResourceManager, Text = "TLS_AUTH_FAILED" };
        ///<summary>
        ///150 Enable encrypted data transfer
        ///</summary>
        //public static readonly Response OPENING_SAFE_DATA_TRANSFER = new Response { Code = "150", ResourceManager = FtpReplies.ResourceManager, Text = "OPENING_SAFE_DATA_TRANSFER" };

        ///<summary>
        ///426 Transmission interrupted
        ///</summary>
        public static readonly Response TRANSFER_ABORTED = new Response { Code = "426", ResourceManager = FtpReplies.ResourceManager, Text = "TRANSFER_ABORTED" };
        ///<summary>
        ///226 The transfer was successful
        ///</summary>
        public static readonly Response TRANSFER_SUCCESSFUL = new Response { Code = "226", ResourceManager = FtpReplies.ResourceManager, Text = "TRANSFER_SUCCESSFUL" };
        ///<summary>
        ///200 Enable UTF8
        ///</summary>
        public static readonly Response UTF8_ENCODING_ON = new Response { Code = "200", ResourceManager = FtpReplies.ResourceManager, Text = "UTF8_ENCODING_ON" };
        ///<summary>
        ///227 Enter passive mode
        ///</summary>
        public static readonly Response ENTERING_PASSIVE_MODE = new Response { Code = "227", ResourceManager = FtpReplies.ResourceManager, Text = "ENTERING_PASSIVE_MODE" };
        ///<summary>
        ///229 Enters extended passive mode
        ///</summary>
        public static readonly Response ENTERING_EXTENDED_PASSIVE_MODE = new Response { Code = "229", ResourceManager = FtpReplies.ResourceManager, Text = "ENTERING_EXTENDED_PASSIVE_MODE" };
        ///<summary>
        ///The 501 parameter is not recognized
        ///</summary>
        public static readonly Response PARAMETER_NOT_RECOGNIZED = new Response { Code = "501", ResourceManager = FtpReplies.ResourceManager, Text = "PARAMETER_NOT_RECOGNIZED" };
        ///<summary>
        ///150 Enable data transfer
        ///</summary>
        public static readonly Response OPENING_DATA_TRANSFER = new Response { Code = "150", ResourceManager = FtpReplies.ResourceManager, Text = "OPENING_DATA_TRANSFER" };
        ///<summary>
        ///257 The current directory
        ///</summary>
        public static readonly Response CURRENT_DIRECTORY = new Response { Code = "257", ResourceManager = FtpReplies.ResourceManager, Text = "CURRENT_DIRECTORY" };

        ///<summary>
        ///Extension directives
        ///</summary>
        private static readonly string ExFeatures = new StringBuilder("{0}:").AppendLine()
            .Append(" MDTM").AppendLine()
            .Append(" SIZE").AppendLine()
            .Append(" UTF8").AppendLine()
            .Append(" EPRT").AppendLine()
            .Append(" EPSV").AppendLine()
            .Append(" REST STREAM").AppendLine() //Fixed: Modify as required by the rfc document
            .Append(" AUTH TLS").AppendLine()   //Tls certification
            .Append(" PBSZ").AppendLine()   //Protection buffer settings
            .Append(" PROT").AppendLine()   //Protection level
            .Append(" MLSD").AppendLine()
            .Append(" MLST").AppendLine()
            .Append("211 End").ToString().ToString(CultureInfo.InvariantCulture);

        //Fixed: The source manager is not specified, and the error shows Chinese

        ///<summary>
        ///211 Supported extended commands
        ///</summary>
        public static readonly Response FEATURES = new Response { Code = "211-", Text = string.Format(ExFeatures, FtpReplies.ResourceManager.GetString("EXTENSIONS_SUPPORTED", CultureInfo.InvariantCulture)) };
        //string.Format("{0}:\r\n MDTM\r\n SIZE\r\n UTF8\r\n211 End"


        //ADDED: Newly added FTP response
        ///<summary>
        ///500 Syntax error
        ///</summary>
        public static readonly Response SYNTAX_ERROR = new Response { Code = "500", ResourceManager = FtpReplies.ResourceManager, Text = "SYNTAX_ERROR" };
        ///<summary>
        ///503 Command order error
        ///</summary>
        public static readonly Response BAD_SEQUENCE = new Response { Code = "503", ResourceManager = FtpReplies.ResourceManager, Text = "BAD_SEQUENCE" };

    }
}

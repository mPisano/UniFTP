using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Resources;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using SharpServer;
using UniFTP.Common.Localization;

namespace UniFTP.Server
{
    public class FtpClientConnection : ClientConnection
    {
        private class DataConnectionOperation
        {
            public Func<NetworkStream, string, Response> Operation { get; set; }
            public string Arguments { get; set; }
        }

        #region 枚举

        public enum TransferType
        {
            Ascii,
            Ebcdic,
            Image,
            Local,
        }

        public enum FormatControlType
        {
            NonPrint,
            Telnet,
            CarriageControl,
        }

        public enum DataConnectionType
        {
            Passive,
            Active,
        }
        /// <summary>
        /// 文件结构（STRU）
        /// </summary>
        public enum FileStructureType
        {
            File,
            Record,
            Page,
        }

        #endregion

        private const int BUFFER_SIZE = 8096;

        private TcpListener _passiveListener;
        private TcpClient _dataClient;
        private TransferType _connectionType = TransferType.Ascii;
        private FormatControlType _formatControlType = FormatControlType.NonPrint;
        private DataConnectionType _dataConnectionType = DataConnectionType.Active;
        private FileStructureType _fileStructureType = FileStructureType.File;

        private string _username;
        private string _password;
        private string _root;
        private string _currentDirectory;
        private IPEndPoint _dataEndpoint;
        private X509Certificate _cert = null;
        private SslStream _sslStream;

        private bool _disposed = false;

        private bool _connected = false;

        private FtpUser _currentUser;
        private List<string> _validCommands;

        private static readonly Regex _invalidPathChars = new Regex(string.Join("|", Path.GetInvalidPathChars().Select(c => string.Format(CultureInfo.InvariantCulture, "\\u{0:X4}", (int)c))), RegexOptions.Compiled);

        private string _renameFrom;

        private Encoding _currentEncoding = Encoding.ASCII;
        private CultureInfo _currentCulture = CultureInfo.CurrentCulture;

        public FtpClientConnection()
            : base()
        {
            _validCommands = new List<string>();
            _renameFrom = null;
        }

        #region 重载

        protected override Response HandleCommand(Command cmd)
        {
            Response response = null;

            FtpLogEntry logEntry = new FtpLogEntry
            {
                Date = DateTime.Now,
                CIP = ClientIP,
                CSUriStem = cmd.RawArguments
            };
            //请求的命令需要权限
            if (!_validCommands.Contains(cmd.Code))
            {
                response = CheckUser();
            }

            // Reset rename from if we don't receive a rename to command. These must be issued back-to-back.
            if (cmd.Code != "RNTO")
            {
                _renameFrom = null;
            }

            if (response == null)
            {
                switch (cmd.Code)
                {
                    case "USER":
                        response = User(cmd.Arguments.FirstOrDefault());
                        break;
                    case "PASS":
                        response = Password(cmd.Arguments.FirstOrDefault());
                        logEntry.CSUriStem = "******";
                        break;
                    case "CWD":
                        response = ChangeWorkingDirectory(cmd.Arguments.FirstOrDefault());
                        break;
                    case "CDUP":
                        response = ChangeWorkingDirectory("..");
                        break;
                    case "QUIT":
                        response = GetResponse(FtpResponses.QUIT);
                        break;
                    case "REIN":
                        _currentUser = null;
                        _username = null;
                        _dataClient = null;
                        _currentCulture = CultureInfo.CurrentCulture;
                        _currentEncoding = Encoding.ASCII;
                        ControlStreamEncoding = Encoding.ASCII;

                        response = GetResponse(FtpResponses.SERVICE_READY);
                        break;
                    case "PORT":
                        response = Port(cmd.RawArguments);
                        logEntry.CPort = _dataEndpoint.Port.ToString(CultureInfo.InvariantCulture);
                        break;
                    case "PASV":    //进入被动模式
                        response = Passive();
                        logEntry.SPort = ((IPEndPoint)_passiveListener.LocalEndpoint).Port.ToString(CultureInfo.InvariantCulture);
                        break;
                    case "TYPE":
                        response = Type(cmd.Arguments.FirstOrDefault(), cmd.Arguments.Skip(1).FirstOrDefault());
                        break;
                    case "STRU":
                        response = Structure(cmd.Arguments.FirstOrDefault());
                        break;
                    case "MODE":
                        response = Mode(cmd.Arguments.FirstOrDefault());
                        break;
                    case "RNFR":
                        _renameFrom = cmd.Arguments.FirstOrDefault();
                        response = GetResponse(FtpResponses.RENAME_FROM);
                        break;
                    case "RNTO":
                        response = Rename(_renameFrom, cmd.Arguments.FirstOrDefault());
                        break;
                    case "DELE":
                        response = Delete(cmd.Arguments.FirstOrDefault());
                        break;
                    case "RMD":
                        response = RemoveDir(cmd.Arguments.FirstOrDefault());
                        break;
                    case "MKD":
                        response = CreateDir(cmd.Arguments.FirstOrDefault());
                        break;
                    case "PWD":
                        response = PrintWorkingDirectory();
                        break;
                    case "RETR":    //下载文件
                        //FIXED:文件名含空格
                        response = Retrieve(cmd.RawArguments);
                        logEntry.Date = DateTime.Now;
                        break;
                    case "STOR":    //上传文件
                        response = Store(cmd.Arguments.FirstOrDefault());
                        logEntry.Date = DateTime.Now;
                        break;
                    case "STOU":
                        response = StoreUnique();
                        logEntry.Date = DateTime.Now;
                        break;
                    case "APPE":
                        response = Append(cmd.Arguments.FirstOrDefault());
                        logEntry.Date = DateTime.Now;
                        break;
                    case "LIST":
                        response = List(cmd.Arguments.FirstOrDefault() ?? _currentDirectory);
                        logEntry.Date = DateTime.Now;
                        break;
                    case "SYST":
                        response = GetResponse(FtpResponses.SYSTEM);
                        break;
                    case "NOOP":
                        response = GetResponse(FtpResponses.OK);
                        break;
                    case "ACCT":
                        response = Account(cmd.Arguments.FirstOrDefault());
                        break;
                    case "ALLO":
                        response = GetResponse(FtpResponses.OK);
                        break;
                    case "NLST":
                        response = NameList(cmd.Arguments.FirstOrDefault() ?? _currentDirectory);
                        break;
                    case "SITE":
                        response = GetResponse(FtpResponses.NOT_IMPLEMENTED);
                        break;
                    case "STAT":
                        response = GetResponse(FtpResponses.NOT_IMPLEMENTED);
                        break;
                    case "HELP":
                        response = GetResponse(FtpResponses.NOT_IMPLEMENTED);
                        break;
                    case "SMNT":
                        response = GetResponse(FtpResponses.NOT_IMPLEMENTED);
                        break;
                    case "REST":
                        response = GetResponse(FtpResponses.NOT_IMPLEMENTED);
                        break;
                    case "ABOR":
                        response = GetResponse(FtpResponses.NOT_IMPLEMENTED);
                        break;

                    // Extensions defined by rfc 2228
                    case "AUTH":
                        response = Auth(cmd.Arguments.FirstOrDefault());
                        break;

                    // Extensions defined by rfc 2389
                    case "FEAT":
                        response = GetResponse(FtpResponses.FEATURES);
                        break;
                    case "OPTS":
                        response = Options(cmd.Arguments);
                        break;

                    // Extensions defined by rfc 3659
                    case "MDTM":
                        response = FileModificationTime(cmd.Arguments.FirstOrDefault());
                        break;
                    case "SIZE":
                        response = FileSize(cmd.Arguments.FirstOrDefault());
                        break;

                    // Extensions defined by rfc 2428
                    case "EPRT":
                        response = EPort(cmd.RawArguments);
                        logEntry.CPort = _dataEndpoint.Port.ToString(CultureInfo.InvariantCulture);
                        break;
                    case "EPSV":
                        response = EPassive();
                        logEntry.SPort = ((IPEndPoint)_passiveListener.LocalEndpoint).Port.ToString(CultureInfo.InvariantCulture);
                        break;

                    // Extensions defined by rfc 2640
                    case "LANG":
                        response = Language(cmd.Arguments.FirstOrDefault());
                        break;

                    default:
                        response = GetResponse(FtpResponses.NOT_IMPLEMENTED);
                        break;
                }
            }

            logEntry.CSMethod = cmd.Code;
            logEntry.CSUsername = _username;
            logEntry.SCStatus = response.Code;

            _log.Info(logEntry);

            return response;
        }

        protected override void OnConnected()
        {
            FtpPerformanceCounters.IncrementCurrentConnections();

            _connected = true;

            Write(GetResponse(FtpResponses.SERVICE_READY));

            _validCommands.AddRange(new string[] { "AUTH", "USER", "PASS", "ACCT", "QUIT", "HELP", "NOOP" });

            _dataClient = new TcpClient();

            Read();
        }

        protected override void OnCommandComplete(Command cmd)
        {
            if (cmd.Code == "AUTH")
            {
                _cert = new X509Certificate("server2.cer");

                _sslStream = new SslStream(ControlStream);

                _sslStream.AuthenticateAsServer(_cert);
            }

            FtpPerformanceCounters.IncrementCommandsExecuted();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!_disposed)
                {
                    _disposed = true;

                    if (_currentUser != null)
                        if (_currentUser.IsAnonymous)
                            FtpPerformanceCounters.DecrementAnonymousUsers();
                        else
                            FtpPerformanceCounters.DecrementNonAnonymousUsers();

                    if (_connected)
                        FtpPerformanceCounters.DecrementCurrentConnections();

                    if (disposing)
                    {
                        if (_dataClient != null)
                        {
                            _dataClient.Close();
                            _dataClient = null;
                        }

                        if (_sslStream != null)
                        {
                            _sslStream.Dispose();
                            _sslStream = null;
                        }
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        protected override void Read()
        {
            if (_sslStream != null)
            {
                Read(_sslStream);
            }
            else
            {
                Read(ControlStream);
            }
        }

        protected override void Write(string content)
        {
            if (_sslStream != null)
            {
                Write(_sslStream, content);
            }
            else
            {
                Write(ControlStream, content);
            }
        }

        #endregion

        private bool IsPathValid(string path)
        {
            return path.StartsWith(_root, StringComparison.OrdinalIgnoreCase);
        }

        private string NormalizeFilename(string path)
        {
            if (path == null)
            {
                path = string.Empty;
            }

            if (_invalidPathChars.IsMatch(path))
            {
                return null;
            }

            if (path == "/")
            {
                return _root;
            }
            else if (path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                path = new FileInfo(Path.Combine(_root, path.Substring(1))).FullName;
            }
            else
            {
                path = new FileInfo(Path.Combine(_currentDirectory, path)).FullName;
            }

            return IsPathValid(path) ? path : null;
        }

        private Response CheckUser()
        {
            if (_currentUser == null)
            {
                return GetResponse(FtpResponses.NOT_LOGGED_IN);
            }

            return null;
        }

        private long CopyStream(Stream input, Stream output, Action<int> perfAction)
        {
            Stream limitedStream = output;

            if (_connectionType == TransferType.Image)
            {
                return CopyStream(input, limitedStream, BUFFER_SIZE, perfAction);
            }
            else
            {
                //BUG:ANSI模式传输
                return CopyStream(input, limitedStream, BUFFER_SIZE, Encoding.BigEndianUnicode, perfAction);
            }
        }

        private Response GetResponse(Response response)
        {
            return response.SetCulture(_currentCulture);
        }

        #region FTP Commands

        /// <summary>
        /// USER Command - RFC 959 - Section 4.1.1
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response User(string username)
        {
            FtpPerformanceCounters.IncrementTotalLogonAttempts();

            _username = username;
            return GetResponse(FtpResponses.USER_OK);
        }

        /// <summary>
        /// PASS Command - RFC 959 - Section 4.1.1
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private Response Password(string password)
        {
            FtpUser user = FtpUserStore.Validate(_username, password);

            if (user != null)
            {
                //if (!string.IsNullOrEmpty(user.TwoFactorSecret))
                //{
                //    _password = password;

                //    return GetResponse(FtpResponses.NEED_VERIFICATION);
                //}
                //else
                //{
                    _currentUser = user;

                    _root = _currentUser.HomeDir;
                    _currentDirectory = _root;

                    if (_currentUser.IsAnonymous)
                        FtpPerformanceCounters.IncrementAnonymousUsers();
                    else
                        FtpPerformanceCounters.IncrementNonAnonymousUsers();

                    return GetResponse(FtpResponses.LOGGED_IN);
                //}
            }
            else
            {
                return GetResponse(FtpResponses.NOT_LOGGED_IN);
            }
        }

        /// <summary>
        /// ACCT Command - RFC 959 - Section 4.1.1
        /// <para>注：RFC协议没有明确指定ACCT的具体功能，因此此处可以采取多种实现</para>
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response Account(string twoFactorCode)
        {
            //_currentUser = FtpUserStore.Validate(_username, _password, twoFactorCode);

            //if (_currentUser != null)
            //{
            //    _root = _currentUser.HomeDir;
            //    _currentDirectory = _root;

            //    return GetResponse(FtpResponses.LOGGED_IN);
            //}
            //else
            //{
            //    return GetResponse(FtpResponses.NOT_LOGGED_IN);
            //}
            return GetResponse(FtpResponses.NOT_IMPLEMENTED);
        }

        /// <summary>
        /// CWD Command - RFC 959 - Section 4.1.1
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response ChangeWorkingDirectory(string pathname)
        {
            if (pathname == "/")
            {
                _currentDirectory = _root;
            }
            else
            {
                string newDir;

                if (pathname.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    pathname = pathname.Substring(1).Replace('/', '\\');
                    newDir = Path.Combine(_root, pathname);
                }
                else
                {
                    pathname = pathname.Replace('/', '\\');
                    newDir = Path.Combine(_currentDirectory, pathname);
                }

                if (Directory.Exists(newDir))
                {
                    _currentDirectory = new DirectoryInfo(newDir).FullName;

                    if (!IsPathValid(_currentDirectory))
                    {
                        _currentDirectory = _root;
                    }
                }
                else
                {
                    _currentDirectory = _root;
                }
            }

            return GetResponse(FtpResponses.OK);
        }

        /// <summary>
        /// PORT Command - RFC 959 - Section 4.1.2
        /// </summary>
        /// <param name="hostPort"></param>
        /// <returns></returns>
        private Response Port(string hostPort)
        {
            _dataConnectionType = DataConnectionType.Active;

            string[] ipAndPort = hostPort.Split(',');

            byte[] ipAddress = ipAndPort.Take(4).Select(s => Convert.ToByte(s, CultureInfo.InvariantCulture)).ToArray();
            byte[] port = ipAndPort.Skip(4).Select(s => Convert.ToByte(s, CultureInfo.InvariantCulture)).ToArray();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(port);

            _dataEndpoint = new IPEndPoint(new IPAddress(ipAddress), BitConverter.ToInt16(port, 0));

            return GetResponse(FtpResponses.OK);
        }

        private Response EPort(string hostPort)
        {
            _dataConnectionType = DataConnectionType.Active;

            char delimiter = hostPort[0];

            string[] rawSplit = hostPort.Split(new char[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

            char ipType = rawSplit[0][0];

            string ipAddress = rawSplit[1];
            string port = rawSplit[2];

            _dataEndpoint = new IPEndPoint(IPAddress.Parse(ipAddress), int.Parse(port));

            return GetResponse(FtpResponses.OK);
        }

        /// <summary>
        /// PASV Command - RFC 959 - Section 4.1.2
        /// <para>进入被动模式（请求服务器等待数据连接）</para>
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response Passive()
        {
            _dataConnectionType = DataConnectionType.Passive;

            IPAddress localIp = ((IPEndPoint)ControlClient.Client.LocalEndPoint).Address;

            _passiveListener = PassiveListenerPool.GetListener(localIp);

            try
            {
                _passiveListener.Start();
            }
            catch
            {
                _log.Error("No more ports available");
                return GetResponse(FtpResponses.UNABLE_TO_OPEN_DATA_CONNECTION);
            }

            IPEndPoint passiveListenerEndpoint = (IPEndPoint)_passiveListener.LocalEndpoint;

            byte[] address = passiveListenerEndpoint.Address.GetAddressBytes();
            ushort port = (ushort)passiveListenerEndpoint.Port;

            byte[] portArray = BitConverter.GetBytes(port);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(portArray);

            return GetResponse(FtpResponses.ENTERING_PASSIVE_MODE.SetData(address[0], address[1], address[2], address[3], portArray[0], portArray[1]));
        }

        private Response EPassive()
        {
            _dataConnectionType = DataConnectionType.Passive;

            IPAddress localIp = ((IPEndPoint)ControlClient.Client.LocalEndPoint).Address;

            _passiveListener = PassiveListenerPool.GetListener(localIp);

            try
            {
                _passiveListener.Start();
            }
            catch
            {
                _log.Error("No more ports available");
                return GetResponse(FtpResponses.UNABLE_TO_OPEN_DATA_CONNECTION);
            }

            IPEndPoint passiveListenerEndpoint = (IPEndPoint)_passiveListener.LocalEndpoint;

            return GetResponse(FtpResponses.ENTERING_EXTENDED_PASSIVE_MODE.SetData(passiveListenerEndpoint.Port));
        }

        /// <summary>
        /// TYPE Command - RFC 959 - Section 4.1.2
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response Type(string typeCode, string formatControl)
        {
            switch (typeCode.ToUpperInvariant())
            {
                case "A":
                    _connectionType = TransferType.Ascii;
                    break;
                case "I":
                    _connectionType = TransferType.Image;
                    break;
                default:
                    return GetResponse(FtpResponses.NOT_IMPLEMENTED_FOR_PARAMETER);
            }

            if (!string.IsNullOrWhiteSpace(formatControl))
            {
                switch (formatControl.ToUpperInvariant())
                {
                    case "N":
                        _formatControlType = FormatControlType.NonPrint;
                        break;
                    default:
                        return GetResponse(FtpResponses.NOT_IMPLEMENTED_FOR_PARAMETER);
                }
            }

            return GetResponse(FtpResponses.OK);
        }

        /// <summary>
        /// STRU Command - RFC 959 - Section 4.1.2
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response Structure(string structure)
        {
            switch (structure)
            {
                case "F":
                    _fileStructureType = FileStructureType.File;
                    break;
                case "R":
                case "P":
                    return GetResponse(FtpResponses.NOT_IMPLEMENTED_FOR_PARAMETER);
                default:
                    return GetResponse(FtpResponses.PARAMETER_NOT_RECOGNIZED.SetData(structure));
            }

            return GetResponse(FtpResponses.OK);
        }

        /// <summary>
        /// MODE Command - RFC 959 - Section 4.1.2
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response Mode(string mode)
        {
            if (mode.ToUpperInvariant() == "S")
            {
                return GetResponse(FtpResponses.OK);
            }
            else
            {
                return GetResponse(FtpResponses.NOT_IMPLEMENTED_FOR_PARAMETER);
            }
        }

        /// <summary>
        /// RETR Command - RFC 959 - Section 4.1.3
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response Retrieve(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname != null)
            {
                if (File.Exists(pathname))
                {
                    var state = new DataConnectionOperation { Arguments = pathname, Operation = RetrieveOperation };

                    SetupDataConnectionOperation(state);

                    return GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType, "RETR"));
                }
            }

            return GetResponse(FtpResponses.FILE_NOT_FOUND);
        }

        /// <summary>
        /// STOR Command - RFC 959 - Section 4.1.3
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response Store(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname != null)
            {
                var state = new DataConnectionOperation { Arguments = pathname, Operation = StoreOperation };

                SetupDataConnectionOperation(state);

                return GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType, "STOR"));
            }

            return GetResponse(FtpResponses.FILE_ACTION_NOT_TAKEN);
        }

        /// <summary>
        /// STOU Command - RFC 959 - Section 4.1.3
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response StoreUnique()
        {
            string pathname = NormalizeFilename(new Guid().ToString());

            var state = new DataConnectionOperation { Arguments = pathname, Operation = StoreOperation };

            SetupDataConnectionOperation(state);

            return GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType, "STOU"));
        }

        /// <summary>
        /// APPE Command - RFC 959 - Section 4.1.3
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response Append(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname != null)
            {
                var state = new DataConnectionOperation { Arguments = pathname, Operation = AppendOperation };

                SetupDataConnectionOperation(state);

                return GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType, "APPE"));
            }

            return GetResponse(FtpResponses.FILE_ACTION_NOT_TAKEN);
        }

        /// <summary>
        /// RNFR - RNTO - RFC 959 - Seciton 4.1.3
        /// </summary>
        /// <param name="renameFrom"></param>
        /// <param name="renameTo"></param>
        /// <returns></returns>
        private Response Rename(string renameFrom, string renameTo)
        {
            if (string.IsNullOrWhiteSpace(renameFrom) || string.IsNullOrWhiteSpace(renameTo))
            {
                return GetResponse(FtpResponses.FILE_ACTION_NOT_TAKEN);
            }

            renameFrom = NormalizeFilename(renameFrom);
            renameTo = NormalizeFilename(renameTo);

            if (renameFrom != null && renameTo != null)
            {
                if (File.Exists(renameFrom))
                {
                    File.Move(renameFrom, renameTo);
                }
                else if (Directory.Exists(renameFrom))
                {
                    Directory.Move(renameFrom, renameTo);
                }
                else
                {
                    return GetResponse(FtpResponses.FILE_ACTION_NOT_TAKEN);
                }

                return GetResponse(FtpResponses.FILE_ACTION_COMPLETE);
            }

            return GetResponse(FtpResponses.FILE_ACTION_NOT_TAKEN);
        }

        /// <summary>
        /// DELE Command - RFC 959 - Section 4.1.3
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response Delete(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname != null)
            {
                if (File.Exists(pathname))
                {
                    File.Delete(pathname);
                }
                else
                {
                    return GetResponse(FtpResponses.FILE_NOT_FOUND);
                }

                return GetResponse(FtpResponses.FILE_ACTION_COMPLETE);
            }

            return GetResponse(FtpResponses.FILE_NOT_FOUND);
        }

        /// <summary>
        /// RMD Command - RFC 959 - Section 4.1.3
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response RemoveDir(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname != null)
            {
                if (Directory.Exists(pathname))
                {
                    Directory.Delete(pathname);
                }
                else
                {
                    return GetResponse(FtpResponses.DIRECTORY_NOT_FOUND);
                }

                return GetResponse(FtpResponses.FILE_ACTION_COMPLETE);
            }

            return GetResponse(FtpResponses.DIRECTORY_NOT_FOUND);
        }

        /// <summary>
        /// MKD Command - RFC 959 - Section 4.1.3
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response CreateDir(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname != null)
            {
                if (!Directory.Exists(pathname))
                {
                    Directory.CreateDirectory(pathname);
                }
                else
                {
                    return GetResponse(FtpResponses.DIRECTORY_EXISTS);
                }

                return GetResponse(FtpResponses.FILE_ACTION_COMPLETE);
            }

            return GetResponse(FtpResponses.DIRECTORY_NOT_FOUND);
        }

        /// <summary>
        /// PWD Command - RFC 959 - Section 4.1.3
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response PrintWorkingDirectory()
        {
            string current = _currentDirectory.Replace(_root, string.Empty).Replace('\\', '/');

            if (current.Length == 0)
            {
                current = "/";
            }

            return GetResponse(FtpResponses.CURRENT_DIRECTORY.SetData(current));
        }

        private Response NameList(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname != null)
            {
                var state = new DataConnectionOperation { Arguments = pathname, Operation = NameListOperation };

                SetupDataConnectionOperation(state);

                return GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType, "NLST"));
            }

            return GetResponse(FtpResponses.FILE_ACTION_NOT_TAKEN);
        }


        /// <summary>
        /// LIST Command - RFC 959 - Section 4.1.3
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response List(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname != null)
            {
                var state = new DataConnectionOperation { Arguments = pathname, Operation = ListOperation };

                SetupDataConnectionOperation(state);

                return GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType, "LIST"));
            }

            return GetResponse(FtpResponses.FILE_ACTION_NOT_TAKEN);
        }

        /// <summary>
        /// AUTH Command - RFC 2228 - Section 3
        /// </summary>
        /// <param name="authMode"></param>
        /// <returns></returns>
        private Response Auth(string authMode)
        {
            if (authMode == "TLS")
            {
                return GetResponse(FtpResponses.ENABLING_TLS);
            }
            else
            {
                return GetResponse(FtpResponses.NOT_IMPLEMENTED_FOR_PARAMETER);
            }
        }

        /// <summary>
        /// OPTS Command - RFC 2389 - Section 4
        /// </summary>
        /// <param name="arguments">command-name [ SP command-options ]</param>
        /// <returns></returns>
        private Response Options(List<string> arguments)
        {
            if (arguments.FirstOrDefault() == "UTF8" && arguments.Skip(1).FirstOrDefault() == "ON")
            {
                _currentEncoding = Encoding.UTF8;
                ControlStreamEncoding = Encoding.UTF8;

                return GetResponse(FtpResponses.UTF8_ENCODING_ON);
            }

            return GetResponse(FtpResponses.OK);
        }

        /// <summary>
        /// MDTM Command - RFC 3659 - Section 3
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response FileModificationTime(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname != null)
            {
                if (File.Exists(pathname))
                {
                    return new Response { Code = "213", Text = File.GetLastWriteTime(pathname).ToString("yyyyMMddHHmmss.fff", CultureInfo.InvariantCulture) };
                }
            }

            return GetResponse(FtpResponses.FILE_NOT_FOUND);
        }

        /// <summary>
        /// SIZE Command - RFC 3659 - Section 4
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response FileSize(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname != null)
            {
                if (File.Exists(pathname))
                {
                    long length = 0;

                    using (FileStream fs = File.Open(pathname, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        length = fs.Length;
                    }

                    return new Response { Code = "213", Text = length.ToString(CultureInfo.InvariantCulture) };
                }
            }

            return GetResponse(FtpResponses.FILE_NOT_FOUND);
        }

        /// <summary>
        /// LANG Command - RFC 2640 - Section 4
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        private Response Language(string language)
        {
            try
            {
                var culture = CultureInfo.GetCultureInfo(language);

                ResourceSet rs = FtpReplies.ResourceManager.GetResourceSet(culture, true, false);

                if (rs == null)
                {
                    _currentCulture = CultureInfo.CurrentCulture;
                    return new Response { Code = "504", Text = "Language not implemented, using en-US" };
                }
                else
                {
                    _currentCulture = culture;

                    return new Response { Code = "200", Text = "Changed language to what you asked for" };
                }
            }
            catch
            {
                _currentCulture = CultureInfo.CurrentCulture;
                return new Response { Code = "500", Text = "Invalid language, using en-US" };
            }
        }

        #endregion

        #region DataConnection Operations

        private void HandleAsyncResult(IAsyncResult result)
        {
            if (_dataConnectionType == DataConnectionType.Active)
            {
                _dataClient.EndConnect(result);
            }
            else
            {
                _dataClient = _passiveListener.EndAcceptTcpClient(result);
            }
        }

        private void SetupDataConnectionOperation(DataConnectionOperation state)
        {
            if (_dataConnectionType == DataConnectionType.Active)
            {
                _dataClient = new TcpClient(_dataEndpoint.AddressFamily);
                _dataClient.BeginConnect(_dataEndpoint.Address, _dataEndpoint.Port, DoDataConnectionOperation, state);
            }
            else
            {
                _passiveListener.BeginAcceptTcpClient(DoDataConnectionOperation, state);
            }
        }

        private void DoDataConnectionOperation(IAsyncResult result)
        {
            FtpPerformanceCounters.IncrementTotalConnectionAttempts();
            FtpPerformanceCounters.IncrementCurrentConnections();

            HandleAsyncResult(result);

            DataConnectionOperation op = result.AsyncState as DataConnectionOperation;

            Response response;

            try
            {
                using (NetworkStream dataStream = _dataClient.GetStream())
                {
                    response = op.Operation(dataStream, op.Arguments);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                response = GetResponse(FtpResponses.TRANSFER_ABORTED);
            }

            if (_dataClient != null)
            {
                _dataClient.Close();
                _dataClient = null;
            }

            FtpPerformanceCounters.DecrementCurrentConnections();

            if (_dataConnectionType == DataConnectionType.Passive)
                PassiveListenerPool.FreeListener(_passiveListener);

            Write(response.ToString());
        }

        private Response RetrieveOperation(NetworkStream dataStream, string pathname)
        {
            using (FileStream fs = new FileStream(pathname, FileMode.Open, FileAccess.Read))
            {
                CopyStream(fs, dataStream, FtpPerformanceCounters.IncrementBytesSent);
            }

            FtpPerformanceCounters.IncrementFilesSent();

            return GetResponse(FtpResponses.TRANSFER_SUCCESSFUL);
        }

        private Response StoreOperation(NetworkStream dataStream, string pathname)
        {
            long bytes = 0;

            using (FileStream fs = new FileStream(pathname, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, BUFFER_SIZE, FileOptions.SequentialScan))
            {
                bytes = CopyStream(dataStream, fs, FtpPerformanceCounters.IncrementBytesReceived);
            }

            FtpLogEntry logEntry = new FtpLogEntry
            {
                Date = DateTime.Now,
                CIP = ClientIP,
                CSMethod = "STOR",
                CSUsername = _username,
                SCStatus = "226",
                CSBytes = bytes.ToString(CultureInfo.InvariantCulture)
            };

            _log.Info(logEntry);

            FtpPerformanceCounters.IncrementFilesReceived();

            return GetResponse(FtpResponses.TRANSFER_SUCCESSFUL);
        }

        private Response AppendOperation(NetworkStream dataStream, string pathname)
        {
            long bytes = 0;

            using (FileStream fs = new FileStream(pathname, FileMode.Append, FileAccess.Write, FileShare.None, BUFFER_SIZE, FileOptions.SequentialScan))
            {
                bytes = CopyStream(dataStream, fs, FtpPerformanceCounters.IncrementBytesReceived);
            }

            FtpLogEntry logEntry = new FtpLogEntry
            {
                Date = DateTime.Now,
                CIP = ClientIP,
                CSMethod = "APPE",
                CSUsername = _username,
                SCStatus = "226",
                CSBytes = bytes.ToString(CultureInfo.InvariantCulture)
            };

            _log.Info(logEntry);

            FtpPerformanceCounters.IncrementFilesReceived();

            return GetResponse(FtpResponses.TRANSFER_SUCCESSFUL);
        }

        /// <summary>
        /// 列目录操作
        /// </summary>
        /// <param name="dataStream"></param>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response ListOperation(NetworkStream dataStream, string pathname)
        {
            DateTime now = DateTime.Now;

            StreamWriter dataWriter = new StreamWriter(dataStream, _currentEncoding);

            IEnumerable<string> directories = Directory.EnumerateDirectories(pathname);

            foreach (string dir in directories)
            {
                DateTime editDate = Directory.GetLastWriteTime(dir);

                string date = editDate < now.Subtract(TimeSpan.FromDays(180)) ?
                    editDate.ToString("MMM dd  yyyy", CultureInfo.InvariantCulture) :
                    editDate.ToString("MMM dd HH:mm", CultureInfo.InvariantCulture);

                dataWriter.Write("drwxr-xr-x    2 2003     2003         4096 ");
                dataWriter.Write(date);
                dataWriter.Write(' ');
                dataWriter.WriteLine(Path.GetFileName(dir));

                dataWriter.Flush();
            }

            IEnumerable<string> files = Directory.EnumerateFiles(pathname);

            foreach (string file in files)
            {
                FileInfo f = new FileInfo(file);

                string date = f.LastWriteTime < now.Subtract(TimeSpan.FromDays(180)) ?
                    f.LastWriteTime.ToString("MMM dd  yyyy", CultureInfo.InvariantCulture) :
                    f.LastWriteTime.ToString("MMM dd HH:mm", CultureInfo.InvariantCulture);

                //权限 所有者 组
                dataWriter.Write("-rw-r--r--    2 2003     2003     ");

                string length = f.Length.ToString(CultureInfo.InvariantCulture);

                if (length.Length < 8)
                {
                    for (int i = 0; i < 8 - length.Length; i++)
                    {
                        dataWriter.Write(' ');
                    }
                }

                dataWriter.Write(length);
                dataWriter.Write(' ');
                dataWriter.Write(date);
                dataWriter.Write(' ');
                dataWriter.WriteLine(f.Name);
                //Console.WriteLine(f.Name);
                dataWriter.Flush();

                f = null;
            }

            FtpLogEntry logEntry = new FtpLogEntry
            {
                Date = now,
                CIP = ClientIP,
                CSMethod = "LIST",
                CSUsername = _username,
                SCStatus = "226"
            };

            _log.Info(logEntry);

            return GetResponse(FtpResponses.TRANSFER_SUCCESSFUL);
        }

        private Response NameListOperation(NetworkStream dataStream, string pathname)
        {
            StreamWriter dataWriter = new StreamWriter(dataStream, _currentEncoding);

            IEnumerable<string> files = Directory.EnumerateFiles(pathname);

            foreach (string file in files)
            {
                dataWriter.WriteLine(Path.GetFileName(file));
                dataWriter.Flush();
            }

            FtpLogEntry logEntry = new FtpLogEntry
            {
                Date = DateTime.Now,
                CIP = ClientIP,
                CSMethod = "NLST",
                CSUsername = _username,
                SCStatus = "226"
            };

            _log.Info(logEntry);

            return GetResponse(FtpResponses.TRANSFER_SUCCESSFUL);
        }

        #endregion
    }
}

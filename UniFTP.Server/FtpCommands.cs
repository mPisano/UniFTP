using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Resources;
using System.Text;
using SharpServer;
using UniFTP.Common.Localization;
using UniFTP.Server.Virtual;

namespace UniFTP.Server
{
    public partial class FtpClientConnection : ClientConnection
    {
        private string _renameFrom;
        private long _transPosition = 0;
        private Command _lastCommand = null;

        protected override Response HandleCommand(Command cmd)
        {
            Console.WriteLine(cmd.Raw);

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
                response = CheckUser(); //如果被拒绝将直接发送权限认证的响应
            }

            // RNFR重命名后面必须立即接RNTO命令
            if (cmd.Code != "RNTO")
            {
                _renameFrom = null;
            }
            // REST重新开始后面必须立即接传输命令
            if (cmd.Code != "RETR" && cmd.Code != "STOR")
            {
                _transPosition = 0;
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
                        response = ChangeWorkingDirectory(cmd.RawArguments);
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
                        _renameFrom = cmd.RawArguments;
                        response = GetResponse(FtpResponses.RENAME_FROM);
                        break;
                    case "RNTO":
                        response = Rename(_renameFrom, cmd.RawArguments);
                        break;
                    case "DELE":    //删除文件
                        response = Delete(cmd.RawArguments);
                        break;
                    case "RMD":     //删除文件夹
                        response = RemoveDir(cmd.RawArguments);
                        break;
                    case "MKD":     //建立文件夹
                        response = CreateDir(cmd.RawArguments);
                        break;
                    case "PWD":     //显示目录
                        response = PrintWorkingDirectory();
                        break;
                    case "RETR":    //下载文件
                        //FIXED:文件名含空格
                        response = Retrieve(cmd.RawArguments);
                        logEntry.Date = DateTime.Now;
                        break;
                    case "STOR":    //上传文件
                        response = Store(cmd.RawArguments);
                        logEntry.Date = DateTime.Now;
                        break;
                    case "STOU":
                        response = StoreUnique(cmd.RawArguments);
                        logEntry.Date = DateTime.Now;
                        break;
                    case "APPE":
                        response = Append(cmd.RawArguments);
                        logEntry.Date = DateTime.Now;
                        break;
                    case "LIST":    //列出目录文件
                        response = List(cmd.RawArguments);
                        logEntry.Date = DateTime.Now;
                        break;
                    case "SYST":
                        response = GetResponse(FtpResponses.SYSTEM);
                        break;
                    case "NOOP":    //空指令 只为获取服务器响应
                        response = GetResponse(FtpResponses.OK);
                        break;
                    case "ACCT":
                        response = Account(cmd.RawArguments);
                        break;
                    case "ALLO":    //分配 (对于不需要分配存储空间的机器，它的作用等于NOOP)
                        response = GetResponse(FtpResponses.OK);
                        break;
                    case "NLST":
                        response = NameList(cmd.RawArguments);
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
                    case "REST":    //重新开始 //ADDED:尝试断点续传
                        response = Restart(cmd.RawArguments);
                        break;
                    case "ABOR":
                        response = GetResponse(FtpResponses.NOT_IMPLEMENTED);
                        break;

                    // Extensions defined by rfc 2228
                    case "AUTH":    //权限验证 AUTH <验证方法>
                        response = Auth(cmd.RawArguments);
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
                        response = FileModificationTime(cmd.RawArguments);//FIXED:只接受一个参数的 务必使用RawArg
                        break;
                    case "SIZE":
                        response = FileSize(cmd.RawArguments);
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

            _lastCommand = cmd;

            return response;
        }

        #region FTP Commands

        /// <summary>
        /// USER Command - RFC 959 - Section 4.1.1
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response User(string username)
        {
            _performanceCounter.IncrementTotalLogonAttempts();

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
            FtpUser user = FtpUser.Validate((FtpServer)CurrentServer, _username, password);

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
                
                _virtualFileSystem = new VirtualFileSystem(((FtpServer)CurrentServer).Config,user.UserGroup);
                //TODO:引入新的虚拟文件系统
                
                if (_currentUser.IsAnonymous)
                    _performanceCounter.IncrementAnonymousUsers();
                else
                    _performanceCounter.IncrementNonAnonymousUsers();

                if (((FtpServer)CurrentServer).Config.LogInWelcome != null)
                {
                    Write(new Response { Code = "230-", Text = "Logged In" });
                    foreach (var welcome in ((FtpServer)CurrentServer).Config.LogInWelcome)
                    {
                        Write(new Response { Code = "", Text = welcome });
                    }
                }
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
        /// <param name="account"></param>
        /// <returns></returns>
        private Response Account(string account)
        {
            return GetResponse(FtpResponses.NOT_IMPLEMENTED);
        }

        /// <summary>
        /// CWD Command - RFC 959 - Section 4.1.1
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response ChangeWorkingDirectory(string pathname)
        {
            _virtualFileSystem.ChangeCurrentDirectory(pathname);
            //if (pathname == "/")
            //{
            //    _currentDirectory = _root;
            //}
            //else
            //{
            //    string newDir;

            //    if (pathname.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            //    {
            //        pathname = pathname.Substring(1).Replace('/', '\\');
            //        newDir = Path.Combine(_root, pathname);
            //    }
            //    else
            //    {
            //        pathname = pathname.Replace('/', '\\');
            //        newDir = Path.Combine(_currentDirectory, pathname);
            //    }

            //    if (Directory.Exists(newDir))
            //    {
            //        _currentDirectory = new DirectoryInfo(newDir).FullName;

            //        if (!IsPathValid(_currentDirectory))
            //        {
            //            _currentDirectory = _root;
            //        }
            //    }
            //    else
            //    {
            //        _currentDirectory = _root;
            //    }
            //}

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

            _dataEndpoint = new IPEndPoint(new IPAddress(ipAddress), BitConverter.ToUInt16(port, 0)); //FIXED:端口号16位无负数！

            return GetResponse(FtpResponses.OK);
        }

        /// <summary>
        /// EPRT Command - RFC 2428
        /// </summary>
        /// <param name="hostPort"></param>
        /// <returns></returns>
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
        /// 
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

        /// <summary>
        /// EPSV Command - RFC 2428
        /// </summary>
        /// <returns></returns>
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
                //case "E":
                //    _connectionType = TransferType.Ebcdic;
                //    break;
                //case "L":
                //    _connectionType = TransferType.Local;
                //    break;
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

                case "F":   //文件结构
                    _fileStructureType = FileStructureType.File;
                    break;
                case "R":   //记录结构
                case "P":   //页结构
                    return GetResponse(FtpResponses.NOT_IMPLEMENTED_FOR_PARAMETER);
                default:
                    return GetResponse(FtpResponses.PARAMETER_NOT_RECOGNIZED.SetData(structure));
            }

            return GetResponse(FtpResponses.OK);
        }

        /// <summary>
        /// MODE Command - RFC 959 - Section 4.1.2
        /// </summary>
        /// <param name="mode"></param>
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
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response Retrieve(string pathname)
        {
            FileInfo f = _virtualFileSystem.GetFile(pathname);

            if (f != null)
            {
                    var state = new DataConnectionOperation { Arguments = f.FullName, Operation = RetrieveOperation };

                    SetupDataConnectionOperation(state);

                    return GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType, "RETR"));
            }

            return GetResponse(FtpResponses.FILE_NOT_FOUND);
        }

        ///ADDED:重启传输命令
        /// <summary>
        /// REST Command - RFC 959 - Section 4.1.3
        /// <para>重启传输</para>
        /// </summary>
        /// <param name="position">无符号整型数，表示传输重启位置</param>
        /// <returns></returns>
        private Response Restart(string position)
        {
            long pos = 0;
            if (!long.TryParse(position,out pos))
            {
                return GetResponse(FtpResponses.PARAMETER_NOT_RECOGNIZED.SetData(position));
            }
            _transPosition = pos;
            return GetResponse(FtpResponses.RESTART_FROM.SetData(pos));
        }

        /// <summary>
        /// STOR Command - RFC 959 - Section 4.1.3
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response Store(string pathname)
        {
            string pre = _virtualFileSystem.GetRealPathOfFile(pathname);

            if (pre != null)
            {
                var state = new DataConnectionOperation { Arguments = pre, Operation = StoreOperation };

                SetupDataConnectionOperation(state);

                return GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType, "STOR"));
            }

            return GetResponse(FtpResponses.FILE_ACTION_NOT_TAKEN);
        }

        /// <summary>
        /// STOU Command - RFC 959 - Section 4.1.3
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response StoreUnique(string pathname)
        {
            string pre = _virtualFileSystem.GetRealPathOfFile(pathname, new Guid().ToString());
            //string pathname = NormalizeFilename(new Guid().ToString());

            var state = new DataConnectionOperation { Arguments = pathname, Operation = StoreOperation };

            SetupDataConnectionOperation(state);

            return GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType, "STOU"));
        }

        /// <summary>
        /// APPE Command - RFC 959 - Section 4.1.3
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response Append(string pathname)
        {
            var f = _virtualFileSystem.GetFile(pathname);
            
            if (f != null)
            {
                var state = new DataConnectionOperation { Arguments = f.FullName, Operation = AppendOperation };

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

            FileError result = _virtualFileSystem.Rename(renameFrom, renameTo);
            if (result!= FileError.None)
            {
                return GetResponse(FtpResponses.FILE_ACTION_NOT_TAKEN);
            }
            else
            {
                return GetResponse(FtpResponses.FILE_ACTION_COMPLETE);
            }

            //renameFrom = NormalizeFilename(renameFrom);
            //renameTo = NormalizeFilename(renameTo);

            //if (renameFrom != null && renameTo != null)
            //{
            //    if (File.Exists(renameFrom))
            //    {
            //        File.Move(renameFrom, renameTo);
            //    }
            //    else if (Directory.Exists(renameFrom))
            //    {
            //        Directory.Move(renameFrom, renameTo);
            //    }
            //    else
            //    {
            //        return GetResponse(FtpResponses.FILE_ACTION_NOT_TAKEN);
            //    }

            //    return GetResponse(FtpResponses.FILE_ACTION_COMPLETE);
            //}

            return GetResponse(FtpResponses.FILE_ACTION_NOT_TAKEN);
        }

        /// <summary>
        /// DELE Command - RFC 959 - Section 4.1.3
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response Delete(string pathname)
        {
            bool result = _virtualFileSystem.Delete(pathname);

            if (result)
            {
                return GetResponse(FtpResponses.FILE_ACTION_COMPLETE);
            }
            //if (pathname != null)
            //{
            //    if (File.Exists(pathname))
            //    {
            //        File.Delete(pathname);
            //    }
            //    else
            //    {
            //        return GetResponse(FtpResponses.FILE_NOT_FOUND);
            //    }

            //    return GetResponse(FtpResponses.FILE_ACTION_COMPLETE);
            //}

            return GetResponse(FtpResponses.FILE_NOT_FOUND);
        }

        /// <summary>
        /// RMD Command - RFC 959 - Section 4.1.3
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response RemoveDir(string pathname)
        {
            bool result = _virtualFileSystem.Delete(pathname, true);

            if (result)
            {
                return GetResponse(FtpResponses.FILE_ACTION_COMPLETE);
            }
            //if (pathname != null)
            //{
            //    if (Directory.Exists(pathname))
            //    {
            //        Directory.Delete(pathname);
            //    }
            //    else
            //    {
            //        return GetResponse(FtpResponses.DIRECTORY_NOT_FOUND);
            //    }

            //    return GetResponse(FtpResponses.FILE_ACTION_COMPLETE);
            //}

            return GetResponse(FtpResponses.DIRECTORY_NOT_FOUND);
        }

        /// <summary>
        /// MKD Command - RFC 959 - Section 4.1.3
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response CreateDir(string pathname)
        {
            FileError result = _virtualFileSystem.CreateDirectory(pathname);

            if (result == FileError.AlreadyExist)
            {
                return GetResponse(FtpResponses.DIRECTORY_EXISTS);
            }
            if (result == FileError.NotFound)
            {
                return GetResponse(FtpResponses.DIRECTORY_NOT_FOUND);
            }
            return GetResponse(FtpResponses.FILE_ACTION_COMPLETE);
            
            //if (pathname != null)
            //{
            //    if (!Directory.Exists(pathname))
            //    {
            //        Directory.CreateDirectory(pathname);
            //    }
            //    else
            //    {
            //        return GetResponse(FtpResponses.DIRECTORY_EXISTS);
            //    }

            //    return GetResponse(FtpResponses.FILE_ACTION_COMPLETE);
            //}

            //return GetResponse(FtpResponses.DIRECTORY_NOT_FOUND);
        }

        /// <summary>
        /// PWD Command - RFC 959 - Section 4.1.3
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response PrintWorkingDirectory()
        {
            //string current = _currentDirectory.Replace(_root, string.Empty).Replace('\\', '/');

            //if (current.Length == 0)
            //{
            //    current = "/";
            //}

            return GetResponse(FtpResponses.CURRENT_DIRECTORY.SetData(_virtualFileSystem.CurrentDirectory.VirtualPath));
        }

        private Response NameList(string pathname)
        {
            if (_dataEndpoint == null && _dataConnectionType == DataConnectionType.Active)
            {
                return GetResponse(FtpResponses.UNABLE_TO_OPEN_DATA_CONNECTION);
            }

            if (_virtualFileSystem.ExistsDirectory(pathname))
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
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response List(string pathname)
        {
            if (pathname.Trim().ToLower() == "-l")  //客户端尝试使用UNIX参数强制详细信息显示,事实上我们提供的本来就是详细信息
            {
                pathname = "";
            }
            if (_dataEndpoint == null && _dataConnectionType == DataConnectionType.Active)
            {
                return GetResponse(FtpResponses.UNABLE_TO_OPEN_DATA_CONNECTION);
            }

            if (_virtualFileSystem.ExistsDirectory(pathname))
            {
                var state = new DataConnectionOperation { Arguments = pathname, Operation = ListOperation };

                SetupDataConnectionOperation(state);

                return GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType, "LIST"));
            }

            return GetResponse(FtpResponses.FILE_ACTION_NOT_TAKEN);
        }

        /// <summary>
        /// AUTH Command - RFC 2228 - Section 3
        /// <para>要求认证</para>
        /// </summary>
        /// <param name="authMode">认证模式</param>
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
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response FileModificationTime(string pathname)
        {
            FileInfo f = _virtualFileSystem.GetFile(pathname);

            if (f != null)
            {
                   //FIXED:观察到FileZilla校准时间偏移了8小时，将返回的时间改为UTC时间，是否有效待验证
             return new Response { Code = "213", Text = f.LastWriteTimeUtc.ToString("yyyyMMddHHmmss.fff", CultureInfo.InvariantCulture) };
                
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
            FileInfo f = _virtualFileSystem.GetFile(pathname);

            if (f != null)
            {

                    //long length = 0;

                    //using (FileStream fs = File.Open(pathname, FileMode.Open, FileAccess.Read, FileShare.Read))
                    //{
                    //    length = fs.Length;
                    //}

                    return new Response { Code = "213", Text = f.Length.ToString(CultureInfo.InvariantCulture) };
                
            }
            else if (_virtualFileSystem.GetDirectory(pathname)!=null)
            {
                return new Response { Code = "213", Text = "0" };
            }
            return GetResponse(FtpResponses.FILE_NOT_FOUND);
        }

        /// <summary>
        /// LANG Command - RFC 2640 - Section 4
        /// <para>指定服务端的响应语言</para>
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
                    _currentCulture = CultureInfo.InvariantCulture;
                    return new Response { Code = "504", Text = "Language not implemented, using default language" };
                }
                else
                {
                    _currentCulture = culture;

                    return new Response { Code = "200", Text = string.Format("Language Changed to {0}", _currentCulture) };
                }
            }
            catch
            {
                _currentCulture = CultureInfo.InvariantCulture;
                return new Response { Code = "500", Text = "Invalid language, using default language" };
            }
        }

        #endregion

    }
}

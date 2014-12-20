using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Resources;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
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
        private bool _sslEnabled = false;
        private int _protectBufferSize = 0;
        private readonly string withSsl = " with TLS/SSL";

        protected override Response HandleCommand(Command cmd)
        {
            Console.WriteLine(cmd.Raw);

            Response response = null;

            FtpLogEntry logEntry = new FtpLogEntry
            {
                Date = DateTime.Now,
                CIP = ClientIP,
                CSArgs = cmd.RawArguments
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
                    case "USER":    //用户名
                        response = User(cmd.Arguments.FirstOrDefault());
                        break;
                    case "PASS":    //密码
                        response = Password(cmd.Arguments.FirstOrDefault());
                        logEntry.CSArgs = "******";
                        break;
                    case "CWD":     //切换目录
                        response = ChangeWorkingDirectory(cmd.RawArguments);
                        break;
                    case "CDUP":    //切换到上级目录
                        response = ChangeWorkingDirectory("..");
                        break;
                    case "QUIT":    //退出
                        if (((FtpServer)CurrentServer).Config.LogOutWelcome != null)
                        {
                            Write(new Response { Code = "221-", Text = "Logging Out" });
                            foreach (var welcome in ((FtpServer)CurrentServer).Config.LogOutWelcome)
                            {
                                Write(new Response { Code = "", Text = welcome });
                            }
                        }
                        response = GetResponse(FtpResponses.QUIT);
                        break;
                    case "REIN":    //初始化
                        _currentUser = null;
                        _username = null;
                        _dataClient = null;
                        _currentCulture = CultureInfo.CurrentCulture;
                        _currentEncoding = Encoding.ASCII;
                        ControlStreamEncoding = Encoding.ASCII;

                        response = GetResponse(FtpResponses.SERVICE_READY);
                        break;
                    case "PORT":    //主动模式设置端口
                        response = Port(cmd.RawArguments);
                        logEntry.CPort = _dataEndpoint.Port.ToString(CultureInfo.InvariantCulture);
                        break;
                    case "PASV":    //进入被动模式
                        response = Passive();
                        logEntry.SPort = ((IPEndPoint)_passiveListener.LocalEndpoint).Port.ToString(CultureInfo.InvariantCulture);
                        break;
                    case "TYPE":    //设置传输类型
                        response = Type(cmd.Arguments.FirstOrDefault(), cmd.Arguments.Skip(1).FirstOrDefault());
                        break;
                    case "STRU":    //设置结构
                        response = Structure(cmd.Arguments.FirstOrDefault());
                        break;
                    case "MODE":    //设置模式
                        response = Mode(cmd.Arguments.FirstOrDefault());
                        break;
                    case "RNFR":    //重命名
                        _renameFrom = cmd.RawArguments;
                        response = GetResponse(FtpResponses.RENAME_FROM);
                        break;
                    case "RNTO":    //重命名为
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
                    case "RETR":    //下载文件  //FIXED:文件名含空格
                        response = Retrieve(cmd.RawArguments);
                        logEntry.Date = DateTime.Now;
                        break;
                    case "STOR":    //上传文件
                        response = Store(cmd.RawArguments);
                        logEntry.Date = DateTime.Now;
                        break;
                    case "STOU":    //上传文件（不覆盖现有文件）
                        response = StoreUnique(cmd.RawArguments);
                        logEntry.Date = DateTime.Now;
                        break;
                    case "APPE":    //追加
                        response = Append(cmd.RawArguments);
                        logEntry.Date = DateTime.Now;
                        break;
                    case "LIST":    //列出目录文件
                        response = List(cmd.RawArguments);
                        logEntry.Date = DateTime.Now;
                        break;
                    case "SYST":    //系统类型
                        response = GetResponse(FtpResponses.SYSTEM);
                        break;
                    case "NOOP":    //空指令 只为获取服务器响应
                        response = GetResponse(FtpResponses.OK);
                        break;
                    case "ACCT":    //要求账户(未实现)
                        response = Account(cmd.RawArguments);
                        break;
                    case "ALLO":    //分配 (对于不需要分配存储空间的机器，它的作用等于NOOP)
                        response = GetResponse(FtpResponses.OK);
                        break;
                    case "NLST":    //列文件名
                        response = NameList(cmd.RawArguments);
                        break;
                    case "SITE":    //服务器系统相关命令 //TODO:可能在此处加入搜索
                        response = GetResponse(FtpResponses.NOT_IMPLEMENTED);
                        break;
                    case "STAT":
                        response = GetResponse(FtpResponses.NOT_IMPLEMENTED);
                        break;
                    case "HELP":    //帮助
                        response = GetResponse(FtpResponses.NOT_IMPLEMENTED);
                        break;
                    case "SMNT":
                        response = GetResponse(FtpResponses.NOT_IMPLEMENTED);
                        break;
                    case "REST":    //重新开始 //ADDED:尝试断点续传
                        response = Restart(cmd.RawArguments);
                        break;
                    case "ABOR":    //中断传输，关闭数据连接
                        response = GetResponse(FtpResponses.NOT_IMPLEMENTED);
                        break;

                    // Extensions defined by rfc 2228
                    case "AUTH":    //权限验证 AUTH <验证方法>
                        response = Auth(cmd.RawArguments);
                        break;
                    case "PBSZ":    //设置保护缓冲区大小，在SSL/TLS中永远是0
                        response = ProtectBufferSize(cmd.RawArguments);
                        break;
                    case "PROT":    //保护级别
                        response = ProtectionLevel(cmd.Arguments.FirstOrDefault());
                        break;

                    // Extensions defined by rfc 2389
                    case "FEAT":    //显示扩展指令
                        response = GetResponse(FtpResponses.FEATURES);
                        break;
                    case "OPTS":    //参数设置
                        response = Options(cmd.Arguments);
                        break;

                    // Extensions defined by rfc 3659
                    case "MDTM":    //回显文件修改时间，通常用于对时
                        response = FileModificationTime(cmd.RawArguments);//FIXED:只接受一个参数的 务必使用RawArg
                        break;
                    case "SIZE":    //回显文件大小
                        response = FileSize(cmd.RawArguments);
                        break;
                    case "MLSD":    //标准格式列目录
                        response = MachineListDirectory(cmd.RawArguments);
                        break;
                    case "MLST":    //标准格式列文件信息
                        response = MachineListTime(cmd.RawArguments);
                        break;

                    // Extensions defined by rfc 2428
                    case "EPRT":    //扩展主动模式（IPv6）
                        response = EPort(cmd.RawArguments);
                        logEntry.CPort = _dataEndpoint.Port.ToString(CultureInfo.InvariantCulture);
                        break;
                    case "EPSV":    //扩展被动动模式（IPv6）
                        response = EPassive();
                        logEntry.SPort = ((IPEndPoint)_passiveListener.LocalEndpoint).Port.ToString(CultureInfo.InvariantCulture);
                        break;

                    // Extensions defined by rfc 2640
                    case "LANG":    //切换服务端显示语言
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
            OnLog(logEntry);

            _lastCommand = cmd;

            return response;
        }

        #region FTP Commands

        /// <summary>
        /// USER Command - RFC 959 - Section 4.1.1
        /// <para>账号</para>
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
        /// <para>密码</para>
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private Response Password(string password)
        {
            FtpUser user = FtpUser.Validate((FtpServer)CurrentServer, _username, password);
            if (user.UserGroup.Auth == AuthType.SSL && _sslEnabled == false)    //只能通过SSL加密登录
            {
                user = null;
            }
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

                _virtualFileSystem = new VirtualFileSystem(((FtpServer)CurrentServer).Config, user.UserGroup);
                //TODO:引入新的虚拟文件系统

                if (_currentUser.IsAnonymous)
                    _performanceCounter.IncrementAnonymousUsers();
                else
                    _performanceCounter.IncrementNonAnonymousUsers();

                if (((FtpServer)CurrentServer).Config.LogInWelcome != null && !_sslEnabled)
                {
                    Write(new Response { Code = "230-", Text = "Logged In" });
                    foreach (var welcome in ((FtpServer)CurrentServer).Config.LogInWelcome)
                    {
                        Write(new Response { Code = "", Text = welcome ?? "" });
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
        /// <para>切换当前目录</para>
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response ChangeWorkingDirectory(string pathname)
        {
            _virtualFileSystem.ChangeCurrentDirectory(pathname);

            return GetResponse(FtpResponses.OK);
        }

        /// <summary>
        /// PORT Command - RFC 959 - Section 4.1.2
        /// <para>主动模式指定端口</para>
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
        /// <para>扩展主动模式指定端口（IPv6）</para>
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
        /// <para>扩展被动模式（IPv6）</para>
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
        /// <para>设置传输类型</para>
        /// </summary>
        /// <param name="typeCode"></param>
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
        /// <para>设置文件结构</para>
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
        /// <para>设置传输模式</para>
        /// </summary>
        /// <param name="mode">S:流 B:块 C:压缩</param>
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
        /// <para>下载</para>
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response Retrieve(string pathname)
        {
            FileInfo f = _virtualFileSystem.GetFile(pathname);

            if (f != null)
            {
                var state = new DataConnectionOperation { Arguments = f.FullName, Operation = RetrieveOperation };
                //建立一个异步过程
                SetupDataConnectionOperation(state);

                return GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType, "RETR" + (_protected ? withSsl : "")));
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
            if (!long.TryParse(position, out pos))
            {
                return GetResponse(FtpResponses.PARAMETER_NOT_RECOGNIZED.SetData(position));
            }
            _transPosition = pos;
            return GetResponse(FtpResponses.RESTART_FROM.SetData(pos));
        }

        /// <summary>
        /// STOR Command - RFC 959 - Section 4.1.3
        /// <para>上传</para>
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response Store(string pathname)
        {
            string pre = _transPosition == 0 ? _virtualFileSystem.GetRealPathOfFile(pathname) : _virtualFileSystem.GetRealPathOfFile(pathname, true);

            if (pre != null)
            {
                var state = new DataConnectionOperation { Arguments = pre, Operation = StoreOperation };

                SetupDataConnectionOperation(state);

                return GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType, "STOR" + (_protected ? withSsl : "")));
            }

            return GetResponse(FtpResponses.FILE_ACTION_NOT_TAKEN);
        }

        /// <summary>
        /// STOU Command - RFC 959 - Section 4.1.3
        /// <para>（禁止覆盖）上传</para>
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response StoreUnique(string pathname)
        {
            string pre = _virtualFileSystem.GetRealPathOfFile(pathname, false, new Guid().ToString());
            //string pathname = NormalizeFilename(new Guid().ToString());

            var state = new DataConnectionOperation { Arguments = pathname, Operation = StoreOperation };

            SetupDataConnectionOperation(state);

            return GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType, "STOU" + (_protected ? withSsl : "")));
        }

        /// <summary>
        /// APPE Command - RFC 959 - Section 4.1.3
        /// <para>追加</para>
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response Append(string pathname)
        {
            var f = _virtualFileSystem.GetFile(pathname, true);

            if (f != null)
            {
                var state = new DataConnectionOperation { Arguments = f.FullName, Operation = AppendOperation };

                SetupDataConnectionOperation(state);

                return
                    GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType,"APPE" + (_protected ? withSsl : "")));
            }

            return GetResponse(FtpResponses.FILE_ACTION_NOT_TAKEN);
        }

        /// <summary>
        /// RNFR - RNTO - RFC 959 - Seciton 4.1.3
        /// <para>重命名</para>
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
            if (result != FileError.None)
            {
                return GetResponse(FtpResponses.FILE_ACTION_NOT_TAKEN);
            }
            else
            {
                _virtualFileSystem.RefreshCurrentDirectory();
                return GetResponse(FtpResponses.FILE_ACTION_COMPLETE);
            }

            //return GetResponse(FtpResponses.FILE_ACTION_NOT_TAKEN);
        }

        /// <summary>
        /// DELE Command - RFC 959 - Section 4.1.3
        /// <para>删除</para>
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response Delete(string pathname)
        {
            FileError result = _virtualFileSystem.Delete(pathname);

            if (result == FileError.None)
            {
                _virtualFileSystem.RefreshCurrentDirectory();
                return GetResponse(FtpResponses.FILE_ACTION_COMPLETE);
            }

            return GetResponse(FtpResponses.FILE_NOT_FOUND);
        }

        /// <summary>
        /// RMD Command - RFC 959 - Section 4.1.3
        /// <para>删除文件夹</para>
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response RemoveDir(string pathname)
        {
            FileError result = _virtualFileSystem.Delete(pathname, true);

            if (result == FileError.None)
            {
                _virtualFileSystem.RefreshCurrentDirectory();
                return GetResponse(FtpResponses.FILE_ACTION_COMPLETE);
            }

            return GetResponse(FtpResponses.DIRECTORY_NOT_FOUND);
        }

        /// <summary>
        /// MKD Command - RFC 959 - Section 4.1.3
        /// <para>创建文件夹</para>
        /// </summary>
        /// <param name="pathname"></param>
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
            _virtualFileSystem.RefreshCurrentDirectory();
            return GetResponse(FtpResponses.FILE_ACTION_COMPLETE);

        }

        /// <summary>
        /// PWD Command - RFC 959 - Section 4.1.3
        /// <para>显示当前目录</para>
        /// </summary>
        /// <returns></returns>
        private Response PrintWorkingDirectory()
        {
            return GetResponse(FtpResponses.CURRENT_DIRECTORY.SetData(_virtualFileSystem.CurrentDirectory.VirtualPath));
        }

        /// <summary>
        /// NLST Command - RFC 959 - Section 4.1.3
        /// <para>列出当前目录下的文件名</para>
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response NameList(string pathname)
        {
            if (_dataEndpoint == null && _dataConnectionType == DataConnectionType.Active)
            {
                return GetResponse(FtpResponses.BAD_SEQUENCE);
            }

            if (_virtualFileSystem.ExistsDirectory(pathname))
            {
                var state = new DataConnectionOperation { Arguments = pathname, Operation = NameListOperation };

                SetupDataConnectionOperation(state);

                return GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType, "NLST" + (_protected ? withSsl : "")));
            }

            return GetResponse(FtpResponses.FILE_ACTION_NOT_TAKEN);
        }


        /// <summary>
        /// LIST Command - RFC 959 - Section 4.1.3
        /// <para>列出当前目录下的文件详细信息</para>
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
                return GetResponse(FtpResponses.BAD_SEQUENCE);
            }

            if (_virtualFileSystem.ExistsDirectory(pathname))
            {
                var state = new DataConnectionOperation { Arguments = pathname, Operation = ListOperation };

                SetupDataConnectionOperation(state);

                return GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType, "LIST" + (_protected ? withSsl : "")));
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
        /// PBSZ Command - RFC 2228
        /// <para>设置保护缓冲区大小，在TLS/SSL中永远是0</para>
        /// </summary>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        private Response ProtectBufferSize(string bufferSize)
        {
            if (!int.TryParse(bufferSize,out _protectBufferSize))
            {
                return GetResponse(FtpResponses.PARAMETER_NOT_RECOGNIZED);
            }
            if (_protectBufferSize!=0)
            {
                return GetResponse(FtpResponses.NOT_IMPLEMENTED_FOR_PARAMETER);
            }
            else
            {
                return new Response() { Code = "200", Text = string.Format("PBSZ={0}",_protectBufferSize) };
            }
        }

        /// <summary>
        /// PROT Command - RFC 2228
        /// <para>设置保护级别</para>
        /// </summary>
        /// <param name="level">C：无保护 P：完全保护</param>
        /// <returns></returns>
        private Response ProtectionLevel(string level)
        {
            level = level.Trim().ToUpper();
            switch (level)
            {
                case "C"://Clear,无保护
                    _protected = false;
                    break;
                case "P"://Private,同时实现机密性和完整性保护
                    _protected = true;
                    break;
                case "S":
                case "E":
                default:
                    return GetResponse(FtpResponses.NOT_IMPLEMENTED_FOR_PARAMETER);
                    break;
            }
            return new Response() { Code = "200", Text = string.Format("PROT {0} Accepted.",level) };
        }

        /// <summary>
        /// OPTS Command - RFC 2389 - Section 4
        /// <para>参数设置</para>
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
        /// <para>文件最后修改时间，用于校准时间</para>
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response FileModificationTime(string pathname)
        {
            FileInfo f = _virtualFileSystem.GetFile(pathname);

            if (f != null)
            {
                //FIXED:返回的时间为UTC时间
                return new Response { Code = "213", Text = f.LastWriteTimeUtc.ToString("yyyyMMddHHmmss.fff", CultureInfo.InvariantCulture) };

            }

            return GetResponse(FtpResponses.FILE_NOT_FOUND);
        }

        /// <summary>
        /// SIZE Command - RFC 3659 - Section 4
        /// <para>返回文件大小</para>
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response FileSize(string pathname)
        {
            FileInfo f = _virtualFileSystem.GetFile(pathname);

            if (f != null)
            {
                return new Response { Code = "213", Text = f.Length.ToString(CultureInfo.InvariantCulture) };
            }
            else if (_virtualFileSystem.GetDirectory(pathname) != null)
            {
                return new Response { Code = "213", Text = "0" };
            }
            return GetResponse(FtpResponses.FILE_NOT_FOUND);
        }

        /// <summary>
        /// MLST Command - RFC 3659 - Section 7
        /// <para>标准化显示文件属性</para>
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response MachineListTime(string pathname)
        {
            if (!_virtualFileSystem.ExistsDirectory(pathname) && !_virtualFileSystem.ExistsFile(pathname))
            {
                return GetResponse(FtpResponses.FILE_NOT_FOUND);
            }
            Write("250-Listing " + pathname);
            Write(_virtualFileSystem.MachineFileInfo(pathname));
            return new Response(){Code = "250",Text = "End"};
        }

        /// <summary>
        /// MLSD Command - RFC 3659 - Section 7
        /// <para>标准化列目录</para>
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response MachineListDirectory(string pathname)
        {
            if (_dataEndpoint == null && _dataConnectionType == DataConnectionType.Active)
            {
                return GetResponse(FtpResponses.BAD_SEQUENCE);
            }

            if (_virtualFileSystem.ExistsDirectory(pathname))
            {
                var state = new DataConnectionOperation { Arguments = pathname, Operation = MachineListOperation };

                SetupDataConnectionOperation(state);

                return GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType, "MLSD" + (_protected ? withSsl : "")));
            }

            return GetResponse(FtpResponses.FILE_ACTION_NOT_TAKEN);
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

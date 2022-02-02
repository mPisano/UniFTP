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
            //Console.WriteLine(cmd.Raw);

            Response response = null;

            FtpLogEntry logEntry = new FtpLogEntry
            {
                Date = DateTime.Now,
                CIP = ClientIP,
                CSArgs = cmd.RawArguments
            };

            ConnectionInfo.LastCommand = cmd.Code;


            //The requested command requires permission
            if (!_validCommands.Contains(cmd.Code))
            {
                response = CheckUser(); //If it is rejected, it will directly send the response of authorization authentication
            }

            //The rnto command must be followed immediately after the Rnfr rename
            if (cmd.Code != "RNTO")
            {
                _renameFrom = null;
            }
            //After Rest restarts, the transfer command must be received immediately
            if (cmd.Code != "RETR" && cmd.Code != "STOR")
            {
                _transPosition = 0;
            }

            if (response == null)
            {
                switch (cmd.Code)
                {
                    case "USER":    //username
                        response = User(cmd.Arguments.FirstOrDefault());
                        break;
                    case "PASS":    //password
                        response = Password(cmd.Arguments.FirstOrDefault());
                        logEntry.CSArgs = "******";
                        break;
                    case "CWD":     //switch directory
                        response = ChangeWorkingDirectory(cmd.RawArguments);
                        break;
                    case "CDUP":    //switch to the parent directory
                        response = ChangeWorkingDirectory("..");
                        break;
                    case "QUIT":    //quit
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
                    case "REIN":    //initialization
                        _currentUser = null;
                        _username = null;
                        _dataClient = null;
                        _currentCulture = CultureInfo.CurrentCulture;
                        _currentEncoding = Encoding.ASCII;
                        ControlStreamEncoding = Encoding.ASCII;

                        response = GetResponse(FtpResponses.SERVICE_READY);
                        break;
                    case "PORT": //Active mode setting port
                        response = Port(cmd.RawArguments);
                        logEntry.CPort = _dataEndpoint.Port.ToString(CultureInfo.InvariantCulture);
                        break;
                    case "PASV": //Enter passive mode
                        response = Passive();
                        logEntry.SPort = ((IPEndPoint)_passiveListener.LocalEndpoint).Port.ToString(CultureInfo.InvariantCulture);
                        break;
                    case "TYPE": //Set the transmission type
                        response = Type(cmd.Arguments.FirstOrDefault(), cmd.Arguments.Skip(1).FirstOrDefault());
                        break;
                    case "STRU": //set the structure
                        response = Structure(cmd.Arguments.FirstOrDefault());
                        break;
                    case "MODE": //Set the mode
                        response = Mode(cmd.Arguments.FirstOrDefault());
                        break;
                    case "RNFR":    //Rename
                        _renameFrom = cmd.RawArguments;
                        response = GetResponse(FtpResponses.RENAME_FROM);
                        break;
                    case "RNTO":    //renamed to
                        response = Rename(_renameFrom, cmd.RawArguments);
                        break;
                    case "DELE":    //Delete Files
                        response = Delete(cmd.RawArguments);
                        break;
                    case "RMD":     //delete folder
                        response = RemoveDir(cmd.RawArguments);
                        break;
                    case "MKD":     //create folder
                        response = CreateDir(cmd.RawArguments);
                        break;
                    case "PWD":     //show directory
                        response = PrintWorkingDirectory();
                        break;
                    case "RETR":    //Download file //FIXED: The file name contains spaces
                        response = Retrieve(cmd.RawArguments);
                        logEntry.Date = DateTime.Now;
                        break;
                    case "STOR":    //upload files
                        response = Store(cmd.RawArguments);
                        logEntry.Date = DateTime.Now;
                        break;
                    case "STOU":    //Upload file (do not overwrite existing file)
                        response = StoreUnique(cmd.RawArguments);
                        logEntry.Date = DateTime.Now;
                        break;
                    case "APPE":    //addition
                        response = Append(cmd.RawArguments);
                        logEntry.Date = DateTime.Now;
                        break;
                    case "LIST":    //list directory files
                        response = List(cmd.RawArguments);
                        logEntry.Date = DateTime.Now;
                        break;
                    case "SYST":    //system type
                        response = GetResponse(FtpResponses.SYSTEM);
                        break;
                    case "NOOP":    //Empty directive only to get server response
                        response = GetResponse(FtpResponses.OK);
                        break;
                    case "ACCT":    //Account required (not implemented)
                        response = Account(cmd.RawArguments);
                        break;
                    case "ALLO":    //allocate (for machines that don't need to allocate storage, it's equivalent to NOOP)
                        response = GetResponse(FtpResponses.OK);
                        break;
                    case "NLST":    //column filename
                        response = NameList(cmd.RawArguments);
                        break;
                    case "SITE":    //Server system related commands //TODO: may add search here
                        response = GetResponse(FtpResponses.NOT_IMPLEMENTED);
                        break;
                    case "STAT":
                        response = GetResponse(FtpResponses.NOT_IMPLEMENTED);
                        break;
                    case "HELP":    //Help
                        response = GetResponse(FtpResponses.NOT_IMPLEMENTED);
                        break;
                    case "SMNT":
                        response = GetResponse(FtpResponses.NOT_IMPLEMENTED);
                        break;
                    case "REST":    //Restart //ADDED: Attempt resumable upload
                        response = Restart(cmd.RawArguments);
                        break;
                    case "ABOR":    //Interrupt the transfer and close the data connection
                        response = GetResponse(FtpResponses.NOT_IMPLEMENTED);
                        break;

                    // Extensions defined by rfc 2228
                    case "AUTH":    //Permission validation <Validation method>AUTH
                        response = Auth(cmd.RawArguments);
                        break;
                    case "PBSZ":    //Set the protection buffer size, which is always 0 in ssl/tls
                        response = ProtectBufferSize(cmd.RawArguments);
                        break;
                    case "PROT":    //Protection level
                        response = ProtectionLevel(cmd.Arguments.FirstOrDefault());
                        break;

                    // Extensions defined by rfc 2389
                    case "FEAT":    //Displays the extension instructions
                        response = GetResponse(FtpResponses.FEATURES);
                        break;
                    case "OPTS":    //Parameter settings
                        response = Options(cmd.Arguments);
                        break;

                    // Extensions defined by rfc 3659
                    case "MDTM":   //Echo file modification time, typically used for timing
                        response = FileModificationTime(cmd.RawArguments); //FIXED: Accept only one parameter Be sure to use RawArg
                        break;
                    case "SIZE":    //Echo file size
                        response = FileSize(cmd.RawArguments);
                        break;
                    case "MLSD":    //Standard format column directory
                        response = MachineListDirectory(cmd.RawArguments);
                        break;
                    case "MLST":    //Standard format column file information
                        response = MachineListTime(cmd.RawArguments);
                        break;

                    // Extensions defined by rfc 2428
                    case "EPRT":    //Extended Active Mode (ipv6)
                        response = EPort(cmd.RawArguments);
                        logEntry.CPort = _dataEndpoint.Port.ToString(CultureInfo.InvariantCulture);
                        break;
                    case "EPSV":    //Extended Passive Mode (ipv6)
                        response = EPassive();
                        logEntry.SPort = ((IPEndPoint)_passiveListener.LocalEndpoint).Port.ToString(CultureInfo.InvariantCulture);
                        break;

                    // Extensions defined by rfc 2640
                    case "LANG":    //Switch the server display language
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
        /// <para>Account</para>
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
        /// <para>password</para>
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private Response Password(string password)
        {
            FtpUser user = FtpUser.Validate((FtpServer)CurrentServer, _username, password);
            if (user != null && user.UserGroup == null)
            {
                user.UserGroup = ((FtpServer)CurrentServer).UserGroups["anonymous"];
            }
            if (user != null && user.UserGroup != null && user.UserGroup.Auth == AuthType.SSL && _sslEnabled == false)    //Logins can only be made via ssl encryption
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

                ConnectionInfo.User = user.UserName;
                ConnectionInfo.UserGroup = user.GroupName;

                _virtualFileSystem = new VirtualFileSystem(((FtpServer)CurrentServer).Config, user.UserGroup);
                //TODO:Introduce a new virtual file system

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
        /// <para>Note: The RFC protocol does not explicitly specify the specific functions of the ACCT, so multiple implementations can be taken here</para>
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        private Response Account(string account)
        {
            return GetResponse(FtpResponses.NOT_IMPLEMENTED);
        }

        /// <summary>
        /// CWD Command - RFC 959 - Section 4.1.1
        /// <para>Switch the current directory</para>
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response ChangeWorkingDirectory(string pathname)
        {
            //var result = _virtualFileSystem.ChangeCurrentDirectory(pathname, _currentUser.UserGroup.AutoMakeDirectory);
            var result = _virtualFileSystem.ChangeCurrentDirectory(pathname);
            ConnectionInfo.CurrentPosition = _virtualFileSystem.CurrentDirectory.VirtualPath;
            return GetResponse(result ? FtpResponses.OK : FtpResponses.DIRECTORY_NOT_FOUND);
        }

        /// <summary>
        /// PORT Command - RFC 959 - Section 4.1.2
        /// <para>Active mode specifies the port</para>
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
        /// <para>Extended Active Mode Specified Port (ipv6)</para>
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
        /// <para>Enter passive mode (requesting the server to wait for a data connection)</para>
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
        /// <para>Extended Passive Mode (ipv6)</para>
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
        /// <para>Sets the transport type</para>
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
        /// <para>Set the file structure</para>
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private Response Structure(string structure)
        {
            switch (structure)
            {

                case "F": // file structure
                    _fileStructureType = FileStructureType.File;
                    break;
                case "R": //Record structure
                case "P": //page structure
                    return GetResponse(FtpResponses.NOT_IMPLEMENTED_FOR_PARAMETER);
                default:
                    return GetResponse(FtpResponses.PARAMETER_NOT_RECOGNIZED.SetData(structure));
            }

            return GetResponse(FtpResponses.OK);
        }

        /// <summary>
        /// MODE Command - RFC 959 - Section 4.1.2
        /// <para>Set the transfer mode</para>
        /// </summary>
        /// <param name="mode" > S: Stream B: Block C: Compressed</param>
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
        /// <para>Download</para>
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response Retrieve(string pathname)
        {
            FileInfo f = _virtualFileSystem.GetFile(pathname);

            if (f != null)
            {
                ConnectionInfo.CurrentFile = pathname;

                var state = new DataConnectionOperation { Arguments = f.FullName, Operation = RetrieveOperation };
                //Establish an asynchronous procedure
                SetupDataConnectionOperation(state);

                return GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType, "RETR" + (_protected ? withSsl : "")));
            }

            return GetResponse(FtpResponses.FILE_NOT_FOUND);
        }

        ///ADDED:Restart the transfer command
        /// <summary>
        /// REST Command - RFC 959 - Section 4.1.3
        /// <para>Restart the transfer</para>
        /// </summary>
        /// <param name="position">An unsigned integer number that represents the location where the transfer restarts</param>
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
            //BUG: Can not overwrite file
            //string pre = _transPosition == 0 ? _virtualFileSystem.GetRealPathOfFile(pathname) : _virtualFileSystem.GetRealPathOfFile(pathname, true);
            string pre = _virtualFileSystem.GetRealPathOfFile(pathname, true);

            if (pre != null)
            {
                ConnectionInfo.CurrentFile = pathname;
                var state = new DataConnectionOperation { Arguments = pre, Operation = StoreOperation };

                SetupDataConnectionOperation(state);

                return GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType, "STOR" + (_protected ? withSsl : "")));
            }

            return GetResponse(FtpResponses.FILE_ACTION_NOT_TAKEN);
        }

        /// <summary>
        /// STOU Command - RFC 959 - Section 4.1.3
        /// <para>(No overwriting) upload</para>
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response StoreUnique(string pathname)
        {
            string pre = _virtualFileSystem.GetRealPathOfFile(pathname, false, Guid.NewGuid().ToString());
            //string pathname = NormalizeFilename(new Guid().ToString());

            ConnectionInfo.CurrentFile = pathname;

            var state = new DataConnectionOperation { Arguments = pathname, Operation = StoreOperation };

            SetupDataConnectionOperation(state);

            return GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType, "STOU" + (_protected ? withSsl : "")));
        }

        /// <summary>
        /// APPE Command - RFC 959 - Section 4.1.3
        /// <para>supplement</para>
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response Append(string pathname)
        {
            var f = _virtualFileSystem.GetFile(pathname, true);

            if (f != null)
            {
                ConnectionInfo.CurrentFile = pathname;

                var state = new DataConnectionOperation { Arguments = f.FullName, Operation = AppendOperation };

                SetupDataConnectionOperation(state);

                return
                    GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType, "APPE" + (_protected ? withSsl : "")));
            }

            return GetResponse(FtpResponses.FILE_ACTION_NOT_TAKEN);
        }

        /// <summary>
        /// RNFR - RNTO - RFC 959 - Seciton 4.1.3
        /// <para>rename</para>
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
        /// <para>Delete</para>
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
        /// <para>Delete the folder</para>
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
        /// <para>Create a folder</para>
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response CreateDir(string pathname)
        {
            DirectoryInfo di;
            FileError result = _virtualFileSystem.CreateDirectory(pathname, out di);

            if (result == FileError.AlreadyExist)
            {
                return GetResponse(FtpResponses.DIRECTORY_EXISTS);
            }
            if (result == FileError.NotFound)
            {
                return GetResponse(FtpResponses.DIRECTORY_NOT_FOUND);
            }

            var name = di != null ? di.Name : VPath.GetFileName(pathname);

            _virtualFileSystem.RefreshCurrentDirectory();
            return GetResponse(FtpResponses.MAKE_DIRECTORY_SUCCESS.SetData(name));

        }

        /// <summary>
        /// PWD Command - RFC 959 - Section 4.1.3
        /// <para>Displays the current directory</para>
        /// </summary>
        /// <returns></returns>
        private Response PrintWorkingDirectory()
        {
            return GetResponse(FtpResponses.CURRENT_DIRECTORY.SetData(_virtualFileSystem.CurrentDirectory.VirtualPath));
        }

        /// <summary>
        /// NLST Command - RFC 959 - Section 4.1.3
        /// <para>Lists the file names in the current directory</para>
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
        /// <para>Lists the file details in the current directory</para>
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response List(string pathname)
        {
            //if (pathname.Trim().ToLower().StartsWith("-a") || pathname.Trim().ToLower().StartsWith("-l"))
            //{
            //    pathname = pathname.Trim().Remove(0, 2).Trim();
            //}

            if (_dataEndpoint == null && _dataConnectionType == DataConnectionType.Active)
            {
                return GetResponse(FtpResponses.BAD_SEQUENCE);
            }

            if (_virtualFileSystem.ExistsDirectory(pathname) || _virtualFileSystem.ExistsEntity(pathname))
            {
                var state = new DataConnectionOperation { Arguments = pathname, Operation = ListOperation };

                SetupDataConnectionOperation(state);

                return GetResponse(FtpResponses.OPENING_DATA_TRANSFER.SetData(_dataConnectionType, "LIST" + (_protected ? withSsl : "")));
            }

            return GetResponse(FtpResponses.FILE_ACTION_NOT_TAKEN);
        }

        /// <summary>
        /// AUTH Command - RFC 2228 - Section 3
        /// <para>Require certification</para>
        /// </summary>
        /// <param name="authMode">Authentication mode</param>
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
        /// <para>Set the protection buffer size, which is always 0 in tls/ssl </para>
        /// </summary>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        private Response ProtectBufferSize(string bufferSize)
        {
            if (!int.TryParse(bufferSize, out _protectBufferSize))
            {
                return GetResponse(FtpResponses.PARAMETER_NOT_RECOGNIZED);
            }
            if (_protectBufferSize != 0)
            {
                return GetResponse(FtpResponses.NOT_IMPLEMENTED_FOR_PARAMETER);
            }
            else
            {
                return new Response() { Code = "200", Text = string.Format("PBSZ={0}", _protectBufferSize) };
            }
        }

        /// <summary>
        /// PROT Command - RFC 2228
        /// <para>Sets the protection level</para>
        /// </summary>
        /// <param name="level">C: Unprotected P: Complete Protection</param>
        /// <returns></returns>
        private Response ProtectionLevel(string level)
        {
            level = level.Trim().ToUpper();
            switch (level)
            {
                case "C"://Clear,Unprotected
                    _protected = false;
                    break;
                case "P"://Private, which implements both confidentiality and integrity protection
                    _protected = true;
                    break;
                case "S":
                case "E":
                default:
                    return GetResponse(FtpResponses.NOT_IMPLEMENTED_FOR_PARAMETER);
                    break;
            }
            return new Response() { Code = "200", Text = string.Format("PROT {0} Accepted.", level) };
        }

        /// <summary>
        /// OPTS Command - RFC 2389 - Section 4
        /// <para>Parameter settings</para>
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
        /// <para>The last modification time of the file, which is used for calibration time </para>
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response FileModificationTime(string pathname)
        {
            FileInfo f = _virtualFileSystem.GetFile(pathname);

            if (f != null)
            {
                //FIXED:The returned time is UTC time
                return new Response { Code = "213", Text = f.LastWriteTimeUtc.ToString("yyyyMMddHHmmss.fff", CultureInfo.InvariantCulture) };

            }

            return GetResponse(FtpResponses.FILE_NOT_FOUND);
        }

        /// <summary>
        /// SIZE Command - RFC 3659 - Section 4
        /// <para>Returns the file size</para>
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response FileSize(string pathname)
        {
            FileInfo f = _virtualFileSystem.GetFile(pathname);

            if (f != null)
            {
                try
                {
                    return new Response { Code = "213", Text = f.Length.ToString(CultureInfo.InvariantCulture) };
                }
                catch (FileNotFoundException)
                {
                    return GetResponse(FtpResponses.FILE_NOT_FOUND);
                }
            }
            else if (_virtualFileSystem.GetDirectory(pathname) != null)
            {
                return new Response { Code = "213", Text = "0" };
            }
            return GetResponse(FtpResponses.FILE_NOT_FOUND);
        }

        /// <summary>
        /// MLST Command - RFC 3659 - Section 7
        /// <para>Standardize the display file properties</para>
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
            return new Response() { Code = "250", Text = "End" };
        }

        /// <summary>
        /// MLSD Command - RFC 3659 - Section 7
        /// <para>Standardize column directories</para>
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
        /// <para>Specifies the response language of the server</para>
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

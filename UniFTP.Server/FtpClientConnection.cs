using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SharpServer;
using UniFTP.Server.Performance;
using UniFTP.Server.Virtual;

namespace UniFTP.Server
{
    //Todo: Speed limit may be added

    /// <summary>
    /// Ftp connection
    /// </summary>
    public partial class FtpClientConnection : ClientConnection
    {
        /// <summary>
        /// Data flow operations
        /// </summary>
        private class DataConnectionOperation
        {
            public Func<Stream, string, Response> Operation { get; set; }
            public string Arguments { get; set; }
        }


        #region enumerate

        ///<summary>
        ///The transport type
        ///<para>Only Image and ASCII are implemented, and ASCII is deprecated due to errors when transmitting UTF8 documents</para>
        ///</summary>
        public enum TransferType
        {
            Ascii,
            Ebcdic,
            Image,
            Local,
        }
        ///<summary>
        ///Format control
        ///</summary>
        public enum FormatControlType
        {
            NonPrint,
            Telnet,
            CarriageControl,
        }
        /// <summary>
        /// Data transfer mode
        /// </summary>
        public enum DataConnectionType
        {
            /// <summary>
            /// Passive mode (recommended)
            /// </summary>
            Passive,
            ///<summary>
            ///Active mode (not recommended)
            ///</summary>
            Active,
        }
        ///<summary>
        ///File Structure (STRU)
        ///</summary>
        public enum FileStructureType
        {
            File,
            Record,
            Page,
        }

        public FtpConnectionInfo ConnectionInfo { get; private set; }

        #endregion
        VirtualFileSystem _virtualFileSystem;

        private const int BUFFER_SIZE = 8096;   //8096

        private TcpListener _passiveListener;
        private TcpClient _dataClient;
        private TransferType _connectionType = TransferType.Ascii;
        private FormatControlType _formatControlType = FormatControlType.NonPrint;
        private DataConnectionType _dataConnectionType = DataConnectionType.Active;
        private FileStructureType _fileStructureType = FileStructureType.File;

        private string _username;
        private string _password;
        private string _root;

        private IPEndPoint _dataEndpoint;
        private X509Certificate2 _cert = null;
        private GnuSslStream _sslStream;
        ///<summary>
        ///Whether Protected Mode is enabled
        ///</summary>
        private bool _protected = false;

        private bool _disposed = false;

        private bool _connected = false;

        private FtpUser _currentUser;
        private List<string> _validCommands;

        private Encoding _currentEncoding = Encoding.UTF8;//Fixed: Use ut f8 all
        private CultureInfo _currentCulture = CultureInfo.InvariantCulture;

        private ICounter _performanceCounter;
        //protected event LogEventHandler OnLog;
        private LogEventHandler OnLog;
        public FtpClientConnection()
            : base()
        {
            _validCommands = new List<string>();
            _renameFrom = null;
            ConnectionInfo = new FtpConnectionInfo();
            ConnectionInfo.ID = 0;
        }

        private void RegisterToServer()
        {
            try
            {
                ((FtpServer)CurrentServer).ConnectionInfos.Add(this.ConnectionInfo);
            }
            catch (Exception)
            {
                return;
                //throw;
            }
        }

        #region heavy load

        protected override void OnConnected()
        {
            _performanceCounter = ((FtpServer)CurrentServer).ServerPerformanceCounter;

            _performanceCounter.IncrementCurrentConnections();

            ConnectionInfo.ID = ID;//Fixed: Note the order of precedence from the next sentence

            RegisterToServer();

            OnLog = ((FtpServer)CurrentServer).SendLog;

            FtpLogEntry logEntry = new FtpLogEntry()
            {
                Date = DateTime.Now,
                Info = LogInfo.ConnectionEstablished.ToString()
            };

            OnLog(logEntry);

            ConnectionInfo.IP = ClientIP;

            _connected = true;

            if (((FtpServer)CurrentServer).Config.Welcome != null)
            {
                Write(new Response { Code = "220-", Text = "Connected" });
                foreach (var welcome in ((FtpServer)CurrentServer).Config.Welcome)
                {
                    Write(new Response { Code = "", Text = welcome });
                }
            }

            Write(GetResponse(FtpResponses.SERVICE_READY));
            //Fixed: Ssl instructions can be executed before logging in
            _validCommands.AddRange(new string[] { "AUTH", "USER", "PASS", "ACCT", "QUIT", "HELP", "NOOP", "PBSZ", "PROT" });

            _dataClient = new TcpClient();

            Read();
        }

        protected override void OnCommandComplete(Command cmd)
        {
            if (cmd.Code == "AUTH")
            {
                try
                {
                    //Write(GetResponse(FtpResponses.ENABLING_TLS));
                    //MARK: Establish a TLS connection
                    _cert = ((FtpServer)CurrentServer).ServerCertificate;

                    _sslStream = new GnuSslStream(ControlStream);

                    _sslStream.ReadTimeout = 5000;
                    _sslStream.WriteTimeout = 5000;

                    _sslStream.AuthenticateAsServer(_cert, false, SslProtocols.Tls, true);

                    _sslEnabled = true;
                }
                catch (AuthenticationException exception)
                {
                    _sslEnabled = false;
                    //throw;
                }
                catch (ArgumentNullException exception)
                {
                    _sslEnabled = false;
                }
            }

            _performanceCounter.IncrementCommandsExecuted();
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
                            _performanceCounter.DecrementAnonymousUsers();
                        else
                            _performanceCounter.DecrementNonAnonymousUsers();

                    if (_connected)
                        _performanceCounter.DecrementCurrentConnections();

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
                FtpLogEntry logEntry = new FtpLogEntry()
                {
                    Date = DateTime.Now,
                    Info = LogInfo.ConnectionTerminated.ToString()
                };
                OnLog(logEntry);

                var serverConnInfos = ((FtpServer)CurrentServer).ConnectionInfos;
                if (serverConnInfos.Contains(ConnectionInfo))
                {
                    serverConnInfos.Remove(ConnectionInfo);
                }
                serverConnInfos.RemoveAll(t => t.ID == this.ID);

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

        ///<summary>
        ///Detects whether the directory is valid (must be below the root directory)
        ///</summary>
        ///<param name="path"></param>
        ///<returns></returns>
        private bool IsPathValid(string path)
        {
            return path.StartsWith(_root, StringComparison.OrdinalIgnoreCase);
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
                //FIXED: ANSI mode transmission is problematic, especially in non-English-speaking countries, where non-ANSI-encoded documents are bound to lead to confusion, so binary is used uniformly
                //return CopyStream(input, limitedStream, BUFFER_SIZE, Encoding.UTF8 , perfAction);
                //return CopyStream(input, limitedStream, BUFFER_SIZE, Encoding.Default, perfAction);
                return CopyStream(input, limitedStream, BUFFER_SIZE, perfAction);
            }
        }

        private Response GetResponse(Response response)
        {
            return response.SetCulture(_currentCulture);
        }


        #region DataConnection Operations

        private void HandleAsyncResult(IAsyncResult result)
        {
            if (_dataConnectionType == DataConnectionType.Active)
            {
                if (_dataClient == null)
                {
                    return;
                }
                try
                {
                    _dataClient.EndConnect(result);
                    /*MARK: Asynchronous operation Tip
* If the asynchronous operation represented by the IAsyncResult object has not completed when the End operation name OperationName is called,
* The End operation name OperationName will block the calling thread until the asynchronous operation completes.
*/
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }

            }
            else
            {
                try
                {
                    _dataClient = _passiveListener.EndAcceptTcpClient(result);
                }
                catch (Exception)
                {
                    //throw;
                }
            }
        }

        private void SetupDataConnectionOperation(DataConnectionOperation state)
        {
            if (_dataConnectionType == DataConnectionType.Active)
            {
                _dataClient = new TcpClient(_dataEndpoint.AddressFamily);
                //Start an asynchronous connection, call the do data connection operation when the connection is successful, and pass the state and connection information as i as an async result parameter
                _dataClient.BeginConnect(_dataEndpoint.Address, _dataEndpoint.Port, DoDataConnectionOperation, state);
            }
            else
            {
                _passiveListener.BeginAcceptTcpClient(DoDataConnectionOperation, state);
            }
        }

        private void DoDataConnectionOperation(IAsyncResult result)
        {
            _performanceCounter.IncrementTotalConnectionAttempts();
            _performanceCounter.IncrementCurrentConnections();
            //Called asynchronously by begin connect, end connect is required, and end connect needs to use [corresponding] i async result as an argument
            HandleAsyncResult(result);
            //Take out the data connection operation structure that we really need, and contain the methods and parameters to be executed this time
            DataConnectionOperation op = result.AsyncState as DataConnectionOperation;

            Response response;

            try
            {
                if (_dataClient == null)
                {
                    throw new SocketException();
                }
                //Through the above asynchronous procedure, the data client is already connected at this time
                if (_protected)
                {
                    using (GnuSslStream dataStream = new GnuSslStream(_dataClient.GetStream(), false))
                    {
                        try
                        {
                            dataStream.AuthenticateAsServer(_cert, false, SslProtocols.Tls, true);
                            response = op.Operation(dataStream, op.Arguments);
                        }
                        catch (Exception)
                        {
                            response = GetResponse(FtpResponses.UNABLE_TO_OPEN_DATA_CONNECTION);
                            //throw;
                        }
                        dataStream.Close();
                        //response = op.Operation(dataStream, op.Arguments);
                    }
                }
                else
                {
                    using (NetworkStream dataStream = _dataClient.GetStream())
                    {
                        response = op.Operation(dataStream, op.Arguments);
                    }
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

            _performanceCounter.DecrementCurrentConnections();

            if (_dataConnectionType == DataConnectionType.Passive)
                PassiveListenerPool.FreeListener(_passiveListener);

            Write(response.ToString());
        }

        ///<summary>
        ///Download operation
        ///</summary>
        ///<param name="dataStream"></param>
        ///<param name="pathname"></param>
        ///<returns></returns>
        private Response RetrieveOperation(Stream dataStream, string pathname)
        {
            long bytes = 0;
            FtpLogEntry logEntry = new FtpLogEntry
            {
                Date = DateTime.Now,
                CIP = ClientIP,
                CSMethod = "RETR",
                CSUsername = _username,
                SCStatus = "226",
            };

            using (FileStream fs = new FileStream(pathname, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(_transPosition, SeekOrigin.Begin);
                //Because it is called asynchronously, it is not blocked
                bytes = CopyStream(fs, dataStream, _performanceCounter.IncrementBytesSent);
            }

            logEntry.SCBytes = bytes.ToString(CultureInfo.InvariantCulture);

            _log.Info(logEntry);
            OnLog(logEntry);

            _performanceCounter.IncrementFilesSent();

            _virtualFileSystem.RefreshCurrentDirectory();

            return GetResponse(FtpResponses.TRANSFER_SUCCESSFUL);
        }

        ///<summary>
        ///Upload operation
        ///</summary>
        ///<param name="dataStream"></param>
        ///<param name="pathname"></param>
        ///<returns></returns>
        private Response StoreOperation(Stream dataStream, string pathname)
        {
            long bytes = 0;

            FtpLogEntry logEntry = new FtpLogEntry
            {
                Date = DateTime.Now,
                CIP = ClientIP,
                CSMethod = "STOR",
                CSUsername = _username,
                SCStatus = "226",
            };
            if (VPath.ContainsInvalidPathChars(pathname))
            {
                pathname = VPath.RemoveInvalidPathChars(pathname);
            }
            using (FileStream fs = new FileStream(pathname, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, BUFFER_SIZE, FileOptions.SequentialScan))
            {
                //if (_lastCommand.Code == "REST")
                //{
                //    fs.Seek(_transPosition, SeekOrigin.Begin);
                //}
                fs.Seek(_transPosition, SeekOrigin.Begin);
                bytes = CopyStream(dataStream, fs, _performanceCounter.IncrementBytesReceived);
            }

            logEntry.CSBytes = bytes.ToString(CultureInfo.InvariantCulture);

            _log.Info(logEntry);
            OnLog(logEntry);

            _performanceCounter.IncrementFilesReceived();

            _virtualFileSystem.RefreshCurrentDirectory();

            return GetResponse(FtpResponses.TRANSFER_SUCCESSFUL);
        }

        ///<summary>
        ///File append operations
        ///</summary>
        ///<param name="dataStream"></param>
        ///<param name="pathname"></param>
        ///<returns></returns>
        private Response AppendOperation(Stream dataStream, string pathname)
        {
            long bytes = 0;

            FtpLogEntry logEntry = new FtpLogEntry
            {
                Date = DateTime.Now,
                CIP = ClientIP,
                CSMethod = "APPE",
                CSUsername = _username,
                SCStatus = "226",
                //CSBytes = bytes.ToString(CultureInfo.InvariantCulture)
            };

            using (FileStream fs = new FileStream(pathname, FileMode.Append, FileAccess.Write, FileShare.Read, BUFFER_SIZE, FileOptions.SequentialScan))
            {
                bytes = CopyStream(dataStream, fs, _performanceCounter.IncrementBytesReceived);
            }


            logEntry.CSBytes = bytes.ToString(CultureInfo.InvariantCulture);

            _log.Info(logEntry);
            OnLog(logEntry);

            _performanceCounter.IncrementFilesReceived();

            _virtualFileSystem.RefreshCurrentDirectory();

            return GetResponse(FtpResponses.TRANSFER_SUCCESSFUL);
        }

        ///<summary>
        ///Column directory operations
        ///</summary>
        ///<param name="dataStream"></param>
        ///<param name="pathname"></param>
        ///<returns></returns>
        private Response ListOperation(Stream dataStream, string pathname)
        {
            DateTime now = DateTime.Now;

            FtpLogEntry logEntry = new FtpLogEntry
            {
                Date = now,
                CIP = ClientIP,
                CSMethod = "LIST",
                CSUsername = _username,
                SCStatus = "226"
            };

            StreamWriter dataWriter = new StreamWriter(dataStream, _currentEncoding);

            var dirList = _virtualFileSystem.ListFiles(pathname);
            dataWriter.WriteLine(dirList.Count);    //Mark: The first row represents the number
            foreach (var dir in dirList)
            {
                dataWriter.WriteLine(dir);
                dataWriter.Flush();
            }

            _log.Info(logEntry);
            OnLog(logEntry);
            return GetResponse(FtpResponses.TRANSFER_SUCCESSFUL);
        }

        ///<summary>
        ///Standardize column directory operations
        ///</summary>
        ///<param name="dataStream"></param>
        ///<param name="pathname"></param>
        ///<returns></returns>
        private Response MachineListOperation(Stream dataStream, string pathname)
        {
            DateTime now = DateTime.Now;

            FtpLogEntry logEntry = new FtpLogEntry
            {
                Date = now,
                CIP = ClientIP,
                CSMethod = "MLSD",
                CSUsername = _username,
                SCStatus = "226"
            };

            StreamWriter dataWriter = new StreamWriter(dataStream, _currentEncoding);
            dataWriter.WriteLine();
            var dirList = _virtualFileSystem.MachineListFiles(pathname);

            foreach (var dir in dirList)
            {
                dataWriter.WriteLine(dir);
                //dataWriter.Flush();
            }
            dataWriter.WriteLine();
            dataWriter.Flush();

            _log.Info(logEntry);
            OnLog(logEntry);
            return GetResponse(FtpResponses.TRANSFER_SUCCESSFUL);
        }

        ///<summary>
        ///Column file name operation
        ///</summary>
        ///<param name="dataStream"></param>
        ///<param name="pathname"></param>
        ///<returns></returns>
        private Response NameListOperation(Stream dataStream, string pathname)
        {
            FtpLogEntry logEntry = new FtpLogEntry
            {
                Date = DateTime.Now,
                CIP = ClientIP,
                CSMethod = "NLST",
                CSUsername = _username,
                SCStatus = "226"
            };

            StreamWriter dataWriter = new StreamWriter(dataStream, _currentEncoding);

            var nameList = _virtualFileSystem.ListFileNames(pathname);
            foreach (var name in nameList)
            {
                dataWriter.WriteLine(name);
                dataWriter.Flush();
            }

            _log.Info(logEntry);
            OnLog(logEntry);
            return GetResponse(FtpResponses.TRANSFER_SUCCESSFUL);
        }

        #endregion
    }
}

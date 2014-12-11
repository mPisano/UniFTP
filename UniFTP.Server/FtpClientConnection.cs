using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SharpServer;
using UniFTP.Server.Virtual;

namespace UniFTP.Server
{
    public partial class FtpClientConnection : ClientConnection
    {
        /// <summary>
        /// 数据流操作
        /// </summary>
        private class DataConnectionOperation
        {
            public Func<NetworkStream, string, Response> Operation { get; set; }
            public string Arguments { get; set; }
        }

        #region 枚举

        /// <summary>
        /// 传输类型
        /// <para>只实现了Image和ASCII，且ASCII由于传输UTF8文档时会出错所以弃用</para>
        /// </summary>
        public enum TransferType
        {
            Ascii,
            Ebcdic,
            Image,
            Local,
        }
        /// <summary>
        /// 格式控制
        /// </summary>
        public enum FormatControlType
        {
            NonPrint,
            Telnet,
            CarriageControl,
        }
        /// <summary>
        /// 数据传输模式
        /// </summary>
        public enum DataConnectionType
        {
            /// <summary>
            /// 被动模式(推荐)
            /// </summary>
            Passive,
            /// <summary>
            /// 主动模式(不推荐)
            /// </summary>
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
        //private string _currentDirectory;
        private IPEndPoint _dataEndpoint;
        private X509Certificate _cert = null;
        private SslStream _sslStream;

        private bool _disposed = false;

        private bool _connected = false;

        private FtpUser _currentUser;
        private List<string> _validCommands;

        private Encoding _currentEncoding = Encoding.UTF8;
        private CultureInfo _currentCulture = CultureInfo.InvariantCulture;

        private FtpPerformanceCounter _performanceCounter;

        public FtpClientConnection()
            : base()
        {
            _validCommands = new List<string>();
            _renameFrom = null;
        }

        #region 重载


        protected override void OnConnected()
        {
            _performanceCounter = ((FtpServer)CurrentServer).ServerPerformanceCounter;

            _performanceCounter.IncrementCurrentConnections();

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

        /// <summary>
        /// 检测目录是否有效（必须在根目录以下）
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsPathValid(string path)
        {
            return path.StartsWith(_root, StringComparison.OrdinalIgnoreCase);
        }

        //private string NormalizeFilename(string path)
        //{
        //    if (path == null)
        //    {
        //        path = string.Empty;
        //    }

        //    //if (_invalidPathChars.IsMatch(path))
        //    //{
        //    //    return null;
        //    //}
        //    try
        //    {
        //        if (path == "/")
        //        {
        //            return _root;
        //        }
        //        else if (path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
        //        {
        //            path = new FileInfo(Path.Combine(_root, path.Substring(1))).FullName;
        //        }
        //        else
        //        {
        //            path = new FileInfo(Path.Combine(_currentDirectory, path)).FullName;
        //        }
        //    }
        //    catch (Exception)
        //    {

        //        return null;
        //    }


        //    return IsPathValid(path) ? path : null;
        //}

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
                //FIXED:ANSI模式传输是有问题的，尤其是在非英语国家，非ANSI编码的文档势必导致混乱，因此统一用二进制
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
                    _dataClient.EndConnect(result); //BUG:
                }
                catch (Exception e)
                {
                    
                }

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
            _performanceCounter.IncrementTotalConnectionAttempts();
            _performanceCounter.IncrementCurrentConnections();

            HandleAsyncResult(result);

            DataConnectionOperation op = result.AsyncState as DataConnectionOperation;

            Response response;

            try
            {
                if (_dataClient == null)
                {
                    throw new SocketException();
                }
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

            _performanceCounter.DecrementCurrentConnections();

            if (_dataConnectionType == DataConnectionType.Passive)
                PassiveListenerPool.FreeListener(_passiveListener);

            Write(response.ToString());
        }

        private Response RetrieveOperation(NetworkStream dataStream, string pathname)
        {
            using (FileStream fs = new FileStream(pathname, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(_transPosition, SeekOrigin.Begin);

                CopyStream(fs, dataStream, _performanceCounter.IncrementBytesSent);
            }

            _performanceCounter.IncrementFilesSent();

            return GetResponse(FtpResponses.TRANSFER_SUCCESSFUL);
        }

        private Response StoreOperation(NetworkStream dataStream, string pathname)
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

            _performanceCounter.IncrementFilesReceived();

            return GetResponse(FtpResponses.TRANSFER_SUCCESSFUL);
        }

        private Response AppendOperation(NetworkStream dataStream, string pathname)
        {
            long bytes = 0;

            FtpLogEntry logEntry = new FtpLogEntry
            {
                Date = DateTime.Now,
                CIP = ClientIP,
                CSMethod = "APPE",
                CSUsername = _username,
                SCStatus = "226",
                CSBytes = bytes.ToString(CultureInfo.InvariantCulture)
            };

            using (FileStream fs = new FileStream(pathname, FileMode.Append, FileAccess.Write, FileShare.Read, BUFFER_SIZE, FileOptions.SequentialScan))
            {
                bytes = CopyStream(dataStream, fs, _performanceCounter.IncrementBytesReceived);
            }


            logEntry.CSBytes = bytes.ToString(CultureInfo.InvariantCulture);

            _log.Info(logEntry);

            _performanceCounter.IncrementFilesReceived();

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
            dataWriter.WriteLine(dirList.Count);    //MARK:第一行表示数目
            foreach (var dir in dirList)
            {
                dataWriter.WriteLine(dir);
                dataWriter.Flush();
            }
            //dataWriter.WriteLine();
            #region Abandoned
            //IEnumerable<string> directories = Directory.EnumerateDirectories(pathname);

            //foreach (string dir in directories)
            //{
            //    DateTime editDate = Directory.GetLastWriteTime(dir);

            //    string date = editDate < now.Subtract(TimeSpan.FromDays(180)) ?
            //        editDate.ToString("MMM dd  yyyy", CultureInfo.InvariantCulture) :
            //        editDate.ToString("MMM dd HH:mm", CultureInfo.InvariantCulture);
            //    //参考格式
            //    //drwxrwxrwx   1 user     group           0 Nov 27 00:13 上传
            //    dataWriter.Write("drwxrwxrwx   1 user     group           0 ");
            //    dataWriter.Write(date);
            //    dataWriter.Write(' ');
            //    dataWriter.WriteLine(Path.GetFileName(dir));
            //    dataWriter.Flush();
            //    Console.Write("drwxrwxrwx   1 2003     2003         4096 ");
            //    Console.Write(date);
            //    Console.Write(' ');
            //    Console.WriteLine(Path.GetFileName(dir));
            //}

            //IEnumerable<string> files = Directory.EnumerateFiles(pathname);

            //foreach (string file in files)
            //{
            //    FileInfo f = new FileInfo(file);

            //    string date = f.LastWriteTime < now.Subtract(TimeSpan.FromDays(180)) ?
            //        f.LastWriteTime.ToString("MMM dd  yyyy", CultureInfo.InvariantCulture) :
            //        f.LastWriteTime.ToString("MMM dd HH:mm", CultureInfo.InvariantCulture);

            //    //参考格式
            //    //-rw-rw-rw-   1 user     group     2339366 Aug 31  2013 音乐全表20130831.TXT
            //    //权限 所有者 组
            //    dataWriter.Write("-rw-r--r--   2 2003     2003     ");

            //    string length = f.Length.ToString(CultureInfo.InvariantCulture);

            //    if (length.Length < 8)
            //    {
            //        for (int i = 0; i < 8 - length.Length; i++)
            //        {
            //            dataWriter.Write(' ');
            //        }
            //    }

            //    dataWriter.Write(length);
            //    dataWriter.Write(' ');
            //    dataWriter.Write(date);
            //    dataWriter.Write(' ');
            //    dataWriter.WriteLine(f.Name);

            //    dataWriter.Flush();

            //    Console.Write("-rw-r--r--   2 2003     2003     ");
            //    if (length.Length < 8)
            //    {
            //        for (int i = 0; i < 8 - length.Length; i++)
            //        {
            //            Console.Write(' ');
            //        }
            //    }
            //    Console.Write(length);
            //    Console.Write(' ');
            //    Console.Write(date);
            //    Console.Write(' ');
            //    Console.WriteLine(f.Name);

            //    f = null;
            //}
            #endregion


            _log.Info(logEntry);

            return GetResponse(FtpResponses.TRANSFER_SUCCESSFUL);
        }

        /// <summary>
        /// 列文件名操作
        /// </summary>
        /// <param name="dataStream"></param>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private Response NameListOperation(NetworkStream dataStream, string pathname)
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

            //IEnumerable<string> dirs = Directory.EnumerateDirectories(pathname);

            //IEnumerable<string> files = Directory.EnumerateFiles(pathname);

            //foreach (string dir in dirs)
            //{
            //    dataWriter.WriteLine(Path.GetFileName(dir));
            //    dataWriter.Flush();
            //}

            //foreach (string file in files)
            //{
            //    dataWriter.WriteLine(Path.GetFileName(file));
            //    dataWriter.Flush();
            //}



            _log.Info(logEntry);

            return GetResponse(FtpResponses.TRANSFER_SUCCESSFUL);
        }

        #endregion
    }
}

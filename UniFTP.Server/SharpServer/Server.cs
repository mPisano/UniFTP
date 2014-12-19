using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using log4net;

namespace SharpServer
{
    /// <summary>
    /// 通用Server
    /// </summary>
    /// <typeparam name="T">特定客户端连接方式</typeparam>
    public class Server<T> : IDisposable where T : ClientConnection, new()
    {
        private static readonly object _listLock = new object();

        private ILog _log = LogManager.GetLogger(typeof(Server<T>));

        private List<T> _state;

        private List<TcpListener> _listeners;

        private bool _disposed = false;
        private bool _disposing = false;
        private bool _listening = false;
        private List<IPEndPoint> _localEndPoints;
        private string _logHeader;

        public Server(int port, string logHeader = null)
            : this(IPAddress.Any, port, logHeader)
        {
        }

        public Server(IPAddress ipAddress, int port, string logHeader = null)
            : this(new IPEndPoint[] { new IPEndPoint(ipAddress, port) }, logHeader)
        {
        }

        public Server(IPEndPoint[] localEndPoints, string logHeader = null)
        {
            _localEndPoints = new List<IPEndPoint>(localEndPoints);
            _logHeader = logHeader??"";
        }

        public void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException("AsyncServer");

            _log.Info("#" + _logHeader);
            _log.Info("date time c-ip c-port cs-username cs-method cs-args sc-status sc-bytes cs-bytes s-name s-port");
            _state = new List<T>();
            _listeners = new List<TcpListener>();

            foreach (var localEndPoint in _localEndPoints)
            {
                TcpListener listener = new TcpListener(localEndPoint);

                try
                {
                    listener.Start();
                }
                catch (SocketException ex)
                {
                    Dispose();

                    throw new Exception("The current local end point is currently in use. Please specify another IP or port to listen on.");
                }
                //开始异步等待连接
                listener.BeginAcceptTcpClient(HandleAcceptTcpClient, listener);

                _listeners.Add(listener);
            }

            _listening = true;

            OnStart();
        }

        public void Stop()
        {
            _log.Info("# Stopping Server");
            _listening = false;

            foreach (var listener in _listeners)
            {
                listener.Stop();
            }

            _listeners.Clear();

            OnStop();
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnStop()
        {
        }

        protected virtual void OnConnectAttempt()
        {
        }

        private void HandleAcceptTcpClient(IAsyncResult result)
        {
            //一个客户成功连接
            OnConnectAttempt();

            TcpListener listener = result.AsyncState as TcpListener;

            if (_listening)
            {
                TcpClient client;
                try
                {
                    //立即再次新开监听
                    listener.BeginAcceptTcpClient(HandleAcceptTcpClient, listener);
                    //对本次的连接结果创建TcpClient处理
                    client = listener.EndAcceptTcpClient(result);

                    var connection = new T { CurrentServer = this };

                    connection.Disposed += new EventHandler<EventArgs>(AsyncClientConnection_Disposed);

                    connection.HandleClient(client);

                    lock (_listLock)
                        _state.Add(connection);
                }
                catch (SocketException e)
                {
                    //throw;
                }
            }
        }

        private void AsyncClientConnection_Disposed(object sender, EventArgs e)
        {
            // Prevent removing if we are disposing of this object. The list will be cleaned up in Dispose(bool).
            if (!_disposing)
            {
                T connection = (T)sender;

                lock (_listLock)
                    _state.Remove(connection);
            }
        }

        public void Dispose()
        {
            //脱离GC 手动回收
            _disposing = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _disposing = true;

            if (!_disposed)
            {
                if (disposing)
                {
                    Stop();

                    lock (_listLock)
                    {
                        foreach (var connection in _state)
                        {
                            if (connection != null)
                                connection.Dispose();
                        }

                        _state = null;
                    }
                }
            }

            _disposed = true;
        }
    }
}
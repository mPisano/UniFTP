﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SharpServer
{
    public abstract class ClientConnection : ClientConnectionBase, IDisposable
    {
        public event EventHandler<EventArgs> Disposed;

        ///<summary>
        ///The owning server
        ///</summary>
        public object CurrentServer { get; set; } //MARK: Try to establish a connection between Connection and Server
        ///<summary>
        ///numbering
        ///</summary>
        public ulong ID = 0;
        private bool _disposed = false;

        #region Private Fields

        private byte[] _buffer = new byte[128];
        private StringBuilder _commandBuffer = new StringBuilder();
        private Encoding _controlStreamEncoding = Encoding.UTF8;//Fixed: Use ut f8 all
        private string _expectedTerminator = "\r\n";

        #endregion

        ///<summary>
        ///Control flow encoding
        ///</summary>
        protected Encoding ControlStreamEncoding
        {
            get { return _controlStreamEncoding; }
            set { _controlStreamEncoding = value; }
        }

        ///<summary>
        ///Expected message ending. In FTP is the &lt; CRLF &gt;. In SMTP is &lt; CRLF &gt;.
        ///When sending the actual email message in SMTP it ends with: &lt; CRLF&gt;.&lt; CRLF &gt;
        ///</summary>
        protected string ExpectedTerminator
        {
            get { return _expectedTerminator; }
            set { _expectedTerminator = value; }
        }

        ///<summary>
        ///Start an asynchronous read from the control connection stream.
        ///</summary>
        protected virtual void Read()
        {
            Read(ControlStream);
        }

        /// <summary>
        /// Begins an asynchronous read from the provided <paramref name="stream"/>.
        /// <para>From the provided stream<paramref name="stream"/>Start an asynchronous read.</para>
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        protected virtual void Read(Stream stream)
        {
            if (_disposed || !stream.CanRead)
            {
                Dispose();
                return;
            }

            try
            {
                stream.BeginRead(_buffer, 0, _buffer.Length, ReadCallback, stream);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                Dispose();
            }
        }

        /// <summary>
        /// Asynchronously writes <paramref name="content"/> to the control connection stream.
        /// </summary>
        /// <param name="content">The text to write.</param>
        protected override void Write(string content)
        {
            Write(ControlStream, content);
        }

        /// <summary>
        /// Asynchronously writes <paramref name="content"/> to the provided <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="content">The text to write.</param>
        protected virtual void Write(Stream stream, string content)
        {
            if (_disposed || !stream.CanWrite)
            {
                Dispose();
                return;
            }

            _log.Debug(content);

            try
            {
                byte[] response = ControlStreamEncoding.GetBytes(string.Concat(content, "\r\n"));

                stream.BeginWrite(response, 0, response.Length, WriteCallback, stream);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                Dispose();
            }
        }

        ///<summary>
        ///Initialize The Connect, dedicated to serving a successfully connected TcpClient
        ///<para>Sets up the class to handle the communication to the given TcpClient.</para>
        ///</summary>
        ///<param name="client">The TcpClient to communicate with.</param>
        public override void HandleClient(object obj)
        {
            TcpClient client = obj as TcpClient;

            ControlClient = client;

            RemoteEndPoint = (IPEndPoint)ControlClient.Client.RemoteEndPoint;

            ClientIP = RemoteEndPoint.Address.ToString();

            ControlStream = ControlClient.GetStream();

            OnConnected();
        }

        protected virtual void OnDisposed()
        {
            if (Disposed != null)
            {
                Disposed(this, EventArgs.Empty);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (ControlClient != null)
                    {
                        ControlClient.Close();
                    }

                    if (ControlStream != null)
                    {
                        ControlStream.Close();
                    }
                }
            }

            _disposed = true;
            OnDisposed();
        }

        // TODO: Make CopyStream async.
        /*
                protected virtual long CopyStream(Stream input, Stream output, int bufferSize)
                {
                    byte[] buffer = new byte[bufferSize];
                    int count = 0;
                    long total = 0;

                    while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, count);
                        total += count;
                    }

                    return total;
                }
        */
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region Private Methods

        private void WriteCallback(IAsyncResult result)
        {
            if (result == null)
            {
                Dispose();
                return;
            }

            Stream stream = (Stream)result.AsyncState;

            if (_disposed || stream == null || !stream.CanWrite)
            {
                Dispose();
                return;
            }

            try
            {
                stream.EndWrite(result);
            }
            catch (IOException ex)
            {
                _log.Error(ex);
                Dispose();
            }
        }

        private void ReadCallback(IAsyncResult result)
        {
            if (result == null)
            {
                Dispose();
                return;
            }

            Stream stream = result.AsyncState as Stream;

            if (_disposed || stream == null || !stream.CanRead)
            {
                Dispose();
                return;
            }

            int bytesRead = 0;

            try
            {
                bytesRead = stream.EndRead(result);
            }
            catch (IOException ex)
            {
                _log.Error(ex);
            }

            // End read returns 0 bytes if the socket closed...
            if (bytesRead == 0)
            {
                Dispose();
                return;
            }

            string line = ControlStreamEncoding.GetString(_buffer, 0, bytesRead);

            _commandBuffer.Append(line);

            _log.Debug(line);

            //The message does not end with a predetermined terminator, and there may be information after it, continue reading (_commandBuffer command buffer reserved)
            //We don't have the full message yet, so keep reading.
            if (!_commandBuffer.EndsWith(ExpectedTerminator))
            {
                Read();
                return;
            }

            string command = _commandBuffer.ToString().Trim();

            _log.Debug(command);

            Command cmd = ParseCommandLine(command);

            //Clear the command buffer (_commandBuffer) in preparation for the next read
            //Clear the command buffer, so we can keep listening for more commands.
            _commandBuffer.Clear();
            command = null;

            Response r = HandleCommand(cmd);

            if (ControlClient != null && ControlClient.Connected)
            {
                if (r != null)  //Fixed: Can not respond
                {
                    Write(r);
                }

                if (r.ShouldQuit)
                {
                    Dispose();
                    return;
                }

                OnCommandComplete(cmd);

                cmd = null;
                r = null;

                Read();
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Resources;
using System.Text;
using log4net;

namespace SharpServer
{
    public abstract class ClientConnectionBase
    {
        protected ILog _log = LogManager.GetLogger(typeof(ClientConnectionBase));

        protected TcpClient ControlClient { get; set; }
        ///<summary>
        ///control flow
        ///</summary>
        protected NetworkStream ControlStream { get; set; }
        ///<summary>
        ///Remote address (IP: port number)
        ///</summary>
        protected IPEndPoint RemoteEndPoint { get; set; }
        ///<summary>
        ///Client IP
        ///</summary>
        protected string ClientIP { get; set; }

        protected abstract void Write(string content);
        ///<summary>
        ///Process the command
        ///</summary>
        ///<param name="cmd"></param>
        ///<returns></returns>
        protected abstract Response HandleCommand(Command cmd);

        protected virtual Response HandleCommand(string cmd)
        {
            return HandleCommand(ParseCommandLine(cmd));
        }

        public abstract void HandleClient(object obj);

        protected virtual void Write(Response response)
        {
            Write(response.ToString());
        }

        ///<summary>
        ///Parse the command
        ///</summary>
        ///<param name="line"></param>
        ///<returns></returns>
        protected virtual Command ParseCommandLine(string line)
        {
            Command c = new Command();
            c.Raw = line;

            string[] command = line.Split(' ');

            string cmd = command[0].ToUpperInvariant();

            c.Arguments = new List<string>(command.Skip(1));
            c.RawArguments = string.Join(" ", command.Skip(1));

            c.Code = cmd;

            return c;
        }

        protected virtual void OnConnected()
        {
        }

        protected virtual void OnCommandComplete(Command cmd)
        {
        }

        protected virtual long CopyStream(Stream input, Stream output, int bufferSize, Action<int> performanceCounterAction)
        {
            byte[] buffer = new byte[bufferSize];
            int count = 0;
            long total = 0;
            //BUG? May cause WSACancelBlockingCall
            while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, count);
                total += count;
                performanceCounterAction(count);
            }

            return total;
        }

        ///<summary>
        ///streaming
        ///</summary>
        ///<param name="input"></param>
        ///<param name="output"></param>
        ///<param name="bufferSize"></param>
        ///<param name="encoding">Encoding</param>
        ///<param name="performanceCounterAction">Counter Action</param>
        ///<returns></returns>
        protected virtual long CopyStream(Stream input, Stream output, int bufferSize, Encoding encoding, Action<int> performanceCounterAction)
        {
            char[] buffer = new char[bufferSize];
            int count = 0;
            long total = 0;

            using (StreamReader rdr = new StreamReader(input))
            {
                using (StreamWriter wtr = new StreamWriter(output, encoding))
                {
                    while ((count = rdr.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        wtr.Write(buffer, 0, count);
                        total += count;
                        performanceCounterAction(count);
                    }
                }
            }

            return total;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UniFTP.Server
{
    ///<summary>
    ///Passive mode Listener connection pooling
    ///</summary>
    public static class PassiveListenerPool
    {
        private static readonly object ListLock = new object();
        private static Dictionary<AutoResetEvent, TcpListener> _listeners = new Dictionary<AutoResetEvent, TcpListener>();

        ///<summary>
        ///Get a TCP Listener
        ///</summary>
        ///<param name="ip"></param>
        ///<returns></returns>
        public static TcpListener GetListener(IPAddress ip)
        {
            TcpListener listener = null;

            lock (ListLock)
            {
                //Find a lister in the listener pool that holds the semaphore (which has been released) and corresponds to the ip
                listener = _listeners.FirstOrDefault(p => p.Key.WaitOne(TimeSpan.FromMilliseconds(10)) && ((IPEndPoint)p.Value.LocalEndpoint).Address.Equals(ip)).Value;

                if (listener != null) return listener;
                //Didn't find such a lister, build a new one
                AutoResetEvent listenerLock = new AutoResetEvent(false);

                listener = new TcpListener(ip, 0);

                _listeners.Add(listenerLock, listener);
            }

            return listener;
        }

        ///<summary>
        ///Release a TCP Listener
        ///</summary>
        ///<param name="listener"></param>
        public static void FreeListener(TcpListener listener)
        {
            AutoResetEvent sync = _listeners.SingleOrDefault(p => p.Value == listener).Key;

            sync.Set();
        }

        ///<summary>
        ///Release all TCP Listeners
        ///</summary>
        public static void ReleaseAll()
        {
            lock (ListLock)
            {
                foreach (var listener in _listeners.Values)
                {
                    listener.Stop();
                }

                _listeners.Clear();
            }
        }
    }
}

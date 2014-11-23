using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UniFTP.Server
{
    /// <summary>
    /// TCP Listener连接池
    /// </summary>
    public static class PassiveListenerPool
    {
        private static readonly object ListLock = new object();
        private static Dictionary<AutoResetEvent, TcpListener> _listeners = new Dictionary<AutoResetEvent, TcpListener>();

        /// <summary>
        /// 取得一个TCP Listener
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static TcpListener GetListener(IPAddress ip)
        {
            TcpListener listener = null;

            lock (ListLock)
            {
                //在Listener池中找到一个持有信号量（已经被释放）且IP对应的Listener
                listener = _listeners.FirstOrDefault(p => p.Key.WaitOne(TimeSpan.FromMilliseconds(10)) && ((IPEndPoint)p.Value.LocalEndpoint).Address.Equals(ip)).Value;

                if (listener != null) return listener;
                //没有找到这样的Listener，新建一个
                AutoResetEvent listenerLock = new AutoResetEvent(false);

                listener = new TcpListener(ip, 0);

                _listeners.Add(listenerLock, listener);
            }

            return listener;
        }

        /// <summary>
        /// 释放一个TCP Listener
        /// </summary>
        /// <param name="listener"></param>
        public static void FreeListener(TcpListener listener)
        {
            AutoResetEvent sync = _listeners.SingleOrDefault(p => p.Value == listener).Key;

            sync.Set();
        }

        /// <summary>
        /// 释放所有TCP Listener
        /// </summary>
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

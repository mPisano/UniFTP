using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;

namespace UniFTP.Server.Performance
{
    class SimpleCounter : ICounter
    {
        private Timer _timer = new Timer(1000);
        private readonly object _speedLock = new object();
        //private List<int> _speedCounter = new List<int>(3);
        //private Stopwatch _timer = new Stopwatch();
        //private string _logHeader;
        private TimeSpan _serverTime;
        private ulong _filesSent;
        private ulong _filesReceived;
        private long _bytesSent;
        private long _bytesReceived;
        private int _currentBytesSent;
        private int _currentBytesReceived;
        private uint _anonymousUsers;
        private uint _nonAnonymousUsers;
        private uint _currentConnections;
        private ulong _totalConnectionAttempts;
        private ulong _commandsExecuted;
        private ulong _totalLogonAttempts;
        private long _bytesTotalPerSec;
        private long _bytesReceivedPerSec;
        private long _bytesSentPerSec;

        public SimpleCounter()
        {
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
            Enabled = true;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (_speedLock)
            {
                _bytesReceivedPerSec = _currentBytesReceived;
                _bytesSentPerSec = _currentBytesSent;
                _bytesTotalPerSec = _bytesSentPerSec + _bytesReceivedPerSec;
                _currentBytesReceived = 0;
                _currentBytesSent = 0;
            }
        }

        //public void Start()
        //{
        //    _timer.Start();
        //}

        //public void Stop()
        //{
        //    _timer.Stop();
        //}

        //public double ServerTime => _serverTime.TotalSeconds;
        public bool Enabled { get; private set; }
        public long CurrentNonAnonymousUsers => _nonAnonymousUsers;
        public long CurrentAnonymousUsers => _anonymousUsers;
        public long CurrentConnections => _currentConnections;
        public float BytesReceivedPerSec => _bytesReceivedPerSec;
        public float BytesSentPerSec => _bytesSentPerSec;
        public float BytesTotalPerSec => _bytesTotalPerSec;

        public void IncrementBytesSent(int count)
        {
            _bytesSent += count;
            lock (_speedLock)
            {
                _currentBytesSent += count;
            }
        }

        public void IncrementBytesReceived(int count)
        {
            _bytesReceived += count;
            lock (_speedLock)
            {
                _currentBytesReceived += count;
            }
        }

        public void IncrementFilesSent()
        {
            _filesSent++;
        }

        public void IncrementFilesReceived()
        {
            _filesReceived++;
        }

        public void IncrementAnonymousUsers()
        {
            _anonymousUsers++;
        }

        public void IncrementNonAnonymousUsers()
        {
            _nonAnonymousUsers++;
        }

        public void DecrementAnonymousUsers()
        {
            if (_anonymousUsers > 1u)
            {
                _anonymousUsers--;
            }
            else
            {
                _anonymousUsers = 0;
            }
        }

        public void DecrementNonAnonymousUsers()
        {
            if (_nonAnonymousUsers > 1u)
            {
                _nonAnonymousUsers--;
            }
            else
            {
                _nonAnonymousUsers = 0;
            }
        }

        public void IncrementCurrentConnections()
        {
            _currentConnections++;
        }

        public void DecrementCurrentConnections()
        {
            if (_currentConnections > 1u)
            {
                _currentConnections--;
            }
            else
            {
                _currentConnections = 0;
            }
        }

        public void IncrementTotalConnectionAttempts()
        {
            _totalConnectionAttempts++;
        }

        public void IncrementTotalLogonAttempts()
        {
            _totalLogonAttempts++;
        }

        public void SetFtpServiceUptime(TimeSpan value)
        {
            _serverTime = value;
        }

        public void IncrementCommandsExecuted()
        {
            _commandsExecuted++;
        }

        public void Dispose()
        {
            Enabled = false;
            _timer.Stop();
            _timer.Close();
        }
    }
}

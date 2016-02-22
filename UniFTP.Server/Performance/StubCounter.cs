using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniFTP.Server.Performance
{
    internal class StubCounter : ICounter
    {
        public void Dispose()
        {
        }

        public bool Enabled { get; } = false;
        public long CurrentNonAnonymousUsers { get; }
        public long CurrentAnonymousUsers { get; }
        public long CurrentConnections { get; }
        public float BytesReceivedPerSec { get; }
        public float BytesSentPerSec { get; }
        public float BytesTotalPerSec { get; }
        public void IncrementBytesSent(int count)
        {
        }

        public void IncrementBytesReceived(int count)
        {
        }

        public void IncrementFilesSent()
        {
        }

        public void IncrementFilesReceived()
        {
        }

        public void IncrementAnonymousUsers()
        {
        }

        public void IncrementNonAnonymousUsers()
        {
        }

        public void DecrementAnonymousUsers()
        {
        }

        public void DecrementNonAnonymousUsers()
        {
        }

        public void IncrementCurrentConnections()
        {
        }

        public void DecrementCurrentConnections()
        {
        }

        public void IncrementTotalConnectionAttempts()
        {
        }

        public void IncrementTotalLogonAttempts()
        {
        }

        public void SetFtpServiceUptime(TimeSpan value)
        {
        }

        public void IncrementCommandsExecuted()
        {
        }
    }
}

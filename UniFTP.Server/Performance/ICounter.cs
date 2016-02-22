using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniFTP.Server.Performance
{
    public interface ICounter : IDisposable
    {
        bool Enabled { get; }
        long CurrentNonAnonymousUsers { get; }
        long CurrentAnonymousUsers { get; }
        long CurrentConnections { get; }
        float BytesReceivedPerSec { get; }
        float BytesSentPerSec { get; }
        float BytesTotalPerSec { get; }


        void IncrementBytesSent(int count);
        void IncrementBytesReceived(int count);
        void IncrementFilesSent();
        void IncrementFilesReceived();
        void IncrementAnonymousUsers();
        void IncrementNonAnonymousUsers();
        void DecrementAnonymousUsers();
        void DecrementNonAnonymousUsers();
        void IncrementCurrentConnections();
        void DecrementCurrentConnections();
        void IncrementTotalConnectionAttempts();
        void IncrementTotalLogonAttempts();
        void SetFtpServiceUptime(TimeSpan value);
        void IncrementCommandsExecuted();
    }
}

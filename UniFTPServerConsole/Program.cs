using System;
using UniFTP.Server;

namespace UniFTPServerTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            FtpServer f = new FtpServer(null);
            f.Start();
            Console.ReadLine();
            f.Stop();
            return;
        }
    }
}

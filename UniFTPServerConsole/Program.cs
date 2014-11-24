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
            Console.WriteLine("UniFTP Server Started!");
            Console.ReadLine();
            f.Stop();
            return;
        }
    }
}

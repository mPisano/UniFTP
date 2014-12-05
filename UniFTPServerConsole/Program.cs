using System;
using UniFTP.Server;

namespace UniFTPServerTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            FtpServer f = new FtpServer(null);
            f.Config = new FtpConfig( "D:\\Temp");
            f.LoadConfigs();
            f.AddUserGroup("test", AuthType.Password);
            f.AddGroupRule("test", "/", "rwxrwxrwx");
            f.AddUser("root", "test", "test");
            f.Start();
            Console.WriteLine("UniFTP Server Started!");
            Console.ReadLine();
            f.Stop();
            f.SaveConfigs();
            return;
        }
    }
}

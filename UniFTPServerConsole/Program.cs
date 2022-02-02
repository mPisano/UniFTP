using System;
using UniFTP.Server;

namespace UniFTPServerTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            FtpServer f = new FtpServer(port: 21, enableIPv6: true, ipv6Port: 2121, logHeader: "UniFTP");
            f.Config = new FtpConfig("D:\\Temp", welcome: new string[] { "By Ulysses" });
            //Load configs...
            f.LoadConfigs();
            //or add configs manually
            //f.AddUserGroup("test", AuthType.Password);
            //f.AddGroupRule("test", "/", "rwxrwxrwx");
            //f.AddUser("root", "test", "test");

            //Import SSL cert
            f.ImportCertificate("UniFTP.Open.pfx", null);
            //Log event
            f.OnLog += sender => Console.WriteLine(((FtpLogEntry)sender).ToString());

            f.Config.LogInWelcome = new string[] { "Welcome back,Commander." };

            //Add directory/file link
            f.AddLink("test", "M:\\ACGMusic", "/");
            f.AddGroupRule("test", "/Music", "r-xr-xr-x");

            f.Start();
            Console.WriteLine("UniFTP Server Started!");
            Console.ReadLine();
            f.Stop();
            //Save configs for next time
            f.SaveConfigs();
            return;
        }
    }
}

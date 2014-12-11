using System;
using UniFTP.Server;

namespace UniFTPServerTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            FtpServer f = new FtpServer(port:21,enableIPv6:true,logHeader:"UniFTP");
            f.Config = new FtpConfig( "D:\\Temp",welcome:new string[]{"By Ulysses"});
            //如果有配置文件则可以读取
            f.LoadConfigs();
            //f.Config.LogInWelcome = new string[]{"Welcome back,Commander."};
            //如果没有则通过以下语句加入配置
            //f.AddUserGroup("test", AuthType.Password);
            //f.AddGroupRule("test", "/", "rwxrwxrwx");
            //f.AddUser("root", "test", "test");
            f.AddLink("test", "K:\\剧场版-乐园追放(raw+Airota)", "/");

            f.Start();
            Console.WriteLine("UniFTP Server Started!");
            Console.ReadLine();
            f.Stop();
            //保存配置，下次就可以读取了
            f.SaveConfigs();
            return;
        }
    }
}

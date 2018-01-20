using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using SharpCompress.Common;

//#pragma warning disable CS3021 //CLS，SharpCompress中处理而项目没有，故禁用//已经在csproj文件中禁用

namespace AeroMangaManager
{
    class Program
    {
        static string addr;
        static void Main(string[] args)
        {
            Console.OutputEncoding=Encoding.UTF8;//我只想输出个中文，这玩意咋这么坑啊……dotnet add package System.Text.Encoding.CodePages
            
            ArchiveManager.Init();

            MangaHTTPServer server=new MangaHTTPServer(Directory.GetCurrentDirectory()+"\\website",19191);//记得防火墙设置
            addr="http://"+GetCurrentIP().ToString()+":"+server.Port+"/";

            Logger.Log(addr);
            ProcessStartInfo procInfo=new ProcessStartInfo();
            procInfo.UseShellExecute=true;
            procInfo.FileName="\""+addr+"\"";
            Process.Start(procInfo);
            while(true)
            {
                string cmd=Console.ReadLine();
                switch(cmd)
                {
                    case "x":
                    {
                        //server.Stop();
                        HttpWebRequest request=(HttpWebRequest) HttpWebRequest.Create(addr+"api/ShutDown" );
                        request.Method="GET";
                        request.GetResponse();
                        ArchiveManager.ClearDir(ArchiveManager.tempDirPath);
                        //To-do:删缓存，停止解压进程
                    }break;
                    
                }
                if(server.stopped)
                {
                    break;
                }
            }
        }
        static IPAddress GetCurrentIP()
        {
            //https://stackoverflow.com/a/27376368
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address;
            }

        }

    }
    
}
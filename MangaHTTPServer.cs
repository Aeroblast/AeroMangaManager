// MIT License - Copyright (c) 2016 Can Güney Aksakalli
// 改自https://aksakalli.github.io/2014/02/24/simple-http-server-with-csparp.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Web;
using System.IO;
using System.Threading;
using System.Diagnostics;
using AeroMangaManager;
class MangaHTTPServer
{
    private readonly string[] _indexFiles = {
        "index.html"
    };

    private static IDictionary<string, string> _mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
        #region extension to MIME type list
        {".asf", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".avi", "video/x-msvideo"},
        {".bin", "application/octet-stream"},
        {".cco", "application/x-cocoa"},
        {".crt", "application/x-x509-ca-cert"},
        {".css", "text/css"},
        {".deb", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dll", "application/octet-stream"},
        {".dmg", "application/octet-stream"},
        {".ear", "application/java-archive"},
        {".eot", "application/octet-stream"},
        {".exe", "application/octet-stream"},
        {".flv", "video/x-flv"},
        {".gif", "image/gif"},
        {".hqx", "application/mac-binhex40"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".ico", "image/x-icon"},
        {".img", "application/octet-stream"},
        {".iso", "application/octet-stream"},
        {".jar", "application/java-archive"},
        {".jardiff", "application/x-java-archive-diff"},
        {".jng", "image/x-jng"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".mml", "text/mathml"},
        {".mng", "video/x-mng"},
        {".mov", "video/quicktime"},
        {".mp3", "audio/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpg", "video/mpeg"},
        {".msi", "application/octet-stream"},
        {".msm", "application/octet-stream"},
        {".msp", "application/octet-stream"},
        {".pdb", "application/x-pilot"},
        {".pdf", "application/pdf"},
        {".pem", "application/x-x509-ca-cert"},
        {".pl", "application/x-perl"},
        {".pm", "application/x-perl"},
        {".png", "image/png"},
        {".prc", "application/x-pilot"},
        {".ra", "audio/x-realaudio"},
        {".rar", "application/x-rar-compressed"},
        {".rpm", "application/x-redhat-package-manager"},
        {".rss", "text/xml"},
        {".run", "application/x-makeself"},
        {".sea", "application/x-sea"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".swf", "application/x-shockwave-flash"},
        {".tcl", "application/x-tcl"},
        {".tk", "application/x-tcl"},
        {".txt", "text/plain"},
        {".war", "application/java-archive"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wmv", "video/x-ms-wmv"},
        {".xml", "text/xml"},
        {".xpi", "application/x-xpinstall"},
        {".zip", "application/zip"},
        #endregion
    };
    private Thread _serverThread;
    private CancellationTokenSource _cts;
    private string _rootDirectory;
    private HttpListener _listener;
    private int _port;
    private List<SessionInfo> _sessions;
    private System.Timers.Timer _timer;
    public int Port
    {
        get { return _port; }
        private set { }
    }
    public bool stopped
    {
        get { return !_listener.IsListening; }
    }
    private void Process(HttpListenerContext context)
    {
        int sessionId = 0;
        if (context.Request.Cookies["id"] != null)
        {
            if (context.Request.Cookies["id"].Expired ||
                !Int32.TryParse(context.Request.Cookies["id"].Value, out sessionId))
            {
                sessionId = 0;
            }
            if (sessionId >= _sessions.Count)
            {
                sessionId = 0;
            }
        }
        if (sessionId == 0)
        {
            for (int i = 1; i < _sessions.Count; i++)
            {
                if (_sessions[i].time.CompareTo(DateTime.Now) < 0)
                {
                    sessionId = i;
                }
            }
            if (sessionId == 0)
            {
                _sessions.Add(new SessionInfo());
                sessionId = _sessions.Count - 1;
            }
        }
        _sessions[sessionId].id = sessionId;
        _sessions[sessionId].time = DateTime.Now.AddHours(1);
        Cookie cookie = new Cookie("id", "" + sessionId);
        cookie.Expires = _sessions[sessionId].time;
        context.Response.Cookies.Add(cookie);

        string filename = context.Request.Url.AbsolutePath;
        Logger.Log("Get request:" + filename);
        filename = filename.Substring(1);

        if (string.IsNullOrEmpty(filename))
        {
            foreach (string indexFile in _indexFiles)
            {
                if (File.Exists(Path.Combine(_rootDirectory, indexFile)))
                {
                    filename = indexFile;
                    break;
                }
            }
        }

        //API行为
        string apiAtt = "api/";
        if (filename.IndexOf(apiAtt) == 0)
        {
            string[] cmd = filename.Substring(apiAtt.Length).Split("_");
            string result = null;
            switch (cmd[0])
            {
                case "GetList":
                    {
                        result = "应该返回LIST看不见这玩意";
                        if (cmd.Length > 1)
                        {

                        }
                        else
                        {
                            result = ArchiveManager.GetList(3, 5);
                        }
                    }
                    break;
                case "LoadManga":
                    {
                        if (cmd.Length < 2)
                        {
                            result = "{\"result\":\"Failed\",\"message\":\"请求参数不足。\"}";
                        }
                        else
                        {
                            int mangaId = 0;
                            if (Int32.TryParse(cmd[1], out mangaId))
                            {
                                result = "{\"result\":\"{0}\",\"message\":\"{1}\"}";
                                if (_sessions[sessionId].info == null||!_sessions[sessionId].info.thread.IsAlive)
                                {
                                    _sessions[sessionId].info = ArchiveManager.StartUnarchive(mangaId, sessionId);
                                    if (_sessions[sessionId].info == null)
                                    {
                                        result = result.Replace("{0}", "Failed");
                                        result = result.Replace("{1}", "文件错误。");
                                    }
                                    else
                                    {
                                        result = result.Replace("{0}", "Success");
                                        result = result.Replace("{1}", Path.GetFileName(_sessions[sessionId].info.archivePath));
                                    }
                                }
                                else
                                {
                                    if(_sessions[sessionId].info.thread.IsAlive)
                                    {
                                    result = result.Replace("{0}", "Busy");
                                    result = result.Replace("{1}", "等会。");
                                    }
                                    else
                                    {
                                        result = result.Replace("{0}", "Failed");
                                        result = result.Replace("{1}", "不应该的错误。");
                                    }
                                }


                            }
                            else
                            {
                                result = "{\"result\":\"Failed\",\"message\":\"非法请求。\"}";
                            }
                        }
                    }
                    break;
                case "CheckManga":
                    {
                        if (_sessions[sessionId].pagePaths == null)
                        {
                            _sessions[sessionId].pagePaths = new List<string>();
                        }
                        _sessions[sessionId].pagePaths.Clear();
                        if(_sessions[sessionId].info==null)
                        {

                        }
                        foreach (string path in Directory.GetFiles(_sessions[sessionId].info.tempDirPath))
                        {
                            if (ArchiveManager.isImage(path))
                            {
                                _sessions[sessionId].pagePaths.Add(path);
                            }
                        }
                        _sessions[sessionId].pagePaths.Sort();
                        result = "{\"result\":\"{0}\",\"message\":\"{1}\"}";
                        result = result.Replace("{1}", _sessions[sessionId].pagePaths.Count.ToString());
                        if (_sessions[sessionId].info.thread.IsAlive)
                            result = result.Replace("{0}", "Loading");
                        else if (_sessions[sessionId].info.success)
                            result = result.Replace("{0}", "Success");
                        else
                            result = result.Replace("{0}", "Failed");
                    }
                    break;
                case "GetPage":
                    {
                        int page = 0;
                        if (cmd.Length >= 2)
                            if (Int32.TryParse(cmd[1], out page))
                                if (page>=0&&page < _sessions[sessionId].pagePaths.Count)
                                {
                                    ServeFile(context, _sessions[sessionId].pagePaths[page]);
                                }

                    }
                    break;
                case "ShutDown":
                    {
                        //不知道为啥GetContext不管是Stop Abort都会阻塞,又不让我强行停线程……反正也要做Web端就写成web api吧
                        result = "溜了";
                        _cts.Cancel();
                    }
                    break;
            }
            if (result != null)
            {
                try
                {
                    byte[] resultByte = Encoding.UTF8.GetBytes(result);
                    context.Response.ContentType = "text/plain";
                    context.Response.ContentLength64 = resultByte.Length;
                    context.Response.OutputStream.Write(resultByte, 0, resultByte.Length);
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.OutputStream.Flush();
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    Logger.Log(ex.ToString());
                }
            }
        }
        else
        {
            //一般获取文件
            filename = Path.Combine(_rootDirectory, filename);
            ServeFile(context, filename);
        }
        context.Response.AddHeader("Server", "AeroMangaManager/0.1");
        context.Response.OutputStream.Close();
    }

    //仅在Process中调用！
    private void ServeFile(HttpListenerContext context, string filename)
    {
        if (File.Exists(filename))
        {
            try
            {
                Stream input = new FileStream(filename, FileMode.Open);

                //Adding permanent http response headers
                string mime;
                context.Response.ContentType =
                 _mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime) ? mime : "application/octet-stream";
                context.Response.ContentLength64 = input.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r"));

                byte[] buffer = new byte[1024 * 16];
                int nbytes;
                while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                    context.Response.OutputStream.Write(buffer, 0, nbytes);
                input.Close();

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Flush();
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                Logger.Log(ex.ToString());
            }

        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }
    }
    /// <summary>
    /// Construct server with given port.
    /// </summary>
    /// <param name="path">Directory path to serve.</param>
    /// <param name="port">Port of the server.</param>
    public MangaHTTPServer(string path, int port)
    {
        this.Initialize(path, port);
    }

    /// <summary>
    /// Construct server with suitable port.
    /// </summary>
    /// <param name="path">Directory path to serve.</param>
    public MangaHTTPServer(string path)
    {
        //get an empty port
        TcpListener l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        this.Initialize(path, port);
    }

    /// <summary>
    /// Stop server and dispose all functions.
    /// </summary>
    public void Stop()
    {
        _cts.Cancel();

    }

    private void Listen()
    {
        try
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:" + _port.ToString() + "/");
            _listener.Start();
        }
        catch (Exception e)
        {
            e.ToString();
            Logger.Log("权限不足或者未开放域名监听？");
            return;
        }


        while (true)
        {
            try
            {
                if (_cts.Token.IsCancellationRequested)
                {
                    Logger.Log("已安全退出。回车键关闭……");
                    _listener.Stop();
                    return;
                }

                HttpListenerContext context = _listener.GetContext();
                Process(context);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }
    }



    private void Initialize(string path, int port)
    {
        this._rootDirectory = path;
        this._port = port;
        _cts = new CancellationTokenSource();
        _serverThread = new Thread(this.Listen);
        _serverThread.Start();
        _sessions = new List<SessionInfo>();
        _sessions.Add(new SessionInfo());
        _timer = new System.Timers.Timer();
        _timer.Enabled = true;
        _timer.Interval = 1000;
        _timer.Start();
        _timer.Elapsed += new System.Timers.ElapsedEventHandler(TimerEvent);
    }

    private static void TimerEvent(object source, System.Timers.ElapsedEventArgs e)
    {

        //Logger.Log("OK, test event is fired at: " + DateTime.Now.ToString());

    }

}
class SessionInfo
{
    public int id;
    public DateTime time;
    public UnarchiveInfo info;

    public List<string> pagePaths;
}
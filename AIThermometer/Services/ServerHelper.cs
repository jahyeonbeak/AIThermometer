using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using AIThermometer.Cores;
using System.Windows;
using AIThermometer.Windows;

namespace AIThermometer.Services
{
    public class ServerHelper : IDisposable
    {
        HttpListener httpListener;
        bool stopped;

        public delegate void ErrorWindowHandler(string title, string mess);
        public ErrorWindowHandler ewHandler = null;

        public delegate bool CaptureHandler(string file_path, CamMode mode);
        public CaptureHandler captureHandler = null;

        #region IDisposable
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        int disposedFlag;

        ~ServerHelper()
        {
            Dispose(false);
        }

        /// <summary>
        /// 释放所占用的资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 获取该对象是否已经被释放
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public bool IsDisposed
        {
            get
            {
                return disposedFlag != 0;
            }
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (System.Threading.Interlocked.Increment(ref disposedFlag) != 1) return;
            if (disposing)
            {
                stopped = true;
                httpListener.Stop();
            }
            //在这里编写非托管资源释放代码
        }


        public string image_dir_path = "";
        public float temp_limit = 0f;
        public int port = 1;
        // 定义一个静态变量来保存类的实例
        private static ServerHelper instance = null;

        // 定义一个标识确保线程同步
        private static readonly object padlock = new object();


        public ServerHelper()
        {

        }

        public static ServerHelper Instance()
        {
            if (instance == null)
            {
                lock (padlock)
                {
                    // 如果类的实例不存在则创建，否则直接返回
                    if (instance == null)
                    {
                        instance = new ServerHelper();
                    }
                }
            }
            return instance;
        }


        public void Initialize()
        {
            httpListener = new HttpListener();
            try
            {
                httpListener.Prefixes.Add(string.Format("http://+:{0}/Report/", port));
                httpListener.Start();
                httpListener.BeginGetContext(GetHttpContextCallback, null);
            }
            catch (Exception e)
            {
                ErrorWindow ew = new ErrorWindow("错误", "服务器初始化错误，请稍后重启程序");
                ew.Show();
            }

        }

        public void GetHttpContextCallback(IAsyncResult iar)
        {
            if (stopped) return;
            var context = httpListener.EndGetContext(iar);
            httpListener.BeginGetContext(GetHttpContextCallback, null);
            string endPoint = context.Request.RemoteEndPoint.ToString();
            int spIndex = endPoint.IndexOf(":");
            endPoint = endPoint.Substring(0, spIndex);

            using (HttpListenerResponse response = context.Response)
            {
                //get 的方式在如下解析即可得到客户端参数及值
                //string userName = context.Request.QueryString["userName"];
                //string password = context.Request.QueryString["password"];
                //string suffix = context.Request.QueryString["suffix"];
                //string adType = context.Request.QueryString["adtype"];//文字,图片,视频



                if (!context.Request.HasEntityBody)//无数据
                {
                    response.StatusCode = 403;
                    return;
                }

                string attachValue = "";

                //post 的方式有文件上传的在如下解析即可得到客户端参数及值

                HttpListenerRequest request = context.Request;
                if (request.ContentType.Length > 20 && string.Compare(request.ContentType.Substring(0, 20), "multipart/form-data;", true) == 0)
                {
                    List<Values> lst = new List<Values>();
                    Encoding Encoding = request.ContentEncoding;
                    string[] values = request.ContentType.Split(';').Skip(1).ToArray();
                    string boundary = string.Join(";", values).Replace("boundary=", "").Trim();
                    byte[] ChunkBoundary = Encoding.GetBytes("--" + boundary + "\r\n");
                    byte[] EndBoundary = Encoding.GetBytes("--" + boundary + "--\r\n");
                    Stream SourceStream = request.InputStream;
                    var resultStream = new MemoryStream();
                    bool CanMoveNext = true;
                    Values data = null;
                    while (CanMoveNext)
                    {
                        byte[] currentChunk = ReadLineAsBytes(SourceStream);
                        if (!Encoding.GetString(currentChunk).Equals("\r\n"))
                            resultStream.Write(currentChunk, 0, currentChunk.Length);
                        if (CompareBytes(ChunkBoundary, currentChunk))
                        {
                            byte[] result = new byte[resultStream.Length - ChunkBoundary.Length];
                            resultStream.Position = 0;
                            resultStream.Read(result, 0, result.Length);
                            CanMoveNext = true;
                            if (result.Length > 0)
                                data.datas = result;
                            data = new Values();
                            lst.Add(data);
                            resultStream.Dispose();
                            resultStream = new MemoryStream();

                        }
                        else if (Encoding.GetString(currentChunk).Contains("Content-Disposition"))
                        {
                            byte[] result = new byte[resultStream.Length - 2];
                            resultStream.Position = 0;
                            resultStream.Read(result, 0, result.Length);
                            CanMoveNext = true;
                            data.name = Encoding.GetString(result).Replace("Content-Disposition: form-data; name=\"", "").Replace("\"", "").Split(';')[0];
                            resultStream.Dispose();
                            resultStream = new MemoryStream();
                        }
                        else if (Encoding.GetString(currentChunk).Contains("Content-Type"))
                        {
                            CanMoveNext = true;
                            data.type = 1;
                            resultStream.Dispose();
                            resultStream = new MemoryStream();
                        }
                        else if (CompareBytes(EndBoundary, currentChunk))
                        {
                            byte[] result = new byte[resultStream.Length - EndBoundary.Length - 2];
                            resultStream.Position = 0;
                            resultStream.Read(result, 0, result.Length);
                            data.datas = result;
                            resultStream.Dispose();
                            CanMoveNext = false;
                        }
                    }
                    TempMessage tm = new TempMessage();

                    foreach (var key in lst)
                    {
                        if (key.type == 0)
                        {
                            string value = Encoding.GetString(key.datas).Replace("\r\n", "");
                            if (key.name == "attachValue")
                                attachValue = value;

                        }
                        if (key.type == 1)
                        {
                            tm.bytes = key.datas;
                            /*
                            FileStream fs = new FileStream("c:\\3.jpg", FileMode.Create);
                            fs.Write(key.datas, 0, key.datas.Length);
                            fs.Close();
                            fs.Dispose();
                            */
                        }
                    }

                    Console.WriteLine(attachValue);
                    CameraWarning items = JsonHelper.FromJSON<CameraWarning>(attachValue);
                    tm.cam = AIThermometerAPP.Instance().cameras_config.GetNameByIP(endPoint);
                    if (items.Code == 0 && !AIThermometerAPP.Instance().blackcell_pos_error)
                    {
                        tm.temp = items.Reports[0].Temperature;
                        tm.date = DateTime.Now;

                        string file_name = tm.date.ToString("yyMMdd") + "\\";
                        if (tm.temp > temp_limit)
                        {
                            file_name = file_name + "nopass\\";
                            AIThermometerAPP.Instance().AddNoPassFace();
                            VoicePlayer.Instance().Play();

                        }
                        else
                        {
                            file_name = file_name + "pass\\";
                            AIThermometerAPP.Instance().AddPassFace();
                        }

                        if (AIThermometerAPP.Instance().CanCapture())
                        {
                            captureHandler?.Invoke(AIThermometerAPP.Instance().CapturePath() + "\\" + DateTime.Now.ToString("yyMMddHHmmssffff")+".jpeg", CamMode.NORMAL);
                        }

                        Common.CreateDir(image_dir_path + "\\" + file_name);
                        file_name = file_name + tm.date.ToString("HHmmssffff") + tm.temp.ToString().Replace(".", "_") + ".jpeg";
                        tm.photo_path = image_dir_path + "\\" + file_name;
                        TempWarning.Instance().Push(tm);
                    }
                    else
                    {
                        // 黑体位置错误
                        if (!AIThermometerAPP.Instance().blackcell_pos_error)
                        {
                            AIThermometerAPP.Instance().blackcell_pos_error = true;
                            
                            ewHandler?.Invoke("错误", "黑体位置错误或被遮挡,请检查");
                            AIThermometerAPP.Instance().ResetBlackCell();
                        }
                    }
                }

                response.ContentType = "text/html;charset=utf-8";
                response.StatusCode = 200;
                return;
                try
                { 
                    using (System.IO.Stream output = response.OutputStream)
                    using (StreamWriter writer = new StreamWriter(output, Encoding.UTF8))
                        writer.WriteLine("接收完成！");
                }
                catch
                {
                }
                response.Close();
            }
        }

        byte[] ReadLineAsBytes(Stream SourceStream)
        {
            var resultStream = new MemoryStream();
            while (true)
            {
                int data = SourceStream.ReadByte();
                resultStream.WriteByte((byte)data);
                if (data == 10)
                    break;
            }
            resultStream.Position = 0;
            byte[] dataBytes = new byte[resultStream.Length];
            resultStream.Read(dataBytes, 0, dataBytes.Length);
            return dataBytes;
        }

        bool CompareBytes(byte[] source, byte[] comparison)
        {
            int count = source.Length;
            if (source.Length != comparison.Length)
                return false;
            for (int i = 0; i < count; i++)
                if (source[i] != comparison[i])
                    return false;
            return true;
        }

        public class Values
        {
            public int type = 0;//0参数，1文件
            public string name;
            public byte[] datas;
        }
    }
}

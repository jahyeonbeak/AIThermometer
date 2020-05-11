using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Vlc.DotNet.Wpf;
using Vlc.DotNet.Core;
using AIThermometer.Cores;
using System.Threading;

namespace AIThermometer.Windows
{
    using System.Net.NetworkInformation;
    using MultiCam = Dictionary<CamMode, CamStream>;

    /// <summary>
    /// VlcWindow.xaml 的交互逻辑
    /// </summary>
    public partial class VlcWindow : UserControl
    {
        enum ViewState
        {
            IR,
            NORMAL,
            BOTH
        }

        private static Dictionary<CamMode, VlcCamera> multiPlayer = new Dictionary<CamMode, VlcCamera>();
        private ViewState viewState = ViewState.BOTH;
        public string ip = "";
        private static Mutex mut = new Mutex();
        bool runCheck = false;

        //System.Threading.Timer normalTimer;
        //System.Threading.Timer irTimer;

        Thread normalTimer;//  = new Thread(CheckConnection);
        Thread irTimer; //  = new Thread(CheckConnection);
        bool normalTRun;
        bool irTRun;

        private static object lockn = new object();
        private static object locki = new object();

        private int video_timeout = 10;
        int normalNeedReset = 0;
        int irNeedReset = 0;

        public VlcWindow()
        {
            InitializeComponent();
            video_timeout = AIThermometerAPP.Instance().config.video_timeout;
            //ChangeLeft();
            //SetupTest();
        }
        

        public void Shutdown()
        {
            normalTRun = false;
            irTRun = false;
            if (irTimer != null)
            {
                if (irTimer.ThreadState != ThreadState.Suspended)
                {
                    irTimer.Abort();
                }
                else
                {
                    irTimer.Resume();
                    irTimer.Abort();
                }
            }
            if (normalTimer != null)
            {
                if (normalTimer.ThreadState != ThreadState.Suspended)
                {
                    normalTimer.Abort();
                }
                else
                {
                    normalTimer.Resume();
                    normalTimer.Abort();
                }
            }
            Thread.Sleep(1000);
            foreach (var a in multiPlayer)
            {
                a.Value.Stop();
            }
            multiPlayer.Clear();
        }

        public void SetCamStream(MultiCam mc)
        {
            runCheck = true;

            normalTimer = new Thread(normal_Tick);
            irTimer = new Thread(ir_Tick);
            normalTRun = true;
            irTRun = true;
            normalNeedReset = 0;
            irNeedReset = 0;

            foreach (var item in mc)
            {
                // 工厂的接口转成控件
                var vc = item.Value as VlcCamera;
                multiPlayer.Add(item.Key, vc);
                //if (item.Key == CamMode.IR)

                RePlayMedia(item.Key, "");
                ip = vc.GetIP();

                // 生成ping线程
                if (item.Key == CamMode.IR)
                {
                    //Thread t = new Thread(CheckConnection);
                    //t.Start(vc.GetIP());
                }
                vc.vlcPlayer.Height = this.Height;
                v1.Height = this.Height;
                v2.Height = this.Height;
                vc.vlcPlayer.HorizontalAlignment = HorizontalAlignment.Stretch;
                vc.vlcPlayer.VerticalAlignment = VerticalAlignment.Stretch;
                var viewBox = vc.vlcPlayer.Content as Viewbox;
                viewBox.Stretch = System.Windows.Media.Stretch.Fill;
                
            }
            
            //aTimer.Start();

            //(mc[CamMode.IR] as VlcCamera).vlcPlayer.SetValue(Grid.ColumnProperty, 1);
            //(mc[CamMode.NORMAL] as VlcCamera).vlcPlayer.SetValue(Grid.ColumnProperty, 0);

            this.v1.Children.Add((mc[CamMode.IR] as VlcCamera).vlcPlayer);
            this.v2.Children.Add((mc[CamMode.NORMAL] as VlcCamera).vlcPlayer);
            //v1.Width = this.Width / 2;
            //v2.Width = this.Width / 2;
        }

        public void DelCamStream()
        {
            //normalTimer.;//.p.Abort();// = new Thread(normal_Tick);
            //irTimer = null;//.Abort();// = new Thread(ir_Tick);


            
            this.v1.Children.Clear();// ((mc[CamMode.IR] as VlcCamera).vlcPlayer);
            this.v2.Children.Clear();// ((mc[CamMode.NORMAL] as VlcCamera).vlcPlayer);
            Shutdown();

        }

    private void CheckConnection(object _ip)
        {
            Ping ping = new Ping();
            string ip = _ip as string;
            int count = 0;
            bool isConnected = true;
            while (runCheck)
            {
                try
                {
                    if (ping.Send(ip, 2000).Status == IPStatus.Success)
                    {
                        if (!isConnected)
                        {
                            foreach (var item in multiPlayer)
                            {
                                // 工厂的接口转成控件
                                var vc = item.Value as VlcCamera;
                                RePlayMedia(item.Key, vc.Player().GetMedia().TrackID);
                            }
                        }
                        isConnected = true;
                        LogHelper.WriteLog(ip + " Online; No. ");
                        Thread.Sleep(500);
                    }
                    else
                    {
                        count++;
                        LogHelper.WriteLog(ip + " Offline, Ping count :" + count);

                        if (count >= 5)
                        {
                            if (isConnected)
                            {
                                foreach (var item in multiPlayer)
                                {
                                    // 工厂的接口转成控件
                                    var vc = item.Value as VlcCamera;
                                    vc.Player().Stop();
                                }
                            }
                            isConnected = false;

                        }
                        Thread.Sleep(200);
                    }
                }
                catch (Exception e)
                {

                }
            }
        }

        private void StreamError(object sender, EventArgs e)
        {
            //
        }

        private void RePlayMedia(CamMode cm, string url)
        {
            LogHelper.WriteLog(cm + " stream will be restarting, path:" + url);

            multiPlayer[cm].SetLibPath(AIThermometerAPP.Instance().AppPath());
            if (url == "")
                multiPlayer[cm].SetMode(cm);
            multiPlayer[cm].Init();
            LogHelper.WriteLog("first start "+ cm + " rstp, value="+ multiPlayer[cm].GetPath());
            multiPlayer[cm].Player().OnMediaPlayerBuffering(0.02f);
            multiPlayer[cm].Player().Play(new Uri(multiPlayer[cm].GetPath()));
            multiPlayer[cm].Player().GetMedia().TrackID = multiPlayer[cm].GetPath();
            multiPlayer[cm].Player().GetMedia().TrackNumber = multiPlayer[cm].GetMode().ToString();
            multiPlayer[cm].Player().Playing += playing;
            multiPlayer[cm].Player().Buffering += buffering;
            multiPlayer[cm].Player().EncounteredError += StreamError;
            RefreshView();


            /*
            if (cm == CamMode.IR)
            {
                //multiPlayer[cm].Player().EncounteredError += StreamError;
                multiPlayer[cm].Player().Buffering += buffering;
                //multiPlayer[cm].Player().Stopped += StreamError;
            }
            //else
                //multiPlayer[cm].Player().Stopped += StreamError;
                */

        }

        void normal_Tick()
        {
            while (true)
            {
                lock (lockn)
                {
                    if (normalNeedReset++ > video_timeout)
                    {
                        try
                        {
                            //multiPlayer[CamMode.NORMAL].Init();
                            LogHelper.WriteLog("key= " + CamMode.NORMAL + "value=" + multiPlayer[CamMode.NORMAL].GetPath());
                            multiPlayer[CamMode.NORMAL].Player().OnMediaPlayerBuffering(0.02f);
                            multiPlayer[CamMode.NORMAL].Player().Play(new Uri(multiPlayer[CamMode.NORMAL].GetPath()));
                            multiPlayer[CamMode.NORMAL].Player().GetMedia().TrackID = multiPlayer[CamMode.NORMAL].GetPath();
                            multiPlayer[CamMode.NORMAL].Player().GetMedia().TrackNumber = multiPlayer[CamMode.NORMAL].GetMode().ToString();

                            LogHelper.WriteLog("Need Reset");
                            normalNeedReset = 0;
                            normalTimer.Suspend();//...Stop();
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteLog("normaltimer stop error", ex);
                        }
                    }
                    else
                    {
                        LogHelper.WriteLog("tick normal count : " + normalNeedReset.ToString());
                        Console.WriteLine("tick normal count : " + irNeedReset.ToString());

                    }
                }
                Thread.Sleep(1000);
            }
        }

        void ir_Tick()
        {
            while (true)
            {
                lock (locki)
                {
                    if (irNeedReset++ > video_timeout)
                    {
                        try
                        {
                            //multiPlayer[CamMode.NORMAL].Init();
                            LogHelper.WriteLog("key= " + CamMode.IR + " value=" + multiPlayer[CamMode.IR].GetPath());
                            multiPlayer[CamMode.IR].Player().OnMediaPlayerBuffering(0.02f);
                            multiPlayer[CamMode.IR].Player().Play(new Uri(multiPlayer[CamMode.IR].GetPath()));
                            multiPlayer[CamMode.IR].Player().GetMedia().TrackID = multiPlayer[CamMode.IR].GetPath();
                            multiPlayer[CamMode.IR].Player().GetMedia().TrackNumber = multiPlayer[CamMode.IR].GetMode().ToString();

                            LogHelper.WriteLog("ir Need Reset");
                            irTimer.Suspend();//...Stop();
                            irNeedReset = 0;
                            //irTimer.Stop();
                        }
                        catch(Exception ex)
                        {
                            LogHelper.WriteLog("irtimer stop error", ex);
                        }
                    }
                    else
                    {
                        LogHelper.WriteLog("tick ir count : " + irNeedReset.ToString());
                        Console.WriteLine("tick ir count : " + irNeedReset.ToString());
                    }
                }
                Thread.Sleep(1000);

            }
        }



        private void playing(object sender, EventArgs e)
        {
            var vmp = sender as VlcMediaPlayer;
            //LogHelper.WriteLog("Stream Error Mode : " + vmp.GetMedia().TrackNumber);
            //vmp.Play(new Uri(vmp.GetMedia().TrackID));

            // Get cammode
            CamMode cm = (CamMode)Enum.Parse(typeof(CamMode), vmp.GetMedia().TrackNumber);
            if (cm == CamMode.NORMAL)
            {
                LogHelper.WriteLog("Stream ok URL : " + vmp.GetMedia().TrackID );
                LogHelper.WriteLog("-------------------"+normalTimer.ThreadState);
                try
                {
                    if (normalTimer.ThreadState != ThreadState.Unstarted)
                        normalTimer.Suspend();//.Stop();
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("normal Timer error : ", ex);
                }
                normalNeedReset = 0;
            }
            else
            {
                LogHelper.WriteLog("Stream ir ok URL : " + vmp.GetMedia().TrackID);
                //irTimer.Stop();
                try
                {
                    if (normalTimer.ThreadState != ThreadState.Unstarted)
                        irTimer.Suspend();//.Stop();
                }
                catch(Exception ex)
                {
                    LogHelper.WriteLog("ir Timer error :", ex);
                }
                irNeedReset = 0;
            }
        }
        private void stoping(object sender, EventArgs e)
        {
            LogHelper.WriteLog("stop ");
            //LogHelper.WriteLog("[P] - StreamingVideo -  aaaaaaaaa REACHED + " + DateTime.Now);
        }
        private void opening(object sender, EventArgs e)
        {
            //LogHelper.WriteLog("opening " + this.vlcPlayer.SourceProvider.MediaPlayer.State);
            //LogHelper.WriteLog("[P] - StreamingVideo -  aaaaaaaaa REACHED + " + DateTime.Now);
        }
        private void buffering(object sender, EventArgs e)
        {
            var vmp = sender as VlcMediaPlayer;
            //LogHelper.WriteLog("Stream Error Mode : " + vmp.GetMedia().TrackNumber);
            //vmp.Play(new Uri(vmp.GetMedia().TrackID));

            // Get cammode
            try {
                CamMode cm = (CamMode)Enum.Parse(typeof(CamMode), vmp.GetMedia().TrackNumber);
                if (cm == CamMode.NORMAL)
                {
                    LogHelper.WriteLog("Stream buff URL : " + vmp.GetMedia().TrackID);
                    if (normalTimer.ThreadState == ThreadState.Unstarted)
                        normalTimer.Start();
                    else if (normalTimer.ThreadState == ThreadState.Suspended)
                        normalTimer.Resume();
                }
                else
                {
                    if (irTimer.ThreadState == ThreadState.Unstarted)
                        irTimer.Start();
                    else if (irTimer.ThreadState == ThreadState.Suspended)
                        irTimer.Resume();
                    //irTimer.Start();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("buffering error ", ex);
            }
            //RePlayMedia(cm, vmp.GetMedia().TrackID);

            //normalNeedReset
            //LogHelper.WriteLog("[P] - StreamingVideo -  aaaaaaaaa REACHED + " + DateTime.Now);
        }
        private void vlc_MediaPlayerEncounteredError(object sender, EventArgs e)
        {
            //LogHelper.WriteLog("error " + this.vlcPlayer.SourceProvider.MediaPlayer.State);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //this.vlcPlayer.SourceProvider.MediaPlayer.Play(new Uri("rtsp://wowzaec2demo.streamlock.net/vod/mp4:BigBuckBunny_115k.mov"));
        }

        public bool Shot(string save_path, CamMode mode)
        {

            try
            {
                FileInfo file = new FileInfo(save_path);
                if (file.Exists)
                {
                    file.Delete();
                }
                multiPlayer[mode].vlcPlayer.SourceProvider.MediaPlayer.TakeSnapshot(file);
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }


        public void ChangeLeft()
        {
            viewState = ViewState.NORMAL;
            v1.Visibility = Visibility.Hidden;
            v1.Width = 0;
            v2.Visibility = Visibility.Visible;
            v2.Width = this.Width;
            
            
            //.vlcPlayer.Height = this.Height;

            RefreshView();

        }

        public void ChangeRight()
        {
            viewState = ViewState.IR;
            v1.Visibility = Visibility.Visible;
            v1.Width = this.Width;
            v2.Visibility = Visibility.Hidden;
            v2.Width = 0;
            
            RefreshView();

        }

        public void ChangeBoth()
        {
            viewState = ViewState.BOTH;
            v1.Visibility = Visibility.Visible;
            v1.Width = this.Width / 2;
            v2.Visibility = Visibility.Visible;
            v2.Width = this.Width / 2;
            


            RefreshView();

        }

        private void RefreshView()
        {
            if (multiPlayer.Count != 2)
                return;

            switch (viewState)
            {
                case ViewState.NORMAL:
                    multiPlayer[CamMode.NORMAL].vlcPlayer.Width = this.Width;
                    multiPlayer[CamMode.IR].vlcPlayer.Width = 0;
                    break;
                case ViewState.IR:
                    multiPlayer[CamMode.NORMAL].vlcPlayer.Width = 0;
                    multiPlayer[CamMode.IR].vlcPlayer.Width = this.Width;
                    break;
                case ViewState.BOTH:
                    multiPlayer[CamMode.NORMAL].vlcPlayer.Width = this.Width / 2;
                    multiPlayer[CamMode.IR].vlcPlayer.Width = this.Width / 2;
                    break;
                default:
                    break;
            }

            }
    }
}

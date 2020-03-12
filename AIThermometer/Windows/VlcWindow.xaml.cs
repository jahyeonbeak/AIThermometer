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

        public VlcWindow()
        {
            InitializeComponent();
            //ChangeLeft();
            //SetupTest();
        }
        

        public void Shutdown()
        {
            foreach (var a in multiPlayer)
            {
                a.Value.Stop();
            }
            multiPlayer.Clear();
        }


        public void RunCheck(bool rc)
        {
            mut.WaitOne();
            runCheck = rc;
            mut.ReleaseMutex();
        }

        public void SetCamStream(MultiCam mc)
        {
            runCheck = true;
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
                    Thread t = new Thread(CheckConnection);
                    t.Start(vc.GetIP());
                }

                //vc.vlcPlayer.HorizontalAlignment = HorizontalAlignment.Stretch;
                //vc.vlcPlayer.VerticalAlignment = VerticalAlignment.Stretch;
            }

            //(mc[CamMode.IR] as VlcCamera).vlcPlayer.SetValue(Grid.ColumnProperty, 1);
            //(mc[CamMode.NORMAL] as VlcCamera).vlcPlayer.SetValue(Grid.ColumnProperty, 0);

            this.v1.Children.Add((mc[CamMode.IR] as VlcCamera).vlcPlayer);
            this.v2.Children.Add((mc[CamMode.NORMAL] as VlcCamera).vlcPlayer);
            v1.Width = this.Width / 2;
            v2.Width = this.Width / 2;
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
                        Console.WriteLine(ip + " Online; No. ");
                        Thread.Sleep(500);
                    }
                    else
                    {
                        count++;
                        Console.WriteLine(ip + " Offline, Ping count :" + count);

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
            var vmp = sender as VlcMediaPlayer;
            Console.WriteLine("Stream Error URL : " + vmp.GetMedia().TrackID);
            Console.WriteLine("Stream Error Mode : " + vmp.GetMedia().TrackNumber);
            //vmp.Play(new Uri(vmp.GetMedia().TrackID));
            CamMode cm = (CamMode)Enum.Parse(typeof(CamMode), vmp.GetMedia().TrackNumber);
            RePlayMedia(cm, vmp.GetMedia().TrackID);
        }

        private void RePlayMedia(CamMode cm, string url)
        {
            Console.WriteLine(cm + " stream will be restarting, path:" + url);

            multiPlayer[cm].SetLibPath(AIThermometerAPP.Instance().AppPath());
            if (url == "")
                multiPlayer[cm].SetMode(cm);
            multiPlayer[cm].Init();
            Console.WriteLine("key={0},value={1}", cm, multiPlayer[cm].GetPath());
            multiPlayer[cm].Player().OnMediaPlayerBuffering(0.02f);
            multiPlayer[cm].Player().Play(new Uri(multiPlayer[cm].GetPath()));
            multiPlayer[cm].Player().GetMedia().TrackID = multiPlayer[cm].GetPath();
            multiPlayer[cm].Player().GetMedia().TrackNumber = multiPlayer[cm].GetMode().ToString();

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




     
        private void playing(object sender, EventArgs e)
        {
            var aa = sender as VlcMediaPlayer;
            //Console.WriteLine(" play " + this.vlcPlayer.SourceProvider.MediaPlayer.State);
            //Console.WriteLine("[P] - StreamingVideo -  aaaaaaaaa REACHED + " + DateTime.Now);
        }
        private void stoping(object sender, EventArgs e)
        {
            Console.WriteLine("stop ");
            //Console.WriteLine("[P] - StreamingVideo -  aaaaaaaaa REACHED + " + DateTime.Now);
        }
        private void opening(object sender, EventArgs e)
        {
            //Console.WriteLine("opening " + this.vlcPlayer.SourceProvider.MediaPlayer.State);
            //Console.WriteLine("[P] - StreamingVideo -  aaaaaaaaa REACHED + " + DateTime.Now);
        }
        private void buffering(object sender, EventArgs e)
        {
            Console.WriteLine("buffering ");
            //Console.WriteLine("[P] - StreamingVideo -  aaaaaaaaa REACHED + " + DateTime.Now);
        }
        private void vlc_MediaPlayerEncounteredError(object sender, EventArgs e)
        {
            //Console.WriteLine("error " + this.vlcPlayer.SourceProvider.MediaPlayer.State);
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

        }
    }
}

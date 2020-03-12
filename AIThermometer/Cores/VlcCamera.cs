using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vlc.DotNet.Core;
using Vlc.DotNet.Wpf;

namespace AIThermometer.Cores
{
    class VlcCamera : CamStream
    {
        private string name = "";
        private string path = "";
        private string lib_path = "";
        private string ip = "";
        private CamMode mode = CamMode.NONE;
        public VlcControl vlcPlayer;

        public VlcCamera()
        {

        }

        public VlcCamera(string _name, string _path, string _lib_path, CamMode _mode = CamMode.NONE)
        {
            this.name = _name;
            this.path = _path;
            this.lib_path = _lib_path;
            this.mode = _mode;

        }

        public void Init()
        {
            if (vlcPlayer == null)
            {
                vlcPlayer = new VlcControl();
            }

            if (name == "")
                return;

            var vlcLibDirectory = new DirectoryInfo(lib_path);

            var options = new string[]
            {
                //"--rtsp-timeout=10",
                //"--rtsp-tcp",
                //"--ffmpeg-skip-frame=1",
                "--network-caching=400",
                "--no-osd","--no-snapshot-preview",
                //"--rtsp-frame-buffer-size=1000000",
                //添加日志
                //"--file-logging", "-vvv", "--logfile=Logs.log"
                // VLC options can be given here. Please refer to the VLC command line documentation.
            };
            vlcPlayer.SourceProvider.Dispose();
            vlcPlayer.SourceProvider.CreatePlayer(vlcLibDirectory, options);
        }

        public void Start()
        {
            
        }

        public void Stop()
        {
            try
            {
                this.vlcPlayer.SourceProvider.MediaPlayer.Dispose();//.MediaPlayer.Stop();
                this.vlcPlayer.SourceProvider.Dispose();
                this.vlcPlayer.Dispose();
                Console.WriteLine("dispose");
            }
            catch
            {

            }
        }

        public void SetLibPath(string str)
        {
            this.lib_path = str;
        }

        public void SetPath(string path)
        {
            this.path = path;
        }

        public string GetPath()
        {
            return this.path;
        }

        public void SetIP(string ip)
        {
            this.ip = ip;
        }

        public string GetIP()
        {
            return this.ip;
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public string GetName()
        {
            return this.name;
        }

        public void SetMode(CamMode md)
        {
            this.mode = md;
            RealPath();
        }

        public CamMode GetMode()
        {
            return this.mode;
        }

        public VlcMediaPlayer Player()
        {
            return this.vlcPlayer.SourceProvider.MediaPlayer;
        }

        private void RealPath()
        {
            // test mode
            if (this.path == "127.0.0.1")
            {
                if (this.mode == CamMode.NORMAL)
                    this.path = AIThermometerAPP.Instance().AppPath() + "test.mp4";
                else
                    this.path = AIThermometerAPP.Instance().AppPath() + "test.mp4";
                //this.path = "rtsp://wowzaec2demo.streamlock.net/vod/mp4:BigBuckBunny_115k.mov";

                Console.WriteLine("Local test mode! 127 0 0 1");
                return;
            }

            if (this.mode == CamMode.NORMAL)
                //this.path = AIThermometerAPP.Instance().AppPath() + "test.mp4";
            this.path = "rtsp://" + this.path + ":8554/live.sdp";
            else if (this.mode == CamMode.IR)
                //this.path = AIThermometerAPP.Instance().AppPath() + "test.mp4";
            this.path = "rtsp://" + this.path + ":8555/liveultra.sdp";
        }
    }
}

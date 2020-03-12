using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIThermometer.Services;
using System.IO;
using Newtonsoft.Json;
using AIThermometer.Cores;
using System.Threading;

namespace AIThermometer
{
    public sealed class AIThermometerAPP
    {
        // 定义一个静态变量来保存类的实例
        private static AIThermometerAPP instance = null;

        // 定义一个标识确保线程同步
        private static readonly object padlock = new object();

        // 系统参数区
        private string app_path = "";
        private string camera_path = "";
        private string setting_path = "";
        private string libvlc_path = "";
        private string libvlccore_path = "";
        private string warning_pic_path = "";
        private string tmp_path = "";
        private string capture_path = "";
        private float temp_limit = 37.3f;
        private int pass_face = 0;
        private int noPass_face = 0;

        public bool blackcell_pos_error = false;
        public bool is_error_window_show = false;

        public CameraConfig cameras_config = null;
        public LocalSetting config = null;

        public AIThermometerAPP()
        {
            this.app_path = AppDomain.CurrentDomain.BaseDirectory;
            Console.WriteLine(this.app_path);
            this.camera_path = this.app_path + "cameras.json";
            this.setting_path = this.app_path + "setting.json";
            this.libvlc_path = this.app_path + "libvlc.dll";
            this.libvlccore_path = this.app_path + "libvlccore.dll";
            this.warning_pic_path = this.app_path + "face_noises";
            this.tmp_path = this.app_path + "tmp";
            this.capture_path = this.app_path + "capture";
            VoicePlayer.Instance().SetPath(this.app_path + "warning.wav");
            Common.CreateDir(this.warning_pic_path);
            Common.CreateDir(this.capture_path);
        }

        public float TempLimit()
        {
            return this.temp_limit;
        }

        public static AIThermometerAPP Instance()
        {
            if (instance == null)
            {
                lock (padlock)
                {
                    // 如果类的实例不存在则创建，否则直接返回
                    if (instance == null)
                    {
                        instance = new AIThermometerAPP();
                    }
                }
            }
            return instance;
        }

        public string AppPath()
        {
            return this.app_path;
        }

        public string SettingPath()
        {
            return this.setting_path;
        }

        public int PassFace
        {
            get { return this.pass_face; }
            set { this.pass_face = value; }
        }

        public int NoPassFace
        {
            get { return this.noPass_face; }
            set { this.noPass_face = value; }
        }

        public void AddPassFace() {
            this.pass_face++;
        }

        public void AddNoPassFace()
        {
            this.noPass_face++;
        }


        public string CameraPath()
        {
            return this.camera_path;
        }

        public string VlcPath()
        {
            return this.libvlc_path;
        }

        public string VlcCorePath()
        {
            return this.libvlccore_path;
        }

        public string WarningPath()
        {
            return this.warning_pic_path;
        }

        public string CapturePath()
        {
            return this.capture_path;
        }

        public bool CanCapture()
        {
            return ((this.noPass_face + this.pass_face) % 7 == 0);
        }

        public bool AutoStartCam()
        {
            return config.camera_auto_start;

        }

        public void SaveCameraConfigs()
        {
            string json = JsonHelper.ToJSON(this.cameras_config);
            File.WriteAllText(this.camera_path, json);
            Console.WriteLine("Save Camera Config.");
        }

        public void LoadConfigs()
        {
            string setting_json = LoadJson(this.setting_path);
            Console.WriteLine(setting_json);
            LocalSetting items = JsonHelper.FromJSON<LocalSetting>(setting_json);
            this.config = items;
        }

        public void SaveConfigs()
        {
            string json = JsonHelper.ToJSON(this.config);
            File.WriteAllText(this.setting_path, json);
            Console.WriteLine("Save Setting Config.");
        }


        public void LoadCameraConfigs()
        {
            Console.WriteLine("Find Camera setting config");
            string camera_json = LoadJson(this.camera_path);
            Console.WriteLine(camera_json);
            CameraConfig items = JsonHelper.FromJSON<CameraConfig>(camera_json);

            try
            {
                Console.WriteLine("Find " + items.Cameras.Count() + "cameras");
            }
            catch
            {

                Console.WriteLine("No camera info in setting file");
            }
            this.cameras_config = items;
        }

        

        public string LoadJson(string path)
        {
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                return json;
            }
        }

        public void ResetBlackCell()
        {
            new Thread(new ThreadStart(new Action(() =>
            {
                Thread.Sleep(5000);
                this.blackcell_pos_error = false;
            }))).Start();
        }


    }
}

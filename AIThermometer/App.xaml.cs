using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;
using System.IO;
using AIThermometer.Services;
using AIThermometer.Windows;
using AIThermometer.Cores;
using System.Net.NetworkInformation;
using System.Threading;

namespace AIThermometer
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {


        public App()
        {
            SplashScreen screen = new SplashScreen("/image/Logo_big.png");
            screen.Show(true);
            //string location_json = HttpRequestHelper.HttpGet1("http://192.168.0.119:9300/config");

            // LoadingWindow loading_window = new LoadingWindow();

            //if (loading_window.ShowDialog() == true)
            Setup();
            //loading_window.Close();
            NumInit();
            CheckMem();
            try
            {
                ServerHelper.Instance().port = AIThermometerAPP.Instance().config.local_port;
                //ServerHelper.Instance().temp_limit = AIThermometerAPP.Instance().TempLimit();
                ServerHelper.Instance().image_dir_path = AIThermometerAPP.Instance().WarningPath();
                ServerHelper.Instance().Initialize();
                LogHelper.WriteLog("Http Server init ok");

            }
            catch (Exception e)
            {
                LogHelper.WriteLog("Http Server init error", e);
            }
            

        }



        private void Application_Startup(object sender, StartupEventArgs e)
        {
            LogHelper.WriteLog("Language config loading start");
            string language = AIThermometerAPP.Instance().config.language;

            if (language == string.Empty || language == null)
            {
                //language = Thread.CurrentThread.CurrentCulture.Name == "zh-CN" ? "zh-CN" : "en-US";
                switch (Thread.CurrentThread.CurrentCulture.Name)
                {
                    case "zh-CN":
                        language = "zh-CN";
                        break;
                    case "en-US":
                        language = "en-US";
                        break;
                    case "ja-JP":
                        language = "ja-JP";
                        break;
                    case "ko-KR":
                        language = "ko-KR";
                        break;
                    default:
                        language = "en-US";
                        break;
                }
            }

            List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                dictionaryList.Add(dictionary);
            }
            string requestedCulture = string.Format(@"Resources\{0}.xaml", language);
            ResourceDictionary resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString.Equals(requestedCulture));
            if (resourceDictionary == null)
            {
                requestedCulture = @"Resources\en-US.xaml";
                resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString.Equals(requestedCulture));
            }
            if (resourceDictionary != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            }
            LogHelper.WriteLog("language config loading end");

        }

        private void Setup()
        {
            LogHelper.WriteLog("setup start");
            if (!Common.FileExists(AIThermometerAPP.Instance().VlcPath())
                || !Common.FileExists(AIThermometerAPP.Instance().VlcCorePath()))
            {
                //MessageBox.Show("VLC dll file error!");
            }


            if (!Common.FileExists(AIThermometerAPP.Instance().SettingPath()))
            {
                LocalSetting config = new LocalSetting();
                string json = JsonHelper.ToJSON(config);
                File.WriteAllText(AIThermometerAPP.Instance().SettingPath(), json);
                LogHelper.WriteLog("Cannot find local config file. Created it.");
            }
            AIThermometerAPP.Instance().LoadConfigs();

            if (!Common.FileExists(AIThermometerAPP.Instance().CameraPath()))
            {
                CameraConfig cameras = new CameraConfig();
                cameras.Cameras = new List<CameraInfo>();
                string json = JsonHelper.ToJSON(cameras);
                File.WriteAllText(AIThermometerAPP.Instance().CameraPath(), json);
                LogHelper.WriteLog("Cannot find cameras.json file. Created it.");
            }
            AIThermometerAPP.Instance().LoadCameraConfigs();

            // 检测cameras列表ping通
            Ping ping = new Ping();
            foreach (var c in AIThermometerAPP.Instance().cameras_config.Cameras)
            {
                Console.Write("Check " + c.Name + ":" + c.IP + " -- ");
                try
                {
                    if (ping.Send(c.IP, 2000).Status == IPStatus.Success)
                    {
                        LogHelper.WriteLog("ONLINE");
                        c.state = CamContectingState.ONLINE;
                    }
                    else
                    {
                        LogHelper.WriteLog("OFFLINE");
                        c.state = CamContectingState.OFFLINE;
                    }
                }
                catch (PingException e)
                {
                    LogHelper.WriteLog(e.Message);
                    c.state = CamContectingState.ERROR;
                }
            }
            LogHelper.WriteLog("setup end");


        }
        private void NumInit()
        {
            string basePath = AIThermometerAPP.Instance().WarningPath() + "\\" + DateTime.Now.ToString("yyMMdd");
            int com = GetFileCount(basePath + "\\pass");
            int high = GetFileCount(basePath + "\\nopass");
            int total = com + high;
            AIThermometerAPP.Instance().PassFace = com;
            AIThermometerAPP.Instance().NoPassFace = high;
        }
        public int GetFileCount(string path)
        {
            if (System.IO.Directory.Exists(path) == false)
            {
                return 0;
            }
            else
            {
                DirectoryInfo root = new DirectoryInfo(path);
                FileInfo[] files = root.GetFiles();
                return files.Count();
            }
        }

        private void CheckMem()
        {
            if (Directory.Exists(AIThermometerAPP.Instance().TmpPath()))
            {
                Common.ClearAllFiles(AIThermometerAPP.Instance().TmpPath());
                
            }
            Common.CreateDir(AIThermometerAPP.Instance().TmpPath());

            if (Directory.Exists(AIThermometerAPP.Instance().WarningPath()))
            {
                string[] directorieStrings = Directory.GetDirectories(AIThermometerAPP.Instance().WarningPath());
                DateTime limit = DateTime.Now.AddDays(-AIThermometerAPP.Instance().config.clean_day);
                foreach (string d in directorieStrings)
                {
                    string folder_name = Path.GetFileNameWithoutExtension(d);
                    DateTime ot = DateTime.ParseExact(folder_name, "yyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                    if (DateTime.Compare(limit, ot) > 0)
                    {
                        LogHelper.WriteLog("删除过期文件");
                        Common.ClearAllFiles(d);
                    }
                }
            }

        }
    }
}
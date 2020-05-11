using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AIThermometer;
using Newtonsoft.Json;
using AIThermometer.Cores;
using AIThermometer.Windows;
using AIThermometer.Services;
using System.Net.NetworkInformation;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Threading;
using System.Windows.Threading;
using System.Globalization;

namespace AIThermometer.Windows
{

    /// <summary>
    /// AppMainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AppMainWindow : Window
    {
        

        private int warning_bar_length = AIThermometerAPP.Instance().config.warning_bar_length;
        System.Timers.Timer aTimer;
        public AppMainWindow()
        {
            InitializeComponent();
            ServerHelper.Instance().ewHandler += ErrorWindowShow;
            ServerHelper.Instance().captureHandler += Shot;
            VclCamInit();
            
            TempWarning tmp = TempWarning.Instance();
            tmp.SetLength(warning_bar_length);
            tmp.addedWarningInfo += new TempWarning.AddedQueueEventHandler(AddTemp);
            aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new System.Timers.ElapsedEventHandler(dt_Tick);
            aTimer.Interval = 1000;//每秒执行一次
            aTimer.Enabled = true;
            aTimer.Start();
            SetLabel();

            CultureInfo ci = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;


        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (AIThermometerAPP.Instance().AutoStartCam())
            {
                if (CameraFactory.Instance().cl.Count > 0)
                    foreach (var ci in CameraFactory.Instance().cl)
                    {
                        this.vlcWindow.SetCamStream(ci);
                    }
            }

            //mc)

            this.vlcWindow.ChangeLeft();
            // 设置全屏
             this.WindowState = System.Windows.WindowState.Normal;
             this.WindowStyle = System.Windows.WindowStyle.None;
              this.ResizeMode = System.Windows.ResizeMode.NoResize;
            // this.Topmost = true;

            this.Left = 0.0;
            this.Top = 0.0;

           this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
           this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
        }

        public void WarningBarRefresh()
        {

        }

        // 以下为主页面代码
        /// <summary>
        /// VCL相机初始化
        /// </summary>
        private void VclCamInit()
        {
            LogHelper.WriteLog("vcl cam init start");
            foreach (var c in AIThermometerAPP.Instance().cameras_config.Cameras)
            {
                // 如果不是自动启动，所有相机的状态设为离线
                if (!AIThermometerAPP.Instance().AutoStartCam())
                {
                    c.state = CamContectingState.OFFLINE;
                }

                if (c.state == CamContectingState.ONLINE && 
                    AIThermometerAPP.Instance().AutoStartCam())
                {
                    // 如果摄像头Online 开始画面
                    if (CameraFactory.Instance().CreateCameraStream(c.Name, c.IP, c.StreamType) == null)
                    {
                        c.state = CamContectingState.OFFLINE;
                    }

                    // TODO 画面左侧的摄像头列表改变状态 在线 不在线 错误  需要完善
                    // TODO 状态从c。state获取, 名称也在c.name里。
                }
                CameraController cameraController = new CameraController(c);
                cameraController.SetConnectHandler(this.ConnectHandler, this.DisconnectHandler);
                listView.Items.Add(cameraController);
                ViewButtonStateChanged(c.state);

            }
            LogHelper.WriteLog("vcl cam init end : " + CameraFactory.Instance().cl.Count + " cameras ready!");
        }


        public void AddTemp(TempMessage tm)
        {

            Dispatcher.BeginInvoke(new Action(delegate
            {

                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    if ((listView1.Items[i] as WarningImage).id == tm.id)
                    {
                        listView1.Items.RemoveAt(i);
                        break;
                    }
                }

                int offset = listView1.Items.Count - warning_bar_length;

                while (offset >= 0)
                { 
                    listView1.Items.RemoveAt(0);
                    offset = listView1.Items.Count - warning_bar_length;
                }
                
                WarningImage warningImage = new WarningImage(tm);
                listView1.Items.Add(warningImage);
                listView1.SelectedItem = listView1.Items.GetItemAt(listView1.Items.Count - 1);
                listView1.ScrollIntoView(listView1.SelectedItem);
            }));


        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            //Application.Current.Shutdown();
            //Environment.Exit(-1);
            
            //Application.Current.Shutdown();
            //Environment.Exit(-1);
            this.Close();
        }

        private void ZoomButton_Click(object sender, RoutedEventArgs e)
        {

        }

        // 添加摄像头
        private void addCamButton_Click(object sender, RoutedEventArgs e)
        {
            AddCameraWindow ac = new AddCameraWindow();
            ac.ShowDialog();
            // 点击确定的话
            if (ac.DialogResult == true)
            {
                // 锁定添加
                if (AIThermometerAPP.Instance().cameras_config.Cameras.Count >= 1)
                {
                    ErrorWindow er = new ErrorWindow(Application.Current.FindResource("errorText").ToString(), Application.Current.FindResource("errorText1").ToString());
                    er.ShowDialog();
                    return;
                }

                Dispatcher.BeginInvoke(new Action(delegate
                {
                    CameraController cameraController = new CameraController(ac.GetCameraInfo());
                    listView.Items.Add(cameraController);
                }));
                // 储存camera信息到系统单例，然后保存到文件
                if (AIThermometerAPP.Instance().cameras_config.AddCam(ac.GetCameraInfo()))
                {
                    AIThermometerAPP.Instance().SaveCameraConfigs();
                    LogHelper.WriteLog("Added camerainfo to camera list. And saved!");
                }
                else
                {
                    // 添加不成功
                }
                //AIThermometerAPP.Instance().cameras_config.Cameras.Add(ac.GetCameraInfo());
                
            }

        }

        private void delCamButton_Click(object sender, RoutedEventArgs e)
        {
            
            CameraController camera = listView.SelectedItem as CameraController;
            if (camera==null)
            {
                return;
                //MessageBox.Show(Application.Current.FindResource("delCameraWarnText").ToString());
            }
            
            if (camera.ConnectState() == CamContectingState.ONLINE)
            {
                ErrorWindow er = new ErrorWindow(Application.Current.FindResource("errorText").ToString(), Application.Current.FindResource("pleaseDisconnect").ToString());
                er.ShowDialog();
                //MessageBox.Show(Application.Current.FindResource("delCameraWarnText").ToString());
                return;
            }
            MessageWindow mw = new MessageWindow(Application.Current.FindResource("delText").ToString(), Application.Current.FindResource("delText1").ToString());
            mw.ShowDialog();
            if (mw.DialogResult == true)
            {
                AIThermometerAPP.Instance().cameras_config.DeleteCamByName(camera.c_name);
                AIThermometerAPP.Instance().SaveCameraConfigs();
                listView.Items.Remove(camera);
            }
        }

        private void BlackcellButton_Click(object sender, RoutedEventArgs e)
        {
            string shot_path = AIThermometerAPP.Instance().TmpPath() + "\\" + Guid.NewGuid().ToString() + ".jpeg";
            if (!Shot(shot_path, CamMode.IR))
            {
                ErrorWindow ew = new ErrorWindow(Application.Current.FindResource("errorText").ToString(), Application.Current.FindResource("errorText2").ToString());
                ew.ShowDialog();
                return ;
            }

            BlackCellSettingWindow bc = new BlackCellSettingWindow(shot_path, vlcWindow.ip);
            if (bc.Init())
            {
                if (bc.ShowDialog() == true)
                {
                    Thread t = new Thread(new ThreadStart(new Action(() =>
                    {
                        PostToHW(bc.POSJSON, vlcWindow.ip);
                    }
                    )));
                    t.Start();
                }
            }
        }

        private static void PostToHW(object j, object i)
        {
            string ip = i as string;
            string json = j as string;
            var formDatas = new List<FormItemModel>();

            //添加文本
            formDatas.Add(new FormItemModel()
            {
                Key = "BlackCell-Position",
                Value = json // "id-test-id-test-id-test-id-test-id-test-"
            });

            //提交表单
            try
            {
                AIThermometerAPP.Instance().blackcell_pos_error = true;
                var result = FormPost.PostForm("http://" + ip + ":9300/config", formDatas);
                AIThermometerAPP.Instance().ResetBlackCell();

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("post to hw error", ex);
            }
        }

        private void CaptureFolderButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(AIThermometerAPP.Instance().WarningPath());//.CurrentDomain.BaseDirectory);
        }

        public bool Shot(string save_path, CamMode mode)
        {
            bool res = vlcWindow.Shot(save_path, mode);
            return res;// AIThermometerAPP.Instance().AppPath() + "\\snapshot.dat", CamMode.IR);
        }

        void dt_Tick(object sender, EventArgs e)
        {
            SetLabel();
        }

        public void SetLabel()
        {
            int com = AIThermometerAPP.Instance().PassFace;
            int high = AIThermometerAPP.Instance().NoPassFace;
            int total = com + high;
            Dispatcher.BeginInvoke(new Action(delegate
            {
                commonLabel.Content = Application.Current.FindResource("tempNormal").ToString() + com;
                highLabel.Content = Application.Current.FindResource("tempHigh").ToString() + high;
                totalLabel.Content = Application.Current.FindResource("temptotal").ToString() + total;
            }));


        }

        private void VclBtn_Click(object sender, RoutedEventArgs e)
        {
            vlcWindow.ChangeLeft();
        }

        private void vclRightBtn_Click(object sender, RoutedEventArgs e)
        {
            vlcWindow.ChangeRight();
        }

        private void VclBoth_Click(object sender, RoutedEventArgs e)
        {
            vlcWindow.ChangeBoth();
        }

        private void Left_Button_Click(object sender, RoutedEventArgs e)
        {
            if (listView1.Items.Count <= 0) {
                return;
            }
            var sel= listView1.SelectedItem;
            if (sel == null)
            {
                listView1.SelectedItem = listView1.Items.GetItemAt(listView1.Items.Count - 1);
                listView1.ScrollIntoView(listView1.SelectedItem);

            }
            else
            {
                int i = listView1.SelectedIndex+1;
                if (i> listView1.Items.Count - 1)
                {
                    return;
                }
                else
                {
                    listView1.SelectedItem = listView1.Items.GetItemAt(i);
                    listView1.ScrollIntoView(listView1.SelectedItem);
                }
            }
        }

        private void RighitButton_Click(object sender, RoutedEventArgs e)
        {
            if (listView1.Items.Count <= 0)
            {
                return;
            }
            var sel = listView1.SelectedItem;
            if (sel == null)
            {
                listView1.SelectedItem = listView1.Items.GetItemAt(0);
                listView1.ScrollIntoView(listView1.SelectedItem);

            }
            else
            {
                int i = listView1.SelectedIndex -1;
                if (i<0)
                {
                    return;
                }
                else
                {
                    listView1.SelectedItem = listView1.Items.GetItemAt(i);
                    listView1.ScrollIntoView(listView1.SelectedItem);
                }
            }
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow settingWindow = new SettingWindow();
            settingWindow.ShowDialog();
            if (settingWindow.DialogResult == true)
            {
                TempWarning.Instance().SetLength(AIThermometerAPP.Instance().config.warning_bar_length);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach(var a in CameraFactory.Instance().cl)
            {
                foreach(var b in a)
                {
                    b.Value.Stop();
                    //(b.Value as VlcCamera).vlcPlayer.Dispose();
                }
            }
            CameraFactory.Instance().cl.Clear();
            this.vlcWindow.Shutdown();

            ServerHelper.Instance().Dispose();
            //TempWarning.Instance().addedWarningInfo = null;
            //;aTimer.Close();
            LogHelper.WriteLog("Closing APP");
            Environment.Exit(0);
            //base.OnClosing();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
             if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftAlt) && Keyboard.IsKeyDown(Key.C))
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;//设置为选择文件夹
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    Thread ct = new Thread((CopyDirectory));
                    List<string> paths = new List<string>();
                    paths.Add(AIThermometerAPP.Instance().CapturePath());
                    paths.Add(AIThermometerAPP.Instance().WarningPath());
                    paths.Add(dialog.FileName);
                    ct.Start(paths);
                    //Common.CopyDirectory(AIThermometerAPP.Instance().WarningPath(), dialog.FileName);
                }
            }
        }

        private void CopyDirectory(object paths)
        {
            string desPath = (paths as List<string>)[(paths as List<string>).Count -1];
            for (int i = 0; i < (paths as List<string>).Count; i++)
            {
                string srcPath = (paths as List<string>)[i];
                string folderName = srcPath.Substring(srcPath.LastIndexOf("\\") + 1);
                string desfolderdir = desPath + "\\" + folderName;
                if (srcPath.LastIndexOf("\\") == (desPath.Length - 1))
                {
                    desfolderdir = desPath + folderName;
                }
                string[] filenames = Directory.GetFileSystemEntries(srcPath);
                foreach (string file in filenames)
                {
                    if (Directory.Exists(file))
                    {
                        string currentdir = desfolderdir + "\\" + file.Substring(file.LastIndexOf("\\") + 1);
                        if (!Directory.Exists(currentdir))
                        {
                            Directory.CreateDirectory(currentdir);
                        }
                        Common.CopyDirectory(file, desfolderdir);
                    }
                    else
                    {
                        string srcfileName = file.Substring(file.LastIndexOf("\\") + 1);
                        srcfileName = desfolderdir + "\\" + srcfileName;
                        if (!Directory.Exists(desfolderdir))
                        {
                            Directory.CreateDirectory(desfolderdir);
                        }

                        File.Copy(file, srcfileName);
                    }
                }
            }
            Dispatcher.BeginInvoke(new Action(delegate
            {
                ErrorWindow ew = new ErrorWindow(Application.Current.FindResource("updateText1").ToString(), Application.Current.FindResource("updateText2").ToString());
                ew.Show();
            }));

            
        }
        ErrorWindow er;

        public void ErrorWindowShow(string ti, string me)
        {
            //er = new ErrorWindow(ti, me);
            //StartCloseTimer();
//            er.Show();
            App.Current.Dispatcher.Invoke((Action)(() =>
            {
                if (!AIThermometerAPP.Instance().is_error_window_show)
                {
                    ErrorWindow er = new ErrorWindow(ti, me, true);// "错误", "当前版本暂时只支持一个设备");
                    AIThermometerAPP.Instance().is_error_window_show = true;
                    er.Show();
                }
            }));
        }  

        public void ConnectHandler(string ip)
        {
            foreach (var c in AIThermometerAPP.Instance().cameras_config.Cameras)
            {
                if (c.IP == ip)
                {
                    var mc = CameraFactory.Instance().CreateCameraStream(c.Name, c.IP, c.StreamType);
                    if (mc != null)
                    {
                        c.state = CamContectingState.ONLINE;
                        this.vlcWindow.SetCamStream(mc);
                        LogHelper.WriteLog("Connect handler cl count :" +CameraFactory.Instance().cl.Count);
                    }
                }
                ViewButtonStateChanged(c.state);
            }
            this.vlcWindow.ChangeLeft();
        }

        public void DisconnectHandler(string ip)
        {

            this.vlcWindow.DelCamStream();
            CameraFactory.Instance().DelCamStream(ip);
            foreach (var c in AIThermometerAPP.Instance().cameras_config.Cameras)
            {
                c.state = CamContectingState.OFFLINE;
                ViewButtonStateChanged(c.state);
            }
        }

        private void ViewButtonStateChanged(CamContectingState ccs)
        {
            switch(ccs)
            {
                case CamContectingState.ONLINE:
                    this.vclLeftBtn.IsEnabled = true;
                    this.vclRightBtn.IsEnabled = true;
                    this.vclBoth.IsEnabled = true;
                    this.blackCellBtn.IsEnabled = true;
                    break;
                case CamContectingState.OFFLINE:
                case CamContectingState.ERROR:
                    this.vclLeftBtn.IsEnabled = false;
                    this.vclRightBtn.IsEnabled = false;
                    this.vclBoth.IsEnabled = false;
                    this.blackCellBtn.IsEnabled = false;
                    break;
                default:
                    break;
            }
        }
        
    }
}

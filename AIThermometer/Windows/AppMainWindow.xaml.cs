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
            tmp.addedWarningInfo += new TempWarning.AddedQueueEventHandler(AddTemp);
            aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new System.Timers.ElapsedEventHandler(dt_Tick);
            aTimer.Interval = 1000;//每秒执行一次
            aTimer.Enabled = true;
            aTimer.Start();
            SetLabel();
            
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (AIThermometerAPP.Instance().AutoStartCam())
            {
                if (CameraFactory.Instance().cl.Count > 0)
                    this.vlcWindow.SetCamStream(CameraFactory.Instance().cl[0]);
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
            List<CameraController> cameraControllers = new List<CameraController>();
            foreach (var c in AIThermometerAPP.Instance().cameras_config.Cameras)
            {
                if (c.state == CamContectingState.ONLINE && 
                    AIThermometerAPP.Instance().AutoStartCam())
                {
                    // 如果摄像头Online 开始画面
                    if (!CameraFactory.Instance().CreateCameraStream(c.Name, c.IP, c.StreamType))
                    {
                        c.state = CamContectingState.OFFLINE;
                    }

                    // TODO 画面左侧的摄像头列表改变状态 在线 不在线 错误  需要完善
                    // TODO 状态从c。state获取, 名称也在c.name里。
                }
                CameraController cameraController = new CameraController(c);
                listView.Items.Add(cameraController);
            }
            Console.WriteLine(CameraFactory.Instance().cl.Count + " cameras ready!");
        }


        public void AddTemp(TempMessage tm)
        {

            Dispatcher.BeginInvoke(new Action(delegate
            {
                if (listView1.Items.Count == warning_bar_length) {
                    listView1.Items.RemoveAt(0);
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
                    ErrorWindow er = new ErrorWindow("错误", "当前版本暂时只支持一个设备");
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
                    Console.WriteLine("Added camerainfo to camera list. And saved!");
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
                MessageBox.Show("请选择需要删除的摄像头");
            }
            MessageWindow mw = new MessageWindow("删除设备", "您确定要删除设备吗?");
            if (mw.DialogResult == true)
            {
                AIThermometerAPP.Instance().cameras_config.DeleteCamByName(camera.Name);
                AIThermometerAPP.Instance().SaveCameraConfigs();
                listView.Items.Remove(camera);
            }
        }

        private void BlackcellButton_Click(object sender, RoutedEventArgs e)
        {
            string shot_path = AIThermometerAPP.Instance().AppPath() + "\\snapshot.jpeg";
            if (!Shot(shot_path, CamMode.IR))
            {
                ErrorWindow ew = new ErrorWindow("错误", "视频流截取错误,请重试");
                ew.ShowDialog();
                return ;
            }
        
            BlackCellSettingWindow bc = new BlackCellSettingWindow(shot_path, vlcWindow.ip);
            if (bc.Init())
            {
                if (bc.ShowDialog() == true)
                {
                    AIThermometerAPP.Instance().ResetBlackCell();
                }
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
                commonLabel.Content = "温度正常" + com + "人";
                highLabel.Content = "温度异常" + high + "人";
                totalLabel.Content = "今天共检测" + total + "人";
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
            vlcWindow.RunCheck(false);
            ServerHelper.Instance().Dispose();
            //TempWarning.Instance().addedWarningInfo = null;
            //;aTimer.Close();
            Console.WriteLine("Closing APP");
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
                ErrorWindow ew = new ErrorWindow("升级操作", "升级操作完成");
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
    }
}

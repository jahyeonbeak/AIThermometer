using AIThermometer.Cores;
using AIThermometer.Services;
using AIThermometer.Windows;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AIThermometer
{
    /// <summary>
    /// CameraController.xaml 的交互逻辑
    /// </summary>
    public partial class CameraController : UserControl
    {
        GridLength heigh = new GridLength(64);
        CameraInfo _cameraInfo;
        ImageBrush openIB;

        ImageBrush closeIB;

        public delegate void ConnectHandlerEvent(string ip);
        public ConnectHandlerEvent ConnectHandler = null;
        public ConnectHandlerEvent DisconnectHandler = null;
        public string c_name = "";

        public CameraController(CameraInfo cameraInfo)
        {
            InitializeComponent();
            openIB = new ImageBrush();
            closeIB = new ImageBrush();
            _cameraInfo = cameraInfo;
            cameraName.Content = cameraInfo.Name;
            ChangedState(cameraInfo.state);
            c_name = cameraInfo.Name;


            Uri uri1 = new Uri(@"pack://application:,,,/image/down.png", UriKind.Absolute);
            closeIB.ImageSource = new BitmapImage(uri1);
            Uri uri2 = new Uri(@"pack://application:,,,/image/up.png", UriKind.Absolute);
            openIB.ImageSource = new BitmapImage(uri2);
            mainGrid.RowDefinitions[1].Height = new GridLength();
            this.Height = 47;
            label1.Width = 323;
        }

        public CamContectingState ConnectState()
        {
            return _cameraInfo.state;
        }

        public void SetConnectHandler(ConnectHandlerEvent ec, ConnectHandlerEvent ed)
        {
            ConnectHandler += ec;
            DisconnectHandler += ed;
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            GridLength temp = mainGrid.RowDefinitions[1].Height;
            GridLength def = new GridLength();
            if (temp == def)
            {
                //展开
                mainGrid.RowDefinitions[1].Height = heigh;
                settingButton.Background = closeIB;
                this.Height = 111;
                label1.Width = 270;
            }
            else
            {
                //折叠
                mainGrid.RowDefinitions[1].Height = def;
                settingButton.Background = openIB;
                this.Height = 47;
                label1.Width = 323;
            }
        }
        
        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (_cameraInfo.state == CamContectingState.ONLINE)
            {
                UpdateCameraWindow uc = new UpdateCameraWindow(_cameraInfo);
                
                uc.ShowDialog();
                // 点击确定的话
                if (uc.DialogResult == true)
                {
                    CameraInfo cameraInfo = uc.GetCameraInfo();
                    // TODO POST 3个信息。

                    cameraName.Content = cameraInfo.Name;

                    Thread t = new Thread(new ThreadStart(new Action(() =>
                    {
                        PostToHW(uc.FI, cameraInfo.IP);
                    }
                    )));
                    t.Start();
                }
            }
            else
            {
                UpdateCameraWindow uc = new UpdateCameraWindow(_cameraInfo);
                uc.ShowDialog();

            }
          
        }

        private static void PostToHW(object j, object i)
        {
            string ip = i as string;
            var formDatas = j as List<FormItemModel>;

            //提交表单
            try
            {
                var result = FormPost.PostForm("http://" + ip + ":9300/config", formDatas);

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("update camera error", ex);
            }
        }

        private void contectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cameraInfo.state == CamContectingState.OFFLINE)
            {
                Ping ping = new Ping();

                try
                {
                    if (ping.Send(_cameraInfo.IP, 2000).Status == IPStatus.Success)
                    {
                        if (ConnectHandler != null)
                        {
                            ConnectHandler(_cameraInfo.IP);
                            _cameraInfo.state = CamContectingState.ONLINE;
                            ChangedState(_cameraInfo.state);
                        }
                    }
                    else
                    {
                        LogHelper.WriteLog("Connect to device. device offline");
                        ErrorWindow mw = new ErrorWindow(Application.Current.FindResource("errorText").ToString(), Application.Current.FindResource("nodevice").ToString());
                        mw.ShowDialog();
                    }
                }
                catch(Exception ex)
                {
                    LogHelper.WriteLog("Connect to device error", ex);
                    ErrorWindow mw = new ErrorWindow(Application.Current.FindResource("errorText").ToString(), Application.Current.FindResource("nodevice").ToString());
                    mw.ShowDialog();
                }
                
            }
            else
            {
                if (DisconnectHandler != null)
                {
                    DisconnectHandler(_cameraInfo.IP);
                    _cameraInfo.state = CamContectingState.OFFLINE;
                    ChangedState(_cameraInfo.state);
                }
            }
        }

        private void ChangedState(CamContectingState cs)
        {
            _cameraInfo.state = cs;
            Uri uri;
            Uri buri;
            ImageBrush ib;
            ImageBrush bib;
            switch (_cameraInfo.state)
            {
                case CamContectingState.OFFLINE:
                    uri = new Uri(@"/image/camera_close.png", UriKind.Relative);
                    ib = new ImageBrush();
                    ib.ImageSource = new BitmapImage(uri);
                    image.Source = ib.ImageSource;
                    connectLabel.Text = Application.Current.FindResource("conCamera").ToString();

                    buri = new Uri(@"/image/lian.png", UriKind.Relative);
                    bib = new ImageBrush();
                    bib.ImageSource = new BitmapImage(buri);
                    imageButton.Source = bib.ImageSource;

                    break;

                case CamContectingState.ONLINE:
                    uri = new Uri(@"/image/camera.png", UriKind.Relative);
                    ib = new ImageBrush();
                    ib.ImageSource = new BitmapImage(uri);
                    image.Source = ib.ImageSource;
                    connectLabel.Text = Application.Current.FindResource("disCamera").ToString();

                    buri = new Uri(@"/image/duan.png", UriKind.Relative);
                    bib = new ImageBrush();
                    bib.ImageSource = new BitmapImage(buri);
                    imageButton.Source = bib.ImageSource;
                    
                    break;

                default:
                    break;
            }
        }
    }
}

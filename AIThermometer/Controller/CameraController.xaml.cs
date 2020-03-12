using AIThermometer.Cores;
using AIThermometer.Services;
using AIThermometer.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public CameraController(CameraInfo cameraInfo)
        {
            InitializeComponent();
            openIB = new ImageBrush();
            closeIB = new ImageBrush();
            _cameraInfo = cameraInfo;
            cameraName.Content = cameraInfo.Name;
            if (cameraInfo.state == CamContectingState.ONLINE)
            {
                Uri uri = new Uri(@"/image/camera.png", UriKind.Relative);
                ImageBrush ib = new ImageBrush();
                ib.ImageSource = new BitmapImage(uri);
                image.Source = ib.ImageSource;
            }
            else
            {
                Uri uri = new Uri(@"/image/camera_close.png", UriKind.Relative);
                ImageBrush ib = new ImageBrush();
                ib.ImageSource = new BitmapImage(uri);
                image.Source = ib.ImageSource;
            }
            Uri uri1 = new Uri(@"pack://application:,,,/image/down.png", UriKind.Absolute);
            closeIB.ImageSource = new BitmapImage(uri1);
            Uri uri2 = new Uri(@"pack://application:,,,/image/up.png", UriKind.Absolute);
            openIB.ImageSource = new BitmapImage(uri2);
            mainGrid.RowDefinitions[1].Height = new GridLength();
            this.Height = 47;
            label1.Width = 323;

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
            try
            {
                HttpRequestHelper http_request = new HttpRequestHelper();
                string json = http_request.HttpGet("http://" + _cameraInfo.IP + ":9300/config", "");
                CameraResponse cr = JsonHelper.FromJSON<CameraResponse>(json);
                _cameraInfo.BlackCell_Temp = cr.SavedParams.BlackCell_Temperature.ToString();
                _cameraInfo.Report_URL = cr.SavedParams.Report_URL;
                _cameraInfo.Device_Name = cr.DEVICE;
                _cameraInfo.Camera_Threshold = cr.SavedParams.Camera_Threshold.ToString();
                _cameraInfo.Face_LimitSize = cr.SavedParams.Face_LimitSize.ToString();
                _cameraInfo.Face_Score = cr.SavedParams.Face_Score.ToString();
            }
            catch
            {
                return;
            }

            UpdateCameraWindow uc = new UpdateCameraWindow(_cameraInfo);
            uc.ShowDialog();
            // 点击确定的话
            if (uc.DialogResult == true)
            {
                CameraInfo cameraInfo = uc.GetCameraInfo();
                // TODO POST 3个信息。

                cameraName.Content = cameraInfo.Name;

            }
        }

        private void contectButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(_cameraInfo.IP);
            ChangedState(_cameraInfo.state);
        }

        public void Connect()
        {
            //if (CameraFactory.Instance().GetCameraByName())
        }

        public void ChangedState(CamContectingState cs)
        {
            _cameraInfo.state = cs;
            switch (_cameraInfo.state)
            {
                case CamContectingState.OFFLINE:

                    // 图片切换为离线图片
                    // 按钮切换为连接
                    break;
                case CamContectingState.ONLINE:
                    // 图片切换为在线图片
                    // 按钮切换为断开
                    break;
                default:
                    break;
            }
        }
    }
}

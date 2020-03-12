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
using System.Windows.Shapes;
using AIThermometer.Cores;
using AIThermometer.Services;

namespace AIThermometer.Windows
{
    /// <summary>
    /// AddCameraWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddCameraWindow : Window
    {
        private CameraInfo ci = new CameraInfo();

        public AddCameraWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Topmost = true;
        }

        public AddCameraWindow(CameraInfo cameraInfo)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Topmost = true;
            cameraName.Text = cameraInfo.Name;
            ip.Text = cameraInfo.IP;
            title.Content = "添加摄像头";
            confirm.Content = "修改";
        }



        public CameraInfo GetCameraInfo()
        {
            return ci;
        }


        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            // 设备名称和设备ip传入以下ci
            // IP 验证通过Common.cs文件里的正则函数验证
            // ci。name 是 前字母（中文）后数字 没有特殊符号的正则，需要在Common.cs实现
            ci.Name = cameraName.Text;
            /*
            if (!Common.IPMatch(ip.Text))
            {
                ErrorWindow er = new ErrorWindow("错误", "ip地址格式不正确");
                return;
            }*/
            ci.IP = ip.Text;
            ci.Date = DateTime.Now;
            ci.BlackCell_Temp = "37";
            ci.Report_URL = "http://192.168.0.112/Report";
            ci.Device_Name = "0.15562";
            
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

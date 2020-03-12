using AIThermometer.Cores;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AIThermometer.Windows
{
    /// <summary>
    /// WarningImage.xaml 的交互逻辑
    /// </summary>
    public partial class WarningImage : UserControl
    {
        private float redTemp = AIThermometerAPP.Instance().config.temp_limit;
        public WarningImage(TempMessage tm)
        {
            InitializeComponent();
            Init(tm);


        }

        public void Init(TempMessage tm)
        {
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.StreamSource = new MemoryStream(tm.bytes);
            myBitmapImage.EndInit();
            image.Source = myBitmapImage;
            cam_name.Content = "设备名称：" + tm.cam;
            temp.Content = "体温:" + tm.temp;
            date.Content = "测温时间:" + tm.date;
            if (tm.temp > redTemp)
            {
                redImage.Visibility = Visibility.Visible;
                temp.Foreground = new SolidColorBrush(Color.FromRgb(246, 111, 106));
                temp.Margin = new Thickness(32, 20, 0, 0);
            }
            else
            {
                redImage.Visibility = Visibility.Hidden;
                temp.Foreground = new SolidColorBrush(Color.FromRgb(24, 144, 255));
                temp.Margin = new Thickness(20, 20, 0, 0);
            }
        }

    }
}

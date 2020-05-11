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
        public int id = 0;
        public WarningImage(TempMessage tm)
        {
            InitializeComponent();
            Init(tm);


        }

        public void Init(TempMessage tm)
        {
            id = tm.id;
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.StreamSource = new MemoryStream(tm.bytes);
            myBitmapImage.EndInit();
            image.Source = myBitmapImage;
            cam_name.Content = Application.Current.FindResource("deviceName").ToString() + tm.cam;
            temp.Content = Application.Current.FindResource("devicetemp").ToString() + tm.temp;
            date.Content = Application.Current.FindResource("tempTime").ToString() + tm.date;
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

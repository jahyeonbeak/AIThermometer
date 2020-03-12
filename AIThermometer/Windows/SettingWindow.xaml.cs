using AIThermometer.Cores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AIThermometer.Windows
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Topmost = true;
            LocalSetting localSetting = AIThermometerAPP.Instance().config;
            ip.Text= localSetting.local_url;
            port.Text= localSetting.local_port.ToString();
            warn_number.Text= localSetting.warning_bar_length.ToString();
            threshold.Text = localSetting.temp_limit.ToString();
            clean_day.Text = localSetting.clean_day.ToString();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
           
            LocalSetting localSetting = new LocalSetting();
            localSetting.local_url = ip.Text;
            localSetting.local_port = int.Parse(port.Text);
            localSetting.warning_bar_length = int.Parse(warn_number.Text);
            localSetting.temp_limit =float.Parse(threshold.Text);
            localSetting.clean_day = int.Parse(threshold.Text);

            AIThermometerAPP.Instance().config = localSetting;
            AIThermometerAPP.Instance().SaveConfigs();
            DialogResult = true;
        }

        private void tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");

            e.Handled = re.IsMatch(e.Text);
        }
        private void tb_PreviewTextInputFloat(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex(@"^-?([1-9]\d*\.\d*|0\.\d*[1-9]\d*|0?\.0+|0)$");

            e.Handled = re.IsMatch(e.Text);
        }



    }
}

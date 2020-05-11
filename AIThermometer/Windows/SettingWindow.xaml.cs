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

        public string _lanage = string.Empty;

        public SettingWindow()
        {
            InitializeComponent();

            app_version.Content = "Version: " + AIThermometerAPP.Instance().GetVersion();

            List<lanageClass> dicItem = new List<lanageClass>();
            dicItem.Add(new lanageClass() { key = "en-US", value = "English" });
            dicItem.Add(new lanageClass() { key = "zh-CN", value = "中文" });
            dicItem.Add(new lanageClass() { key = "ja-JP", value = "日本語" });
            //dicItem.Add(new lanageClass() { key = "ko-KR", value = "한국어" });
            lanageCombo.ItemsSource = dicItem;


            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Topmost = true;
            LocalSetting localSetting = AIThermometerAPP.Instance().config;
            ip.Text= localSetting.local_url;
            port.Text= localSetting.local_port.ToString();
            warn_number.Text= localSetting.warning_bar_length.ToString();
            threshold.Text = localSetting.temp_limit.ToString();
            clean_day.Text = localSetting.clean_day.ToString();
            autoconCheckBox.IsChecked = localSetting.camera_auto_start;

            string lange = localSetting.language;

            switch (lange)
            {
                case "zh-CN":
                    lanageCombo.SelectedIndex = 1;
                    break;
                case "en-US":
                    lanageCombo.SelectedIndex = 0;
                    break;
                case "ja-JP":
                    lanageCombo.SelectedIndex = 2;
                    break;
                case "ko-KR":
                    lanageCombo.SelectedIndex = 3;
                    break;
                default:
                    lanageCombo.SelectedIndex = 0;
                    break;

            }
            
            _lanage = localSetting.language;

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
            try
            {
                localSetting.local_port = int.Parse(port.Text);
                int wn = int.Parse(warn_number.Text);
                int cd = int.Parse(clean_day.Text);
                float ts = float.Parse(threshold.Text);
                localSetting.camera_auto_start = (bool)autoconCheckBox.IsChecked;
                if (wn > 30 || wn < 10)
                {
                    ErrorWindow er = new ErrorWindow("Value error", "Our of range.(10-30)");
                    er.ShowDialog();
                    return;
                }
                else if (cd < 15 || cd > 180)
                {
                    ErrorWindow er = new ErrorWindow("Value error", "Our of range.(15-180)");
                    er.ShowDialog();
                    return;
                }
                else if (ts < 35 || ts > 42)
                {
                    ErrorWindow er = new ErrorWindow("Value error", "Our of range.(35-42)");
                    er.ShowDialog();
                    return;
                }

                localSetting.warning_bar_length = wn;
                localSetting.temp_limit = ts;
                localSetting.clean_day = cd;
            }
            catch
            {
                ErrorWindow er = new ErrorWindow("Value error", "Input value error, Please check.");
                er.ShowDialog();
                return;
            }
            lanageClass dic = lanageCombo.SelectedItem as lanageClass;
            if (dic != null)
            {
                localSetting.language = dic.key;
            }
            ErrorWindow ew = null;
            if (localSetting.language != _lanage)
            {
                 ew = new ErrorWindow(System.Windows.Application.Current.FindResource("warn").ToString(), System.Windows.Application.Current.FindResource("warn1").ToString());
            }
            AIThermometerAPP.Instance().config = localSetting;
            AIThermometerAPP.Instance().SaveConfigs();
            if (ew == null)
            {
                DialogResult = true;
                return;
            }
            ew.ShowDialog();
            System.Windows.Forms.Application.Restart();
            System.Windows.Application.Current.Shutdown();
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

        public class lanageClass
        {
            public string key { get; set; }

            public string value { get; set; }
        }
    }
}

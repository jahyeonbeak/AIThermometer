using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AIThermometer.Windows
{
    /// <summary>
    /// ErrorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorWindow : Window
    {
        bool blackcell_error = false;
        public ErrorWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Topmost = true;

            
        }

        public ErrorWindow(string _title, string _content, bool bc = false)
        {
            InitializeComponent();
            this.title.Content = _title;
            this.content.Content = _content;
            this.blackcell_error = bc;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Topmost = true;
            
        }

        private void CloseWindow()
        {

            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.blackcell_error)
                AIThermometerAPP.Instance().is_error_window_show = false;
            this.Close();
            //DialogResult = true;
        }


    }
}

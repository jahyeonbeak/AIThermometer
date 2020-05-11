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
using System.IO;
using System.ComponentModel;

namespace AIThermometer.Windows
{
    /// <summary>
    /// AddCameraWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FirmwareUpdateWindow : Window
    {
        private CameraInfo ci;
        private new List<FormItemModel> fi;
        bool isFileUpgrade;

        public FirmwareUpdateWindow(CameraInfo _ci)
        {
            InitializeComponent();
            ci = _ci;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Topmost = true;
            // device.Text = ci.Name;
            versionCombo.IsEnabled = false;
            isFileUpgrade = true;
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                bw.DoWork += new DoWorkEventHandler(bw_DoWork);
                bw.RunWorkerAsync(ci.IP);
            }
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {

            string ip = e.Argument as string;
            var formDatas = new List<FormItemModel>();

            // 文件名
            formDatas.Add(new FormItemModel()
            {
                Key = "",
                Value = "",
            });

            try
            {
                //提交表单
                var result = FormPost.PostForm("http://" + ip + ":9301/Patch/list", null);
                //cameraInfo.IP = ip.Text;
                FirmwareResponse cr = JsonHelper.FromJSON<FirmwareResponse>(result);

                Console.WriteLine(result);
                LogHelper.WriteLog(result);
                e.Result = cr;

            }
            catch (Exception)
            {
                e.Result = null;
            }


        }

        public class versionClass
        {
            public string key { get; set; }

            public string value { get; set; }
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null)
                return;

            var cr = e.Result as FirmwareResponse;

            List<versionClass> dicItem = new List<versionClass>();
            if (cr.Patchs.Count > 0)
            {
                foreach (string vr in cr.Patchs)
                {
                    dicItem.Add(new versionClass() { value = vr.Split('/').Last<string>(), key = vr });
                }
            }
            versionCombo.ItemsSource = dicItem;
            foreach (var di in dicItem)
            {
                if (cr.Message == di.key)
                    versionCombo.SelectedItem = di;
            }
            //versionCombo.SelectedIndex = 0;//.SelectedValue = cr.Message.Split('/').Last<string>();
            //cr.Message;
        }

        public CameraInfo GetCameraInfo()
        {
            return ci;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isFileUpgrade)
            {
                var sm = versionCombo.SelectedValue as string;
                var formDatas = new List<FormItemModel>();

                // 温度系数
                formDatas.Add(new FormItemModel()
                {
                    Key = "Patch",
                    Value = sm // "id-test-id-test-id-test-id-test-id-test-"
                });

                var result = FormPost.PostForm("http://" + ci.IP + ":9301/Patch/apply", formDatas);
                DialogResult = true;
            }
            else
            {
                DialogResult = true;
            }
            

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void FilePath_Click(object sender, RoutedEventArgs e)
        {
            string file_path;
            System.Windows.Forms.OpenFileDialog op = new System.Windows.Forms.OpenFileDialog();
            op.Multiselect = false;
            op.AddExtension = true;
            op.DereferenceLinks = true;
            if (op.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                file_path = op.FileName;
                if (UpdateCameraFW(op.SafeFileName, op.FileName))
                {
                    MessageBox.Show("update ok");
                    this.DialogResult = false;
                }
            }
            else
            {
                ErrorWindow er = new ErrorWindow(Application.Current.FindResource("upError").ToString(), Application.Current.FindResource("error6").ToString());
                er.ShowDialog();
                this.DialogResult = false;
            }
        }

        private bool UpdateCameraFW(string file_name, string update_fpath)
        {
            if (ci.state == CamContectingState.ONLINE)
            {

                var formDatas = new List<FormItemModel>();

                // 文件名
                formDatas.Add(new FormItemModel()
                {
                    Key = "File",
                    Value = "",
                    FileName = file_name,
                    FileContent = File.OpenRead(update_fpath)


                });

                try
                {
                    //提交表单
                    var result = FormPost.PostForm("http://" + ci.IP + ":9301/Update", formDatas);
                    //cameraInfo.IP = ip.Text;

                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;

        }

        public new List<FormItemModel> FI { get { return fi; } set { fi = value; } }

        private void radioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton btn = sender as RadioButton;
            if (btn == null)
                return;
            if (btn.Name == "fileRadio")
            {
                isFileUpgrade = true;
                versionCombo.IsEnabled = false;
                openFileButton.IsEnabled = true;
            }
            if (btn.Name == "memRadio")
            {
                isFileUpgrade = false;
                versionCombo.IsEnabled = true;
                openFileButton.IsEnabled = false;
            }
        }

    }
}

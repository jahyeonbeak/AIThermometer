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
    public partial class UpdateCameraWindow : Window
    {
        private CameraInfo ci;
        private new List<FormItemModel> fi;

        public UpdateCameraWindow(CameraInfo _ci)
        {
            InitializeComponent();
            ci = _ci;
            title.Content = ci.Name;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Topmost = true;
            ip.Text = ci.IP;
            version_label.Content = "";
            // device.Text = ci.Name;

            switch (ci.state)
            {
                case CamContectingState.OFFLINE:
                    ip.IsEnabled = true;
                    device.IsEnabled = false;

                    balckTemp.IsEnabled = false;
                    address.IsEnabled = false;
                    camera_warn_limit.IsEnabled = false;
                    face_limit.IsEnabled = false;
                    face_score.IsEnabled = false;
                    temp_value.IsEnabled = false;
                    temp_check.IsEnabled = false;
                    break;
                case CamContectingState.ONLINE:
                    ip.IsEnabled = false;
                    device.IsEnabled = false;

                    balckTemp.IsEnabled = true;
                    address.IsEnabled = true;
                    camera_warn_limit.IsEnabled = true;
                    face_limit.IsEnabled = true;
                    face_score.IsEnabled = true;
                    temp_value.IsEnabled = false;
                    temp_check.IsEnabled = true;
                    using (BackgroundWorker bw = new BackgroundWorker())
                    {
                        bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                        bw.DoWork += new DoWorkEventHandler(bw_DoWork);
                        bw.RunWorkerAsync(ci.IP);
                    }
                    break;
                default:
                    break;
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
                var result = FormPost.PostForm("http://" + ip + ":9300/config", null);
                //cameraInfo.IP = ip.Text;
                CameraResponse cr = JsonHelper.FromJSON<CameraResponse>(result);

                Console.WriteLine(result);
                LogHelper.WriteLog(result);
                e.Result = cr;

            }
            catch (Exception)
            {
                e.Result = null;
            }


        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null)
                return;

            var cr = e.Result as CameraResponse;
            balckTemp.Text = cr.SavedParams.BlackCell_Temperature.ToString();
            address.Text = cr.SavedParams.Report_URL;
            device.Text = cr.DEVICE;
            camera_warn_limit.Text = cr.SavedParams.Camera_Threshold.ToString();
            face_limit.Text = cr.SavedParams.Face_LimitSize.ToString();
            face_score.Text = cr.SavedParams.Face_Score.ToString();
            temp_value.Text = cr.SavedParams.Upgrade_Coefficient.ToString();
            version_label.Content = "Device version:" + cr.VERSION;

        }

        public CameraInfo GetCameraInfo()
        {
            return ci;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            float black_temp = 0.0f;
            if (float.TryParse(balckTemp.Text, out black_temp))
            {
                if (!(black_temp >= 33 && black_temp <= 42))
                {
                    ErrorWindow ew = new ErrorWindow(Application.Current.FindResource("numError").ToString(), Application.Current.FindResource("error1").ToString());
                    ew.ShowDialog();
                    return;
                }
            }

            float cwl = 0.0f;
            if (float.TryParse(camera_warn_limit.Text, out cwl))
            {
                if (!(cwl >= 35 && cwl <= 42))
                {
                    ErrorWindow ew = new ErrorWindow(Application.Current.FindResource("numError").ToString(), Application.Current.FindResource("error2").ToString());
                    ew.ShowDialog();
                    return;
                }
            }
            float fl = 0.0f;
            if (float.TryParse(face_limit.Text, out fl))
            {
                if (!(fl >= 15 && fl <= 80))
                {
                    ErrorWindow ew = new ErrorWindow(Application.Current.FindResource("numError").ToString(), Application.Current.FindResource("error3").ToString());
                    ew.ShowDialog();
                    return;
                }
            }
            float fc = 0.0f;
            if (float.TryParse(face_score.Text, out fc))
            {
                if (!(fc >= 0.2 && fc <= 1.0))
                {
                    ErrorWindow ew = new ErrorWindow(Application.Current.FindResource("numError").ToString(), Application.Current.FindResource("error4").ToString());
                    ew.ShowDialog();
                    return;
                }
            }

            if (ci.state == CamContectingState.ONLINE)
            {
                var formDatas = new List<FormItemModel>();

                // 温度系数
                formDatas.Add(new FormItemModel()
                {
                    Key = "BlackCell-Temperature",
                    Value = balckTemp.Text // "id-test-id-test-id-test-id-test-id-test-"
                });

                formDatas.Add(new FormItemModel()
                {
                    Key = "Report-URL",
                    Value = address.Text // "id-test-id-test-id-test-id-test-id-test-"
                });

                formDatas.Add(new FormItemModel()
                {
                    Key = "Camera-Threshold",
                    Value = camera_warn_limit.Text
                });

                formDatas.Add(new FormItemModel()
                {
                    Key = "Face-LimitSize",
                    Value = face_limit.Text
                });

                formDatas.Add(new FormItemModel()
                {
                    Key = "Face-Score",
                    Value = face_score.Text
                });

                if (temp_check.IsChecked == true)
                {
                    formDatas.Add(new FormItemModel()
                    {
                        Key = "Upgrade-Coefficient",
                        Value = temp_value.Text
                    });
                }

                    fi = formDatas;

                CameraInfo cameraInfo = new CameraInfo();

                //cameraInfo.IP = ip.Text;

                cameraInfo.Name = ci.Name;
                cameraInfo.IP = ci.IP;
                cameraInfo.Device_Name = ci.Device_Name;
                cameraInfo.Date = DateTime.Now;
                cameraInfo.BlackCell_Temp = balckTemp.Text;
                cameraInfo.Report_URL = address.Text;
                AIThermometerAPP.Instance().cameras_config.UpdateCam(cameraInfo);
                AIThermometerAPP.Instance().SaveCameraConfigs();
                DialogResult = true;

            }
            else
            {
                CameraInfo cameraInfo = new CameraInfo();
                cameraInfo.Name = ci.Name;
                cameraInfo.IP = ip.Text; //.IP;
                //cameraInfo.Device_Name = ci.Device_Name;
                cameraInfo.Date = DateTime.Now;
                //cameraInfo.BlackCell_Temp = balckTemp.Text;
                //cameraInfo.Report_URL = address.Text;
                AIThermometerAPP.Instance().cameras_config.UpdateCam(cameraInfo);
                AIThermometerAPP.Instance().SaveCameraConfigs();
                DialogResult = true;                
            }

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftAlt) && Keyboard.IsKeyDown(Key.U))
            {
                FirmwareUpdateWindow fw = new FirmwareUpdateWindow(ci);
                fw.ShowDialog();

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
        
        private void temp_check_Click(object sender, RoutedEventArgs e)
        {
            if (temp_check.IsChecked == true)
            {
                temp_value.IsEnabled = true;
            }
            else
            {
                temp_value.IsEnabled = false;
            }
        }
    }
}

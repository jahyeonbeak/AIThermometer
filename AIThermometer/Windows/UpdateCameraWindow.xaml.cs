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

namespace AIThermometer.Windows
{
    /// <summary>
    /// AddCameraWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateCameraWindow : Window
    {
        private CameraInfo ci;

        public UpdateCameraWindow(CameraInfo _ci)
        {
            InitializeComponent();
            ci = _ci;
            ci.state = ci.state;
            title.Content = ci.Name;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Topmost = true;
            ip.Text = ci.IP;

            balckTemp.Text = ci.BlackCell_Temp;
            address.Text = ci.Report_URL;
            device.Text = ci.Device_Name;

            camera_warn_limit.Text = ci.Camera_Threshold;
            face_limit.Text = ci.Face_LimitSize;
            face_score.Text = ci.Face_Score;
        }

        public CameraInfo GetCameraInfo()
        {
            return ci;
        }


        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (ci.state == CamContectingState.ONLINE) {
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

                try
                {
                    //提交表单
                    CameraInfo cameraInfo = new CameraInfo();

                    var result = FormPost.PostForm("http://"+ci.IP +":9300/config", formDatas);
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
                catch (Exception)
                {

                    ErrorWindow er = new ErrorWindow("修改错误", "不能修改离线的设备");
                    er.ShowDialog();
                }

            }
            else
            {
                ErrorWindow er = new ErrorWindow("修改错误", "不能修改离线的设备");
                er.ShowDialog();
                return;
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
                    ErrorWindow er = new ErrorWindow("升级错误", "不能升级离线的设备");
                    er.ShowDialog();
                    this.DialogResult = false;
                }
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
                    var result = FormPost.PostForm("http://" + ci.IP + ":9300/Update", formDatas);
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
    }
}

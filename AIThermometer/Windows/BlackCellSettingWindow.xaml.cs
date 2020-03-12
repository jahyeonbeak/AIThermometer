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
using System.Windows.Shapes;
using AIThermometer.Services;
using System.Threading;
using AIThermometer.Cores;

namespace AIThermometer.Windows
{
    /// <summary>
    /// BlackCellSettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BlackCellSettingWindow : Window
    {

        CameraResponse cr;

        float image_height = 288;
        float image_width = 384;

        private string file_path;
        private float _leftup_pos_x;
        private float _leftup_pos_y;
        private float _rightdown_pos_x;
        private float _rightdown_pos_y;
        private float _pos_margin = 10;
        private string request_ip = "";
        Point startPoint = new Point();

        HttpRequestHelper http_request = new HttpRequestHelper();


        public BlackCellSettingWindow(string path, string ip)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Topmost = true;
            request_ip = ip;
            file_path = path;
            
        
        }

        public bool Init()
        {
            string location_json = "";
            // Http get
            //location_json = HttpRequestHelper.HttpGet1("http://192.168.0.119:9300/config");
            //location_json = http_request.HttpGet("http://192.168.0.119:9300/config", "");
                location_json = http_request.HttpGet("http://" + request_ip + ":9300/config", "");
            
            if (location_json == "")
                return false;
            //Console.WriteLine("fffff"+location_json);
            // test_string
            cr = JsonHelper.FromJSON<CameraResponse>(location_json);
            //  MessageBox.Show(cr.SavedParams.Camera_IP);

            if (!Common.FileExists(AIThermometerAPP.Instance().CameraPath()))
                return false;

            using (FileStream fs = new FileStream(file_path, FileMode.Open, FileAccess.Read)) //将图片以文件流的形式进行保存

            {
                BinaryReader br = new BinaryReader(fs);
                byte[] imgBytesIn = br.ReadBytes((int)fs.Length); //将流读入到字节数组中

                BitmapImage myBitmapImage = new BitmapImage();
                myBitmapImage.BeginInit();
                myBitmapImage.StreamSource = new MemoryStream(imgBytesIn);
                myBitmapImage.EndInit();
                imagebox.Source = myBitmapImage;



                Leftup_Pos_X = cr.SavedParams.BlackCell_Position[0];
                Leftup_Pos_Y = cr.SavedParams.BlackCell_Position[1];
                Rightdown_Pos_X = cr.SavedParams.BlackCell_Position[2];
                Rightdown_Pos_Y = cr.SavedParams.BlackCell_Position[3];

                float lx = (float)(image_width * Leftup_Pos_X);// cr.SavedParams.BlackCell_Position[0]);
                float ly = (float)(image_height * Leftup_Pos_Y);// cr.SavedParams.BlackCell_Position[1]);
                float rx = (float)(image_width * Rightdown_Pos_X);// cr.SavedParams.BlackCell_Position[2]);
                float ry = (float)(image_height * Leftup_Pos_Y);// cr.SavedParams.BlackCell_Position[3]);

                canvas.Children.Clear();
                Rectangle rect = new Rectangle
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Height = 10,
                    Width = 10,
                };
                Canvas.SetLeft(rect, lx);
                Canvas.SetTop(rect, ly);
                Canvas.SetRight(rect, rx);
                Canvas.SetBottom(rect, ry);
                canvas.Children.Add(rect);
                return true;
            }
            {
                ErrorWindow ew = new ErrorWindow("错误", "获得黑体坐标错误");
                ew.ShowDialog();
                return false;
            }
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            cr.SavedParams.BlackCell_Position[0] = Leftup_Pos_X;
            cr.SavedParams.BlackCell_Position[1] = Leftup_Pos_Y;
            cr.SavedParams.BlackCell_Position[2] = Rightdown_Pos_X;
            cr.SavedParams.BlackCell_Position[3] = Rightdown_Pos_Y;
            POS pos = new POS();

            pos.BlackCell_Position = cr.SavedParams.BlackCell_Position;
            string json = JsonHelper.ToJSON(cr.SavedParams.BlackCell_Position);
            //bool a = true;

            //HttpRequestHelper fff = new HttpRequestHelper();
            //fff.HttpPost("http://192.168.0.85:9300/config", json,ref a);
            Console.WriteLine(json);


            var formDatas = new List<FormItemModel>();

            //添加文本
            formDatas.Add(new FormItemModel()
            {
                Key = "BlackCell-Position",
                Value = json // "id-test-id-test-id-test-id-test-id-test-"
            });

            //提交表单
            try
            {
                AIThermometerAPP.Instance().blackcell_pos_error = true;
                var result = FormPost.PostForm("http://" + this.request_ip + ":9300/config", formDatas);
                AIThermometerAPP.Instance().ResetBlackCell();
                DialogResult = true;

            }
            catch (Exception ex)
            {
                DialogResult = false;
            }

        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }




        // 实现一下 xaml背景图的set接口
        // 把郭德纲换成任意自定义的图，参数用什么都行

        /*
         * 四个点比例的返回接口实现一下
      

            x35, y145,  x45, y155
            500  200    500  200
        */

        public float Leftup_Pos_X { get { return _leftup_pos_x; } set { _leftup_pos_x = value; } }

        public float Leftup_Pos_Y { get { return _leftup_pos_y; } set { _leftup_pos_y = value; } }

        public float Rightdown_Pos_X { get { return _rightdown_pos_x; } set { _rightdown_pos_x = value; } }

        public float Rightdown_Pos_Y { get { return _rightdown_pos_y; } set { _rightdown_pos_y = value; } }

        public float Pos_Margin { get { return _pos_margin; } set { _pos_margin = value; } }

        private void inkCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // 鼠标点击坐标中心,上下左右加减变量pos_margin
            //如  0，0-------——
            //    |                 |
            //    |   鼠标（10，10）  |
            //    |                  |
            //    ————————20，20
            //    存到变量里
            //    清空canver上的所有黑色矩形后，画新的！ 永远只有1个矩形
            //  
            startPoint = e.GetPosition(sender as Canvas);
            canvas.Children.Clear();
            Rectangle rect = new Rectangle
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Height = 10,
                Width = 10,
            };
            Canvas.SetLeft(rect, startPoint.X - 5);
            Canvas.SetTop(rect, startPoint.Y - 5);
            Canvas.SetRight(rect, startPoint.Y + 5);
            Canvas.SetBottom(rect, startPoint.Y + 5);
            canvas.Children.Add(rect);

            float lx = (float)(startPoint.X - 5);
            float ly = (float)(startPoint.Y - 5);
            float rx = (float)(startPoint.X + 5);
            float ry = (float)(startPoint.Y + 5);
            Leftup_Pos_X = (float)(lx / image_width);
            Leftup_Pos_Y = (float)(ly / image_height);
            Rightdown_Pos_X = (float)(rx / image_width);
            Rightdown_Pos_Y = (float)(ry / image_height);

            Console.WriteLine(Leftup_Pos_X + "," + Leftup_Pos_Y + "   " + Rightdown_Pos_X + "," + Rightdown_Pos_Y);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

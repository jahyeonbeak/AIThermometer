using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIThermometer.Cores
{
    // 本地设置信息
    public class LocalSetting
    {
        // 本地IP地址
        public string local_url { get; set; }

        // 本地监听端口
        public int local_port { get; set; }

        // 软件语言
        public string language { get; set; }

        // 告警Bar元素长度
        public int warning_bar_length { get; set; }

       
        // vlc库路径
        //public string vlc_path { get; set; }

        // 告警温度阈值
        public float temp_limit { get; set; }

        // 是否自动启动摄像头
        public bool camera_auto_start { get; set; }

        // 人脸图片保存期限
        public int clean_day { get; set; }

        // 视频超时间
        public int video_timeout { get; set; }
    }
}

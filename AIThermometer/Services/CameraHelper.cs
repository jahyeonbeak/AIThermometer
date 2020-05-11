using AIThermometer.Cores;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIThermometer.Services
{
    public enum CamContectingState
    {
        ONLINE,
        OFFLINE,
        ERROR
    }

    public class CameraInfo
    {
        // camera name
        [JsonProperty]
        public string Name { get; set; }
        // camera ip
        [JsonProperty]
        public string IP { get; set; }
        // time
        [JsonProperty]
        public DateTime Date { get; set; }

        public CamContectingState state { get; set; }

        [JsonProperty]
        public StreamType StreamType { set; get; }

        [JsonProperty(PropertyName = "BlackCell-Temperature")]
        public string BlackCell_Temp { set; get; }

        [JsonProperty(PropertyName = "Report-URL")]
        public string Report_URL { set; get; }

        [JsonProperty(PropertyName = "DEVICE")]
        public string Device_Name { set; get; }

        // 相机显示温度报警阈值
        [JsonProperty(PropertyName = "Camera-Threshold")]
        public string Camera_Threshold { get; set; }

        // 最小人脸阈值
        [JsonProperty(PropertyName = "Face-LimitSize")]
        public string Face_LimitSize { get; set; }
        // 人脸可信度阈值
        [JsonProperty(PropertyName = "Face-Score")]
        public string Face_Score { get; set; }

        // 温度系数
        [JsonProperty(PropertyName = "Upgrade-Coefficient")]
        public string Upgrade_Coefficient { get; set; }

    }

    public class CameraConfig
    {
        [JsonProperty]
        public List<CameraInfo> Cameras { get; set; }

        public bool AddCam(CameraInfo ci)
        {
            foreach (var c in Cameras)
            {
                if (c.Name == ci.Name)
                {
                    return false;
                }
            }
            Cameras.Add(ci);
            return true;
        }

        public void UpdateCam(CameraInfo ci) {
            if (Cameras.Count <= 0)
                return;
            foreach (var c in Cameras)
            {
                if (c.Name == ci.Name)
                {
                    c.IP = ci.IP;
                    //c.BlackCell_Temp = ci.BlackCell_Temp;
                    break;
                }
            }
        }


        public void DeleteCamByName(string name)
        {
            if (Cameras.Count <= 0)
                return;
            foreach (var c in Cameras)
            {
                if (c.Name == name)
                {
                    Cameras.Remove(c);
                    break;
                }
            }
        }

        public void DeleteCamByIP(string ip)
        {
            if (Cameras.Count <= 0)
                return;
            foreach (var c in Cameras)
            {
                if (c.IP == ip)
                {
                    Cameras.Remove(c);
                    break;
                }
            }
        }

        public string GetNameByIP(string ip)
        {
            if (Cameras.Count <= 0)
                return "";
            foreach (var c in Cameras)
            {
                if (c.IP == ip)
                {
                    return c.Name;
                }
            }
            return "";
        }
    }

    public class CameraHelper
    {
        public static void SavePic(string ip)
        {
            LogHelper.WriteLog("Saved picture from " + ip + "! File path " + "");
        }
    }


    // 摄像头储存数据
    public class CameraSavedParams
    {
        /*
        "Algorithm-DetMod" : 0,
		"Algorithm-Mask-Enable" : 1,
		"BlackCell-Distance" : 5.0,
		"BlackCell-Position" : 
		[
			0.34000000000000002,
			0.45000000000000001,
			0.75,
			0.58999999999999997
		],
		"BlackCell-Power" : 12.0,
		"BlackCell-Temperature" : 37.0,
		"\u0699!BkCell-Power" : null,
		"BlackCell-Temperature" : 37.0,
		"Camera-CaptureMode" : 0,
		"Camera-IP" : "192.168.0.84",
		"Camera-MeasureMode" : 0,
		"Camera-Screen" : 0,
		"Camera-Threshold" : 37.330001831054688,
		"Face-LimitSize" : null,
		"Port" : 9300,
		"RGB-Danger" : 
		[
			0,
			0,
			255
		],
		"RGB-Health" : 
		[
			255,
			255,
			0
		],
		*/
	
        // 检测模式
        //[JsonProperty(PropertyName = "Algorithm-DetMod")]
        //public int Algorithm_DetMod { get; set; }
        // 是否上报口罩情况
        //[JsonProperty(PropertyName = "Algorithm-Mask-Enable")]
        //public int Algorithm_Mask_Enable { get; set; }
        // 黑体距离相机的距离
        //[JsonProperty(PropertyName = "BlackCell-Distance")]
        //public float BlackCell_Distance { get; set; }
        // 黑体在视野中标定的位置
        [JsonProperty(PropertyName = "BlackCell-Position")]
        public List<float> BlackCell_Position { get; set; }
        // 黑体发射功率
        //[JsonProperty(PropertyName = "BlackCell-Power")]
        //public float BlackCell_Power { get; set; }
        // 黑体当前设置的绝对值温度
        [JsonProperty(PropertyName = "BlackCell-Temperature")]
        public float BlackCell_Temperature { get; set; }
        // 报警快照模式
        //[JsonProperty(PropertyName = "Camera-CaptureMode")]
        //public int Camera_CaptureMode { get; set; }
        // 相机IP地址
        //[JsonProperty(PropertyName = "Camera-IP")]
        //public string Camera_IP { get; set; }
        // 测温模式
        //[JsonProperty(PropertyName = "Camera-MeasureMode")]
        //public int Camera_MeasureMode { get; set; }
        // 输出分辨率设置
        //[JsonProperty(PropertyName = "Camera-Screen")]
        //public int Camera_Screen { get; set; }
        // 温度报警阈值
        [JsonProperty(PropertyName = "Camera-Threshold")]
        public float Camera_Threshold { get; set; }
        // 报警推送接口
        [JsonProperty(PropertyName = "Report-URL")]
        public string Report_URL { get; set; }
        // 最小人脸阈值
        [JsonProperty(PropertyName = "Face-LimitSize")]
        public int Face_LimitSize { get; set; }
        // 人脸可信度阈值
        [JsonProperty(PropertyName = "Face-Score")]
        public string Face_Score { get; set; }



        // 温度系数
        [JsonProperty(PropertyName = "Upgrade-Coefficient")]
        public float Upgrade_Coefficient { get; set; }

    }
    
    public class POS
    {
        // 黑体在视野中标定的位置
        [JsonProperty(PropertyName = "BlackCell-Position")]
        public List<float> BlackCell_Position { get; set; }
    }
    // 请求摄像头后返回数据
    public class CameraResponse
    {
        // 返回码
        public int Code { get; set; }

        // 版本号
        public string DEVICE { get; set; }

        // 返回信息
        public string Message { get; set; }

        // 摄像头储存数据
        public CameraSavedParams SavedParams { get; set; }

        // VERSION
        public string VERSION { get; set; }
    }

    // 摄像头请求的报警数据
    public class CameraWarningReports
    {
        // 异常温度
        public float Temperature { get; set; }
        // 画面位置
        public List<float> Position { get; set; }
        // 是否带了口罩
        public bool Mask { get; set; }
        // id
        public int objId { get; set; }

    }

    // 摄像头请求的报警数据
    public class CameraWarning
    {
        // 返回码
        public int Code { get; set; }
        // 返回信息
        public string Message { get; set; }
        // 异常反馈人员
        public int Count { get; set; }
        public List<CameraWarningReports> Reports { get; set; }


    }


    // 请求摄像头后返回数据
    public class FirmwareResponse
    {
        // 返回码
        public int Code { get; set; }

        // 返回信息
        public string Message { get; set; }

        // version
        public List<string> Patchs { get; set; }

        }

}

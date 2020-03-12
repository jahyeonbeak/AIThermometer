using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIThermometer.Cores
{
    using MultiCam = Dictionary<CamMode, CamStream>;

    // 单一视频流
    public interface CamStream
    {
        void Init();
        void Start();
        void Stop();
        void SetPath(string path);
        string GetPath();
        void SetIP(string ip);
        string GetIP();
        void SetName(string name);
        string GetName();
        CamMode GetMode();
        void SetMode(CamMode md);
    }

    public enum CamMode
    {
        NONE,
        NORMAL,
        IR
    }

    public enum StreamType
    {
        VLC
    }


    class CameraFactory
    {
        // 定义一个静态变量来保存类的实例
        private static CameraFactory instance = null;

        // 定义一个标识确保线程同步
        private static readonly object padlock = new object();

        public List<MultiCam> cl;

        // s双视频流 红外+普通
        private MultiCam mc;

        public bool CreateCameraStream(string name, string ip, StreamType stype)
        {
            var ir = GetCamStream(stype);
            var normal = GetCamStream(stype);

            if (ir == null || normal == null)
                return false;

            // 生成ir流实例
            ir.SetMode(CamMode.IR);
            ir.SetName(name);
            ir.SetPath(ip);
            ir.SetIP(ip);
            // 生成普通流实例
            normal.SetMode(CamMode.IR);
            normal.SetName(name);
            normal.SetPath(ip);
            normal.SetIP(ip);

            mc.Add(CamMode.IR, ir);
            mc.Add(CamMode.NORMAL, normal);

            cl.Add(mc);

            return true;

        }

        public static CamStream GetCamStream(StreamType stype)
        {
            if (stype == StreamType.VLC)
            {
                return new VlcCamera();
            }
            else
            {
                return null;
            }
        }

        public CameraFactory()
        {
            mc = new MultiCam();
            cl = new List<MultiCam>();
        }

        public static CameraFactory Instance()
        {
            if (instance == null)
            {
                lock (padlock)
                {
                    // 如果类的实例不存在则创建，否则直接返回
                    if (instance == null)
                    {
                        instance = new CameraFactory();
                    }
                }
            }
            return instance;
        }

        public MultiCam GetCameraByName(string cn)
        {
            if (cl.Count <= 0)
                return null;
            foreach (var c in cl)
            {
                if (c[0].GetName() == cn)
                    return c;
            }
            return null;
        }

        public MultiCam GetCameraByIP(string ci)
        {
            if (cl.Count <= 0)
                return null;
            foreach (var c in cl)
            {
                if (c[0].GetIP() == ci)
                    return c;
            }
            return null;
        }
    }
}


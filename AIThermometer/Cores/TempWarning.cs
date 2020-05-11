using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;


namespace AIThermometer.Cores
{
    public struct TempMessage
    {
        public int id;                                                                        
        public string cam;
        public float temp;
        public string photo_path;
        public byte[] bytes;
        public DateTime date;
    }

    public class TempWarning
    {
        // 报警温度队列
        private List<TempMessage> temp_list;
        // 报警温度队列长度
        private int length;

        // 定义一个静态变量来保存类的实例
        private static TempWarning instance = null;

        // 定义一个标识确保线程同步
        private static readonly object padlock = new object();

        // 添加管道委托
        public delegate void AddedQueueEventHandler(TempMessage tm);
        public AddedQueueEventHandler addedWarningInfo = null;

        public static TempWarning Instance()
        {
            if (instance == null)
            {
                lock (padlock)
                {
                    // 如果类的实例不存在则创建，否则直接返回
                    if (instance == null)
                    {
                        instance = new TempWarning();
                    }
                }
            }
            return instance;
        }

        public TempWarning(int len = 10)
        {
            this.length = len;
            this.temp_list = new List<TempMessage>();
        }

        public void SetLength(int len)
        {
            this.length = len;
            if (this.temp_list.Count - this.length > 0)
                this.temp_list.RemoveRange(0, this.temp_list.Count - this.length);
        }

        public void SetEventHandler(AddedQueueEventHandler et)
        {
            this.addedWarningInfo += et;
        }

        /// <summary>
        /// 添加到报警温度队列
        /// </summary>
        /// <param name="tm"></param>
        public void Push(TempMessage tm)
        {
            foreach(var item in this.temp_list)
            {
                if (item.id == tm.id)
                {
                    this.temp_list.Remove(item);
                    break;
                }
            }

            if (this.temp_list.Count() >= this.length)
                Pop();
            this.temp_list.Add(tm);

            if (addedWarningInfo != null)
                addedWarningInfo(tm);

            // 用线程储存头像文件
            Thread t = new Thread(SaveImage);
            t.Start(tm);
        }

        /// <summary>
        /// 从报警队列中移除
        /// </summary>
        public void Pop()
        {
            this.temp_list.RemoveAt(0);
        }

        public int Count()
        {
            return this.temp_list.Count;
        }

        private static void SaveImage(object obj)
        {
            TempMessage tm = (TempMessage)obj;
            //LogHelper.WriteLog("Method {0}!", obj.ToString());

            FileStream fs = new FileStream(tm.photo_path, FileMode.Create);
            fs.Write(tm.bytes, 0, tm.bytes.Length);
            fs.Close();
            fs.Dispose();
        }
    }
}

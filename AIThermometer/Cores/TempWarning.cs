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
        public string cam;
        public float temp;
        public string photo_path;
        public byte[] bytes;
        public DateTime date;
    }

    public class TempWarning
    {
        // 报警温度队列
        private Queue<TempMessage> temp_list;
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
            this.temp_list = new Queue<TempMessage>();
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
            if (this.temp_list.Count() >= this.length)
                Pop();
            this.temp_list.Enqueue(tm);

            if (addedWarningInfo != null)
                addedWarningInfo(tm);

            Thread t = new Thread(SaveImage);
            t.Start(tm);
        }

        /// <summary>
        /// 从报警队列中移除
        /// </summary>
        public void Pop()
        {
            this.temp_list.Dequeue();
        }

        public int Count()
        {
            return this.temp_list.Count;
        }

        private static void SaveImage(object obj)
        {
            TempMessage tm = (TempMessage)obj;
            //Console.WriteLine("Method {0}!", obj.ToString());

            FileStream fs = new FileStream(tm.photo_path, FileMode.Create);
            fs.Write(tm.bytes, 0, tm.bytes.Length);
            fs.Close();
            fs.Dispose();
        }
    }
}

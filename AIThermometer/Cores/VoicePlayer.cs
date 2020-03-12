using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AIThermometer.Cores
{
    public class VoicePlayer
    {

        SoundPlayer player = null;


        private static VoicePlayer instance = null;

        // 定义一个标识确保线程同步
        private static readonly object padlock = new object();

        private bool isPlaying = false;
        private string voice_path = "";

        public static VoicePlayer Instance()
        {
            if (instance == null)
            {
                lock (padlock)
                {
                    // 如果类的实例不存在则创建，否则直接返回
                    if (instance == null)
                    {
                        instance = new VoicePlayer();
                    }
                }
            }
            return instance;
        }

        public VoicePlayer()
        {
            player = new SoundPlayer();
        }

        public bool IsPlaying()
        {
            return this.isPlaying;
        }

        public void SetPath(string path)
        {
            this.voice_path = path;
            player.SoundLocation = this.voice_path;
            player.Load();
        }

        public void Play()
        {
            if (this.isPlaying == true)
                return;

            this.isPlaying = true;
            Thread t = new Thread(new ThreadStart(() =>
            {
                player.PlaySync();//.Play();
                this.isPlaying = false;

            }));
            t.Start();

        }
    }
}
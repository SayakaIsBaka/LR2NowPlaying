using System;
using System.Threading;

namespace LR2NowPlaying
{
    class Program
    {
        static int Main(string[] args)
        {
            string dllPath = AppDomain.CurrentDomain.BaseDirectory + "/LR2mind.dll";
            string processName = "LRHbody";

            Injector injector = new Injector(processName);
            int ret = injector.Inject(dllPath);

            Thread receiverThread = new Thread(
                delegate ()
                {
                    MindReceiver receiver = new MindReceiver(2222);
                    receiver.Listen();
                });
            receiverThread.Start();
            return ret;
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

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

            BlockingCollection<string> queue = new BlockingCollection<string>();

            Thread receiverThread = new Thread(
                delegate ()
                {
                    MindReceiver receiver = new MindReceiver(2222, queue);
                    receiver.Listen();
                });
            receiverThread.Start();

            Task.Run(() =>
            {
                Processor processor = new Processor(queue);
                while (true)
                {
                    processor.Process();
                }
            });

            return ret;
        }
    }
}

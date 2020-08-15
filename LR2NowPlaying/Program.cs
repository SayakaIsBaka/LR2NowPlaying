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
            string[] processNames = { "LRHbody", "LR2body" };
            int ret = 0;

            foreach (string processName in processNames) {
                Injector injector = new Injector(processName);
                ret = injector.Inject(dllPath);
                if (ret == 0) break;
            }

            if (ret != 0)
            {
                Console.Error.WriteLine("Could not inject the dll to LR2, exiting...");
                Environment.Exit(ret);
            }

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

            return 0;
        }
    }
}

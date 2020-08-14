using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LR2NowPlaying
{
    class MindReceiver
    {
        private int port;
        private UdpClient udpClient;
        private BlockingCollection<string> queue;

        public MindReceiver(int port, BlockingCollection<string> queue)
        {
            this.port = port;
            this.queue = queue;
            udpClient = new UdpClient();
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));
        }

        public void Listen()
        {
            IPEndPoint from = new IPEndPoint(0, 0);
            while (true)
            {
                byte[] recvBuffer = udpClient.Receive(ref from);
                string str = Encoding.UTF8.GetString(recvBuffer).Substring(1);
                Console.WriteLine(str);
                queue.Add(str);
            }
        }
    }
}

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace LR2NowPlaying
{
    class MindReceiver
    {
        private int port;
        private UdpClient udpClient;

        public MindReceiver(int port)
        {
            this.port = port;
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
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(str))
                    {
                        var hash = md5.ComputeHash(stream);
                        Console.WriteLine(BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant());
                    }
                }
            }
        }
    }
}

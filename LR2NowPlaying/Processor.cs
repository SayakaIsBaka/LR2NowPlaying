using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;

namespace LR2NowPlaying
{
    class Processor
    {
        private BlockingCollection<string> queue;
        
        public Processor(BlockingCollection<string> queue)
        {
            this.queue = queue;
        }

        public void Process()
        {
            string str = queue.Take();
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(str))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    string hashStr = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    Console.WriteLine(hashStr);
                }
            }
        }
    }
}

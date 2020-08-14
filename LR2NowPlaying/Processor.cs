using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace LR2NowPlaying
{
    class Processor
    {
        private BlockingCollection<string> queue;
        static readonly HttpClient client = new HttpClient();

        public Processor(BlockingCollection<string> queue)
        {
            this.queue = queue;
        }

        public void Process()
        {
            string bmsPath = queue.Take();
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(bmsPath))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    string hashStr = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    Console.WriteLine(hashStr);

                    Task.Run(async () =>
                    {
                        try
                        {
                            string responseBody = await client.GetStringAsync("http://www.dream-pro.info/~lavalse/LR2IR/search.cgi?mode=ranking&bmsmd5=" + hashStr);
                            Console.WriteLine(responseBody);
                        }
                        catch (HttpRequestException e)
                        {
                            Console.Error.WriteLine(e);
                        }
                    });
                }
            }
        }
    }
}

﻿using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

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
                            byte[] response = await client.GetByteArrayAsync("http://www.dream-pro.info/~lavalse/LR2IR/search.cgi?mode=ranking&bmsmd5=" + hashStr);
                            string responseBody = Encoding.GetEncoding(932).GetString(response, 0, response.Length - 1);

                            HtmlDocument htmlDoc = new HtmlDocument();
                            htmlDoc.LoadHtml(responseBody);

                            bool isBMSFound = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='box']").Descendants("h4").Any();
                            if (isBMSFound)
                            {
                                Dictionary<string, string> dic = GetInfoFromLR2IR(htmlDoc);
                                WriteFile(dic);
                            }
                            else
                            {
                                throw new HttpRequestException("BMS not found on LR2IR, fallback to BMS reading");
                            }
                            
                        }
                        catch (Exception e)
                        {
                            await Console.Error.WriteLineAsync(e.Message);
                            Dictionary<string, string> dic = GetInfoFromBMS(bmsPath);
                            WriteFile(dic);
                        }
                    });
                }
            }
        }

        private Dictionary<string, string> GetInfoFromLR2IR(HtmlDocument htmlDoc)
        {
            HtmlNode tagsHtml = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='box']/table/tr[2]/td");
            HashSet<string> tags = new HashSet<string>();

            HtmlNode exLevelHtml = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='box']/table/tr[1]/td[2]");
            if (exLevelHtml.Descendants("a").Any())
            {
                string exLevelUrl = exLevelHtml.LastChild.Attributes["href"].Value;
                string exLevel = HttpUtility.ParseQueryString(new Uri("http://www.dream-pro.info/~lavalse/LR2IR/" + exLevelUrl).Query).Get("exlevel");
                int exLevelInt = int.Parse(exLevel);
                if (exLevelInt >= 31 && exLevelInt <= 43) // normal1 table (1 to 13)
                {
                    tags.Add("#☆" + (exLevelInt - 30));
                }
                else if (exLevelInt == 44) // normal1 table (X)
                {
                    tags.Add("#☆X");
                }
                else if (exLevelInt <= 25) // insane1 table (1 to 25)
                {
                    tags.Add("#★" + exLevel);
                }
                else if (exLevelInt == 99) // insane1 table (???)
                {
                    tags.Add("#★???");
                }
            }
            
            foreach (HtmlNode tag in tagsHtml.Descendants())
            {
                if (tag.NodeType == HtmlNodeType.Element && tag.InnerText.Length != 0)
                {
                    tags.Add("#" + tag.InnerText);
                }
            }

            Dictionary<string, string> dic = new Dictionary<string, string>
            {
                { "{genre}", htmlDoc.DocumentNode.SelectSingleNode("//div[@id='box']/h4").InnerText },
                { "{title}", htmlDoc.DocumentNode.SelectSingleNode("//div[@id='box']/h1").InnerText },
                { "{artist}", htmlDoc.DocumentNode.SelectSingleNode("//div[@id='box']/h2").InnerText },
                { "{tags}", String.Join(", ", tags) }
            };

            return dic;
        }

        private Dictionary<string, string> GetInfoFromBMS(string bmsPath)
        {
            try
            {
                using (FileStream stream = File.OpenRead(bmsPath))
                {
                    BmsParser parser = new BmsParser();
                    BmsHeader bms = parser.Parse(stream);

                    Dictionary<string, string> dic = new Dictionary<string, string>
                    {
                        { "{genre}", bms.Genre },
                        { "{title}", bms.Title },
                        { "{artist}", bms.Artist },
                        { "{tags}", bms.Total.HasValue ? "TOTAL " + bms.Total : "" }
                    };

                    return dic;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return new Dictionary<string, string>();
            }
        }

        private void WriteFile(Dictionary<string, string> dic)
        {
            string templatePath = AppDomain.CurrentDomain.BaseDirectory + "/template.txt";
            string template = "";

            try
            {
                byte[] templateByte = File.ReadAllBytes(templatePath);
                template = Encoding.UTF8.GetString(templateByte);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                template = "[{genre}] {title} - {artist} ({tags})";
            }
            
            foreach (var key in dic.Keys)
            {
                template = template.Replace(key, dic[key]);
                Console.WriteLine($"{key}: {dic[key]}");
            }

            File.WriteAllText("nowplaying.txt", template);
        }
    }
}

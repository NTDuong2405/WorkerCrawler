using HtmlAgilityPack;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerCrawler
{
    class Program
    {

        static void Main(string[] args)
        {
            HtmlWeb web = new HtmlWeb()
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8  //Set UTF8 để hiển thị tiếng Việt
            };
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            MyDbContext db = new MyDbContext();

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "amqCrawler",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    //Console.WriteLine(" [x] Received {0}", message);

                    var p = db.News.SingleOrDefault(s => s.UrlBasePost.Equals(message));
                    if (p == null)
                    {
                        doc = web.Load(message);
                        var title = doc.DocumentNode.SelectSingleNode("//h1[@class='title-detail']");
                        var titleText = title.InnerText;
                        //Console.WriteLine(title.InnerText);

                        var description = doc.DocumentNode.SelectSingleNode("//p[@class='description']");
                        var descriptionText = description.InnerText;
                        //Console.WriteLine(description.InnerText);

                        var urlImg = doc.DocumentNode.SelectSingleNode("//img[@itemprop='contentUrl']");
                        var urlImgStr = "";
                        if (urlImg != null)
                        {
                            urlImgStr = urlImg.Attributes["data-src"].Value;
                            //    Console.WriteLine(urlImg.Attributes["data-src"].Value);
                        }


                        var content = doc.DocumentNode.SelectSingleNode("//div[@class='sidebar-1']");
                        var contentText = content.InnerHtml;
                        //Console.WriteLine(content.InnerHtml);


                        News post = new News()
                        {
                            UrlBasePost = message,
                            Title = titleText,
                            UrlImg = urlImgStr,
                            Description = descriptionText,
                            Content = contentText
                        };

                        db.News.Add(post);
                        db.SaveChanges();

                        Console.WriteLine("Save Success !");
                    }


                };
                channel.BasicConsume(queue: "amqCrawler",
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

    }
}

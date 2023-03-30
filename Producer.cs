
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace OnlinePhotoCollage
{
    public class Producer
    {
        private int _messageCount = 1;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _config;
        private static string url= "amqps";
        public Producer(IMemoryCache memoryCache,IConfiguration config)
        {
            _memoryCache = memoryCache;
            _config = config;
        }

        public string PushMessageToQ(List<String> images,int border,int colorRed,int colorGreen,int colorBlue,String orientation)
        {
            try
            {   var uniqueId=Guid.NewGuid().ToString();
               // var factory = new ConnectionFactory() { HostName = "localhost"};
                var factory= new ConnectionFactory();
                url = _config.GetConnectionString("RabbitMQ");
                factory.Uri = new Uri(url.Replace("amqp://", "amqps://"));
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: _config.GetConnectionString("QueueName"),
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                        var message = $"{uniqueId}";
                        

                        Dictionary<List<string>,Tuple<int,int,int,int, string, string>> messages = null;
                        Dictionary<string,string> outputImage = null;

                        _memoryCache.TryGetValue<Dictionary<string,string>>("outputImage", out outputImage);
                         if (outputImage == null) outputImage = new Dictionary<string,string>();
                         
                         outputImage.Add(uniqueId,"");
                         _memoryCache.Set<Dictionary<string,string>>("outputImage",outputImage);

                        _memoryCache.TryGetValue<Dictionary<List<string>,Tuple<int,int,int,int,string, string>>>("messages", out messages);
                        if (messages == null) messages = new Dictionary<List<string>,Tuple<int,int,int,int,string, string>>();
                        messages.Add(images, Tuple.Create(border,colorRed,colorGreen,colorBlue,orientation,uniqueId));
                        _memoryCache.Set<Dictionary<List<string>, Tuple<int,int,int,int,string,string>>>("messages", messages);

                        var messageBody = Encoding.UTF8.GetBytes(message);
                       

                        channel.BasicPublish(exchange: _config.GetConnectionString("ExchangeName"), routingKey: _config.GetConnectionString("RoutingKey"), body: messageBody, basicProperties: null);
                    }
                }

                return uniqueId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} | {ex.StackTrace}");
                return null;
            }
        }
    }
}

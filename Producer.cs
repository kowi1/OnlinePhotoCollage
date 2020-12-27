
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RabbitMQ.Client;


namespace OnlinePhotoCollage
{
    public class Producer
    {
        private int _messageCount = 1;
        private readonly IMemoryCache _memoryCache;

        public Producer(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public bool PushMessageToQ(List<String> images,int border,int colorRed,int colorGreen,int colorBlue,String orientation)
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: "counter",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                        var message = $"Message {_messageCount++}";

                        Dictionary<List<string>,Tuple<int,int,int,int, string, int>> messages = null;
                        _memoryCache.TryGetValue<Dictionary<List<string>,Tuple<int,int,int,int,string, int>>>("messages", out messages);
                        if (messages == null) messages = new Dictionary<List<string>,Tuple<int,int,int,int,string, int>>();
                        messages.Add(images, Tuple.Create(border,colorRed,colorGreen,colorBlue,orientation,_messageCount));
                        _memoryCache.Set<Dictionary<List<string>, Tuple<int,int,int,int,string, int>>>("messages", messages);

                        var messageBody = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish(exchange: "counter", routingKey: "counter", body: messageBody, basicProperties: null);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} | {ex.StackTrace}");
                return false;
            }
        }
    }
}
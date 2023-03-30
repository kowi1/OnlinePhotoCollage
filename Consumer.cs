using Microsoft.Extensions.Caching.Memory;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace OnlinePhotoCollage
{
    public class Consumer
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _config;
        private static string url = "amqps" ;
        ConnectionFactory _factory { get; set; }
        IConnection _connection { get; set; }
        IModel _channel { get; set; }

        public Consumer(IMemoryCache memoryCache,IConfiguration config)
        {
            _memoryCache = memoryCache;
            _config = config;
        }

        public void ReceiveMessageFromQ()
        {
            try
            {

                //_factory = new ConnectionFactory() { HostName = "localhost" };
                url = _config.GetConnectionString("RabbitMQ");
                _factory = new ConnectionFactory();
                _factory.Uri = new Uri(url.Replace("amqp://", "amqps://"));
                _connection = _factory.CreateConnection();

 
                _channel = _connection.CreateModel();

                {
                    _channel.QueueDeclare(queue: _config.GetConnectionString("QueueName"),
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    _channel.BasicQos(prefetchSize: 0, prefetchCount: 3, global: false);

                    var consumer = new EventingBasicConsumer(_channel);
                    consumer.Received += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var outputImageId=message;
                        
                        Dictionary<string,string> outputImage = null;
                        _memoryCache.TryGetValue<Dictionary<string,string>>("outputImage", out outputImage);
                        if (outputImage == null) outputImage = new Dictionary<string,string>();
                         

                        Dictionary<List<string>,Tuple<int,int,int,int,string,string>> messages = null;
                       // Dictionary<List<string>, int> messag = null;
                        _memoryCache.TryGetValue<Dictionary<List<string>, Tuple<int,int,int,int,string,string>>>("messages", out messages);
                        if (messages == null) messages = new Dictionary<List<string>, Tuple<int,int,int,int,string,string>>();

                        Console.WriteLine(" [x] Received {0}", message);
                        Thread.Sleep(3000);
                       // var messageCount = new string(message.SkipWhile(c=>!char.IsDigit(c)).TakeWhile(c=>char.IsDigit(c)).ToArray());
                       var KeyofIndex= messages.Where(kvp => kvp.Value.Item6 == outputImageId).FirstOrDefault();
                        var messageDictIndex =  Array.IndexOf(messages.Keys.ToArray(), KeyofIndex.Key);

                        Tasks _task=new Tasks();

                        // Get imagelist and stitching options from messages stored in shared memory cache
                        // Both the consumer and producer have access to this memory cache.
                        
                        List<string> imageList = messages.ElementAt(messageDictIndex).Key;
                        Tuple<int,int,int,int,string,string> stitchSettings = messages.ElementAt(messageDictIndex).Value;

                       // _task.Resize(messages.ElementAt(Int16.Parse(number)-1).Key[0],Int16.Parse(number)-1);
                        var ouputFilePath =_task.Stitch(imageList,message,stitchSettings);
                        outputImage[outputImageId]=ouputFilePath;

                         messages.Remove(imageList);
                        _memoryCache.Set<Dictionary<List<string>, Tuple<int,int,int,int,string, string>>>("messages", messages);
                        _channel.BasicAck(ea.DeliveryTag, false);

                    };

                    _channel.BasicConsume(queue: _config.GetConnectionString("QueueName"),
                                         autoAck: false,
                                         consumer: consumer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} | {ex.StackTrace}");
            }
        }
    }
}

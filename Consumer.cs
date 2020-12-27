using Microsoft.Extensions.Caching.Memory;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnlinePhotoCollage
{
    public class Consumer
    {
        private readonly IMemoryCache _memoryCache;
        ConnectionFactory _factory { get; set; }
        IConnection _connection { get; set; }
        IModel _channel { get; set; }

        public Consumer(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void ReceiveMessageFromQ()
        {
            try
            {
                _factory = new ConnectionFactory() { HostName = "localhost" };
                _connection = _factory.CreateConnection();
                _channel = _connection.CreateModel();

                {
                    _channel.QueueDeclare(queue: "counter",
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

                        Dictionary<List<string>,Tuple<int,int,int,int,string, int>> messages = null;
                       // Dictionary<List<string>, int> messag = null;
                        _memoryCache.TryGetValue<Dictionary<List<string>, Tuple<int,int,int,int,string, int>>>("messages", out messages);
                        if (messages == null) messages = new Dictionary<List<string>, Tuple<int,int,int,int,string, int>>();

                        Console.WriteLine(" [x] Received {0}", message);
                        //Thread.Sleep(3000);
                        var number = new string(message.SkipWhile(c=>!char.IsDigit(c))
                         .TakeWhile(c=>char.IsDigit(c))
                         .ToArray());
                         Console.WriteLine(messages.ElementAt(Int16.Parse(number)-1).Key[0]);
                        Tasks _task=new Tasks();
                          _task.Resize(messages.ElementAt(Int16.Parse(number)-1).Key[0],Int16.Parse(number)-1);
                        _task.Stitch(messages.ElementAt(Int16.Parse(number)-1).Key,Int16.Parse(number)-1,messages.ElementAt(Int16.Parse(number)-1).Value);
                       // messag.Add(messages.ElementAt(Int16.Parse(number)).Key,messages.ElementAt(Int16.Parse(number)).Value);
                       // messages.Remove(messages.ElementAt(Int16.Parse(number)-1).Key);
                        _memoryCache.Set<Dictionary<List<string>, Tuple<int,int,int,int,string, int>>>("messages", messages);
                        _channel.BasicAck(ea.DeliveryTag, false);
                    };

                    _channel.BasicConsume(queue: "counter",
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
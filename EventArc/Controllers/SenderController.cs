using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ServiceStack.Redis.Generic;
using ServiceStack.Redis.Pipeline;
using ServiceStack.Text;
using ServiceStack.Caching;
using ServiceStack.Redis;

namespace EventArc.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SenderController : ControllerBase
    {
        private const string URL = "https://localhost86/CallReceiver";

        public ActionResult<string> Send()
        {

            List<string> message = new List<string>();
            var factory = new ConnectionFactory() { HostName = "10.0.75.1", Port = 5672, UserName = "admin", Password = "admin", VirtualHost = "/" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "test",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new QueueingBasicConsumer(channel);
                channel.BasicConsume("test", true, consumer);

                while (true)
                {
                    var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

                    var body = ea.Body;
                    message.Add(Encoding.UTF8.GetString(body));
                    return JsonConvert.SerializeObject(message);
                }
            }
        }

        public ActionResult<string> Send1()
        {

            List<string> message = new List<string>();
            var factory = new ConnectionFactory() { HostName = "rabbit", UserName = "admin", Password = "admin", VirtualHost = "/" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "test",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new QueueingBasicConsumer(channel);
                channel.BasicConsume("test", true, consumer);

                while (true)
                {
                    var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

                    var body = ea.Body;
                    message.Add(Encoding.UTF8.GetString(body));
                    CallSender(JsonConvert.SerializeObject(message));
                }
            }
        }

        public string CallSender(string message)
        {
            using (RedisClient redisClient = new RedisClient("10.0.75.1", 6379))

            {

                var isSuccess = redisClient.Set("message", message);
                return message;
            }
        }
    }
}
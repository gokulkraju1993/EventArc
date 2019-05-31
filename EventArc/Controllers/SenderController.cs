using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventArc.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SenderController : ControllerBase
    {
        public void Send() {

            List<string> message = new List<string>();
            var factory = new ConnectionFactory() { HostName = "localhost" };
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
                    
                }
            }
        }
    }
}
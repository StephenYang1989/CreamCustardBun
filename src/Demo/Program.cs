using CreamCustardBun.Handling;
using CreamCustardBun.Handling.Interface;
using CreamCustardBun.Model;
using CreamCustardBun.Serialization;
using System;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            IConsumer consumer = new Consumer(new SimpleJsonMessageDecoder());

            IConsumer<TestModel> consumerT = new Consumer<TestModel>(new SimpleJsonMessageDecoder());


            HostOption hostOption = new HostOption
            {
                ClientName = "TestCreamCustardBun",
                Host = "192.168.1.10",
                Port = 5672,
                VirtualHost = "/dev",
                UserName = "username",
                Password = "password",
            };

            ExchangeOption exchangeOption = new ExchangeOption
            {
                ExchangeName = "temp.exchange",
                ExchangeType = "direct",
                IsDurable = false,
                IsAutoDeleted = false,
                RoutingKey = "temp.routingkey",
            };

            QueueOption queueOption = new QueueOption
            {
                IsDurable = false,
                IsAutoDeleted = false,
                IsExclusive = false,
                QueueName = "temp.queue",
                ConsumerTag = "",
            };

            MessageQueueOption option = new MessageQueueOption
            {
                ClientName = "TestCreamCustardBun",
                Host = "192.168.1.10",
                Port = 5672,
                VirtualHost = "/dev",
                UserName = "username",
                Password = "password",
                ExchangeName = "temp.exchange",
                ExchangeType = "direct",
                ExchangeDurable = false,
                ExchangeAutoDelete = false,
                RoutingKey = "temp.routingkey",
                QueueDurable = false,
                QueueAutoDelete = false,
                QueueExclusive = false,
                QueueName = "temp.queue",
                CustomerTag = "",
                QueueAutoAck = true
            };

            consumerT.MessageArrived += Consumer_MessageArrivedT;
            consumerT.Start(option);


            consumer.MessageArrived += Consumer_MessageArrived;
            consumer.Start(option);

            Console.ReadLine();
        }

        private static void Consumer_MessageArrivedT(object sender, CreamCustardBun.Model.MessageArrivedEventArgs<TestModel> e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Id[{e.Data.Id}] Name[{e.Data.Name}]");

        }

        private static void Consumer_MessageArrived(object sender, CreamCustardBun.Model.MessageArrivedEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - DataLength[{e.Data.Length}]");

        }
    }

    public class TestModel
    {
        public int Id { set; get; }

        public string Name { set; get; }
    }
}

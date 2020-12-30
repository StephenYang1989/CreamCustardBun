using System;
using System.Collections.Generic;
using System.Text;

namespace CreamCustardBun.Model
{
    public class MessageQueueOption
    {
        public string ClientName { set; get; }

        public string Host { set; get; }

        public int Port { set; get; }

        public string VirtualHost { set; get; }

        public string UserName { set; get; }

        public string Password { set; get; }

        public string ExchangeName { set; get; }

        public string ExchangeType { set; get; }

        public bool ExchangeDurable { set; get; }

        public bool ExchangeAutoDelete { set; get; }

        public string QueueName { set; get; }

        public bool QueueDurable { set; get; }

        public bool QueueExclusive { set; get; }

        public bool QueueAutoDelete { set; get; }

        public bool QueueAutoAck { set; get; }

        public string RoutingKey { set; get; }

        public string CustomerTag { set; get; }
    }
}

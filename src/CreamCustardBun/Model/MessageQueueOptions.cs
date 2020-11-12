using System;
using System.Collections.Generic;
using System.Text;

namespace CreamCustardBun.Options
{
    public class MessageQueueOption
    {
        public string Host { set; get; }

        public int Port { set; get; }

        public string VirtualHost { set; get; }

        public string UserName { set; get; }

        public string Password { set; get; }

        public string ExchangeName { set; get; }

        public string ExchangeType { set; get; }

        public string QueueName { set; get; }

        public string RoutingKey { set; get; }

        public bool IsExclusive { set; get; } = true;

        public bool IsDurable { set; get; }

        public bool IsAutoDeleted { set; get; }

        public string CustomerTag { set; get; }
    }
}

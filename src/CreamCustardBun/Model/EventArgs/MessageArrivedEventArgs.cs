using System;
using System.Collections.Generic;
using System.Text;

namespace CreamCustardBun.Model
{
    public class MessageArrivedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// The consumer tag of the consumer that the message was delivered to.
        /// </summary>
        public string ConsumerTag { get; private set; }

        /// <summary>
        /// The delivery tag for this delivery. 
        /// </summary>
        public ulong DeliveryTag { get; private set; }

        /// <summary>
        /// The exchange the message was originally published to.
        /// </summary>
        public string Exchange { get; private set; }

        /// <summary>
        /// The AMQP "redelivered" flag.
        /// </summary>
        public bool Redelivered { get; private set; }

        /// <summary>
        /// The routing key this queue binding.
        /// </summary>
        public string RoutingKey { get; private set; }

        /// <summary>
        /// Message body
        /// </summary>
        public T Data { set; private get; }

        private Action<ulong,bool> AckAction;

        public MessageArrivedEventArgs(string consumerTag, ulong deliveryTag, string exchange, bool redelivered, string routingKey, T data,Action<ulong,bool> ackAction)
        {
            ConsumerTag = consumerTag;
            DeliveryTag = deliveryTag;
            Exchange = exchange;
            Redelivered = redelivered;
            RoutingKey = routingKey;
            Data = data;
            AckAction += ackAction;
        }

        /// <summary>
        /// Ack message
        /// </summary>
        public void Ack() 
        {
            AckAction?.Invoke(DeliveryTag, false);
        }
    }
}

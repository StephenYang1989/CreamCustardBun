using System;
using System.Collections.Generic;
using System.Text;

namespace CreamCustardBun.Model
{
    public class MessageArrivedEventArgs : EventArgs
    {
        /// <summary>
        /// The consumer tag of the consumer that the message was delivered to.
        /// </summary>
        public string ConsumerTag { get; protected set; }

        /// <summary>
        /// The delivery tag for this delivery. 
        /// </summary>
        public ulong DeliveryTag { get; protected set; }

        /// <summary>
        /// The exchange the message was originally published to.
        /// </summary>
        public string Exchange { get; protected set; }

        /// <summary>
        /// The AMQP "redelivered" flag.
        /// </summary>
        public bool Redelivered { get; protected set; }

        /// <summary>
        /// The routing key this queue binding.
        /// </summary>
        public string RoutingKey { get; protected set; }

        /// <summary>
        /// Message body
        /// </summary>
        public byte[] Data { protected set; get; }

        protected Action<ulong, bool> AckAction;

        public MessageArrivedEventArgs(string consumerTag, ulong deliveryTag, string exchange, bool redelivered, string routingKey, byte[] data, Action<ulong, bool> ackAction)
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

    public class MessageArrivedEventArgs<T> : MessageArrivedEventArgs
    {
        /// <summary>
        /// Message body
        /// </summary>
        public new T Data { private set; get; }

        public MessageArrivedEventArgs(string consumerTag, ulong deliveryTag, string exchange, bool redelivered, string routingKey, T data, Action<ulong, bool> ackAction) : base(consumerTag, deliveryTag, exchange, redelivered, routingKey, new byte[] { }, ackAction)
        {
            Data = data;
        }
    }
}

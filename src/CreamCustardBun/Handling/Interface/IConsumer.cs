using CreamCustardBun.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace CreamCustardBun.Handling.Interface
{
    public interface IConsumer : IDisposable
    {
        event EventHandler<ConnectedEventArgs> Connected;

        event EventHandler<DisconnectedEventArgs> Disconnected;

        event EventHandler<MessageArrivedEventArgs> MessageArrived;

        /// <summary>
        /// Is reconnect the queue while the connection is shutdown
        /// </summary>
        bool IsReconnect { set; get; }

        void Start(MessageQueueOption option);

        void Start(HostOption hostOption, ExchangeOption exchangeOption, QueueOption queueOption);

        void Stop();

        uint ConsumerCount();
        uint MessageCount();
    }

    public interface IConsumer<T> : IConsumer
    {
        event EventHandler<MessageArrivedEventArgs<T>> MessageArrived;
    }
}

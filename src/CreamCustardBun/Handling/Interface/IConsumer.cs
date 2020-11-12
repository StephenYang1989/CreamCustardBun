using CreamCustardBun.Model;
using CreamCustardBun.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace CreamCustardBun.Handling.Interface
{
    public interface IConsumer<T> : IDisposable
    {
        event EventHandler<BrokerConnectedEventArgs> Connected;

        event EventHandler<BrokerDisconnectedEventArgs> Disconnected;

        event EventHandler<MessageArrivedEventArgs<T>> MessageArrived;

        void Start(HostOption hostOption, ExchangeOption exchangeOption, QueueOption queueOption);

        void Stop();

        uint ConsumerCount();
        uint MessageCount();
    }
}

using CreamCustardBun.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace CreamCustardBun.Handling.Interface
{
    public interface IProducer
    {
        bool IsConnected { get; }

        void Start(MessageQueueOption option);

        void Start(HostOption hostOption, ExchangeOption exchangeOption);

        void Start(HostOption hostOption, ExchangeOption exchangeOption, QueueOption queueOption);

        void ReConnect();
        void PublishData(byte[] data);
        void Close();
    }
}

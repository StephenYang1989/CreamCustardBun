using CreamCustardBun.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace CreamCustardBun.Handling.Interface
{
    public interface IProducer
    {
        void Start(HostOption hostOption, ExchangeOption exchangeOption);
        void PublishData(byte[] data);
        void Close();
    }
}

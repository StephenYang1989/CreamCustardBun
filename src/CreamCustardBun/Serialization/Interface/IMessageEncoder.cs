using System;
using System.Collections.Generic;
using System.Text;

namespace CreamCustardBun.Serialization
{
    public interface IMessageEncoder
    {
        byte[] EncodeMessage(object message);
        byte[] EncodeMessage<T>(T message);
    }
}

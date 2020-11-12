using System;
using System.Collections.Generic;
using System.Text;

namespace CreamCustardBun.Serialization
{
    public interface IMessageDecoder
    {
        T DecodeMessage<T>(byte[] data);
        T DecodeMessage<T>(byte[] data, int dataLength);
        T DecodeMessage<T>(byte[] data, int dataOffset, int dataLength);
    }
}

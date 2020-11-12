using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CreamCustardBun.Serialization
{
    public class SimpleJsonMessageDecoder : IMessageDecoder
    {
        public T DecodeMessage<T>(byte[] data)
        {
            return DecodeMessage<T>(data, data.Length);
        }

        public T DecodeMessage<T>(byte[] data, int dataLength)
        {
            return DecodeMessage<T>(data, 0, data.Length);
        }

        public T DecodeMessage<T>(byte[] data, int dataOffset, int dataLength)
        {
            if (data == null)
                throw new ArgumentNullException("The message cannot be null.");

            if (dataOffset >= data.Length)
                throw new ArgumentException("The parameter 'dataOffset' should less then length of data");

            string jsonStr = Encoding.UTF8.GetString(data,dataOffset,dataLength);
            if (string.IsNullOrWhiteSpace(jsonStr))
                throw new Exception("Deserialize message gets empty content.");

            return JsonConvert.DeserializeObject<T>(jsonStr);
        }
    }
}

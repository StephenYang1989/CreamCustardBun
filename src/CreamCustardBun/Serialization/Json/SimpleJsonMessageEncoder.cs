﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CreamCustardBun.Serialization
{
    public class SimpleJsonMessageEncoder : IMessageEncoder
    {
        public byte[] EncodeMessage(object message)
        {
            if (message == null)
                throw new ArgumentNullException("The message cannot be null.");

            var jsonString = JsonConvert.SerializeObject(message);
            if (jsonString == null)
                throw new Exception("Serialize message gets empty content.");

            return Encoding.UTF8.GetBytes(jsonString);
        }

        public byte[] EncodeMessage<T>(T message) => EncodeMessage(message);
    }
}

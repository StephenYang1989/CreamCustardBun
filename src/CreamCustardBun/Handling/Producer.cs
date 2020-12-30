using CreamCustardBun.Handling.Interface;
using CreamCustardBun.Model;
using CreamCustardBun.Serialization;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace CreamCustardBun.Handling
{
    public class Producer : IProducer
    {
        private IModel mChannel;

        private IConnection mConnection;

        string mExchangeName;

        string mRoutingKey;

        /// <summary>
        /// DEnoder
        /// </summary>
        IMessageEncoder mEncoder;

        public Producer(IMessageEncoder encoder)
        {
            mEncoder = encoder;
        }

        public void Start(MessageQueueOption option)
        {
            var hostOption = new HostOption
            {
                ClientName = option.ClientName,
                Host = option.Host,
                Port = option.Port,
                VirtualHost = option.VirtualHost,
                UserName = option.UserName,
                Password = option.Password
            };

            var exchangeOption = new ExchangeOption
            {
                ExchangeName = option.ExchangeName,
                ExchangeType = option.ExchangeType,
                IsAutoDeleted = option.ExchangeAutoDelete,
                IsDurable = option.ExchangeDurable,
                RoutingKey = option.RoutingKey
            };

            Start(hostOption, exchangeOption);
        }

        /// <summary>
        /// Start consume
        /// </summary>
        /// <param name="hostOption"></param>
        /// <param name="exchangeOption"></param>
        public void Start(HostOption hostOption, ExchangeOption exchangeOption)
        {
            var factory = new ConnectionFactory()
            {
                ClientProvidedName = hostOption.ClientName,
                HostName = hostOption.Host,
                Port = hostOption.Port,
                VirtualHost = hostOption.VirtualHost,
                UserName = hostOption.UserName,
                Password = hostOption.Password,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(hostOption.ConnectionTimeout),
                RequestedHeartbeat = TimeSpan.FromSeconds(hostOption.HeartBeat)
            };

            mExchangeName = exchangeOption.ExchangeName;
            mRoutingKey = exchangeOption.RoutingKey;

            mConnection = factory.CreateConnection();

            mChannel = mConnection.CreateModel();

            mChannel.ExchangeDeclare(exchangeOption.ExchangeName, exchangeOption.ExchangeType);
        }

        public void PublishData<T>(T t)
        {
            var data= mEncoder.EncodeMessage(t);
            PublishData(data);
        }

        public void PublishData(byte[] data)
        {
            if (mChannel.IsClosed)
                throw new Exception("Broker unreachable");

            mChannel.BasicPublish(mExchangeName, mRoutingKey, false, null, data);
        }

        public void Close()
        {
            if (mChannel != null)
            {
                mChannel.Close();
                mChannel.Dispose();
                mChannel = null;
            }

            if (mConnection == null)
            {
                mConnection.Close();
                mConnection.Dispose();
                mConnection = null;
            }
        }
    }
}

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
        private IConnectionFactory mConnectionFactory;

        private IModel mChannel;

        private IConnection mConnection;

        string mExchangeName;

        string mExchangeType;

        string mRoutingKey;

        string mQueueName;

        bool mQueueDurable;

        bool mQueueAutoDelete;

        bool mQueueExclusive;

        /// <summary>
        /// DEnoder
        /// </summary>
        IMessageEncoder mEncoder;

        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (mChannel == null)
                    return false;
                return mChannel.IsOpen;
            }
        }

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

            var queueOption = new QueueOption
            {
                QueueName = option.QueueName,
                IsDurable = option.QueueDurable,
                IsAutoDeleted = option.QueueAutoDelete,
                IsExclusive = option.QueueExclusive,
            };

            Start(hostOption, exchangeOption);
        }
        public void Start(HostOption hostOption, ExchangeOption exchangeOption)
        {
            Start(hostOption, exchangeOption, null);
        }

        /// <summary>
        /// Start publisher
        /// </summary>
        /// <param name="hostOption"></param>
        /// <param name="exchangeOption"></param>
        public void Start(HostOption hostOption, ExchangeOption exchangeOption, QueueOption queueOption)
        {
            mConnectionFactory = new ConnectionFactory()
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
            mExchangeType = exchangeOption.ExchangeType;
            mRoutingKey = exchangeOption.RoutingKey;

            mConnection = mConnectionFactory.CreateConnection();

            mChannel = mConnection.CreateModel();
            if (queueOption != null && !string.IsNullOrWhiteSpace(queueOption.QueueName))
            {
                mQueueName = queueOption.QueueName;
                mQueueDurable = queueOption.IsDurable;
                mQueueAutoDelete = queueOption.IsAutoDeleted;
                mQueueExclusive = queueOption.IsExclusive;

                mChannel.QueueDeclare(mQueueName, mQueueDurable, mQueueExclusive, mQueueAutoDelete);
            }

            mChannel.ExchangeDeclare(mExchangeName, mExchangeType);
        }

        public void ReConnect()
        {
            if (mConnection == null || !mConnection.IsOpen)
                mConnection = mConnectionFactory.CreateConnection();

            if (mChannel == null || !mChannel.IsOpen)
            {
                mChannel = mConnection.CreateModel();
                mChannel.ExchangeDeclare(mExchangeName, mExchangeType);

                if (!string.IsNullOrWhiteSpace(mQueueName))
                {
                    mChannel.QueueDeclare(mQueueName, mQueueDurable, mQueueExclusive, mQueueAutoDelete);
                }
            }
        }

        public void PublishData<T>(T t)
        {
            var data = mEncoder.EncodeMessage(t);
            PublishData(data);
        }

        public void PublishData(byte[] data)
        {
            if (!mConnection.IsOpen)
            {
                mConnection = mConnectionFactory.CreateConnection();
            }

            if (mChannel.IsClosed)
            {
                mChannel = mConnection.CreateModel();

                mChannel.ExchangeDeclare(mExchangeName, mExchangeType);
            }

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

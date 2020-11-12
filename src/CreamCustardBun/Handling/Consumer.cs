using CreamCustardBun.Handling.Interface;
using CreamCustardBun.Model;
using CreamCustardBun.Options;
using CreamCustardBun.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace CreamCustardBun.Handling
{
    public class Consumer<T> : IConsumer<T>
    {
        private IModel mChannel;

        private IConnection mConnection;

        /// <summary>
        /// Queue name
        /// </summary>
        string mQueueName = string.Empty;

        /// <summary>
        /// Decoder
        /// </summary>
        IMessageDecoder mDecoder;

        /// <summary>
        /// Is auto ack the message
        /// </summary>
        public bool IsAutoAck { private set; get; }

        public Consumer(IMessageDecoder messageDecoder)
        {
            mDecoder = messageDecoder;
        }

        /// <summary>
        /// Connected event
        /// </summary>
        public event EventHandler<BrokerConnectedEventArgs> Connected;

        /// <summary>
        /// Disconnected event
        /// </summary>
        public event EventHandler<BrokerDisconnectedEventArgs> Disconnected;

        /// <summary>
        /// Message arrived event
        /// </summary>
        public event EventHandler<MessageArrivedEventArgs<T>> MessageArrived;

        /// <summary>
        /// Get the consumer count in this queu
        /// </summary>
        /// <returns></returns>
        public uint ConsumerCount()
        {
            return mChannel.ConsumerCount(mQueueName);
        }

        /// <summary>
        /// Get the message count in this queue
        /// </summary>
        /// <returns></returns>
        public uint MessageCount()
        {
            return mChannel.MessageCount(mQueueName);
        }

        /// <summary>
        /// Start consume
        /// </summary>
        /// <param name="hostOption"></param>
        /// <param name="exchangeOption"></param>
        /// <param name="queueOption"></param>
        public void Start(HostOption hostOption, ExchangeOption exchangeOption, QueueOption queueOption)
        {
            mQueueName = queueOption.QueueName;

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
            mConnection = factory.CreateConnection();

            mChannel = mConnection.CreateModel();

            mChannel.ExchangeDeclare(exchangeOption.ExchangeName, exchangeOption.ExchangeType);
            mChannel.QueueDeclare(queueOption.QueueName, queueOption.IsDurable, queueOption.IsExclusive, queueOption.IsAutoDeleted);
            mChannel.QueueBind(queueOption.QueueName, exchangeOption.ExchangeName, exchangeOption.RoutingKey);

            //consuming by event
            var consumer = new EventingBasicConsumer(mChannel);

            //registe events
            consumer.Received += Consumer_Received;
            consumer.Registered += Consumer_Registered;
            consumer.Shutdown += Consumer_Shutdown;

            //set the customer tag
            if (string.IsNullOrWhiteSpace(queueOption.CustomerTag))
                queueOption.CustomerTag = string.Empty;

            mChannel.BasicConsume(queueOption.QueueName, false, queueOption.CustomerTag, consumer);
        }

        /// <summary>
        /// Stop consume
        /// </summary>
        public void Stop()
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

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            //decode message
            var objectModel = mDecoder.DecodeMessage<T>(e.Body.ToArray());

            var eventArgsModel = new MessageArrivedEventArgs<T>(e.ConsumerTag, e.DeliveryTag, e.Exchange, e.Redelivered, e.RoutingKey, objectModel, Ack);

            //Invoke the event
            MessageArrived?.Invoke(sender, eventArgsModel);

            if (IsAutoAck)
                Ack(e.DeliveryTag, false);
        }

        /// <summary>
        /// Ack message
        /// </summary>
        /// <param name="deliveryTag"></param>
        /// <param name="isMultiple"></param>
        private void Ack(ulong deliveryTag, bool isMultiple)
        {
            mChannel.BasicAck(deliveryTag, isMultiple);
        }

        private void Consumer_Registered(object sender, ConsumerEventArgs e)
        {
            Connected?.Invoke(sender, new BrokerConnectedEventArgs());
        }

        private void Consumer_Shutdown(object sender, ShutdownEventArgs e)
        {
            Disconnected?.Invoke(sender, new BrokerDisconnectedEventArgs());
        }
    }
}

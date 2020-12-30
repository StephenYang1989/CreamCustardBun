using CreamCustardBun.Handling.Interface;
using CreamCustardBun.Model;
using CreamCustardBun.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CreamCustardBun.Handling
{
    public class Consumer : IConsumer
    {

        protected IModel mChannel;

        protected IConnection mConnection;

        protected EventingBasicConsumer mEventConsumer;

        /// <summary>
        /// Queue name
        /// </summary>
        protected string mQueueName = string.Empty;

        /// <summary>
        /// Decoder
        /// </summary>
        protected IMessageDecoder mDecoder;

        /// <summary>
        /// Host option
        /// </summary>
        protected HostOption mHostOption = null;

        /// <summary>
        /// Exchange option
        /// </summary>
        protected ExchangeOption mExchangeOption = null;

        /// <summary>
        /// Queue option
        /// </summary>
        protected QueueOption mQueueOption = null;

        protected string mLastConsumerTag = string.Empty;

        /// <summary>
        /// Is auto ack the message
        /// </summary>
        protected bool mIsAutoAck;

        /// <summary>
        /// Is reconnect the queue while the connection is shutdown
        /// </summary>
        public bool IsReconnect { set; get; }

        /// <summary>
        /// Is the connection close by manul.
        /// </summary>
        private bool IsCloseMamul { set; get; }

        /// <summary>
        /// Timer for reconnect
        /// </summary>
        Timer mTimerReconnect = null;

        /// <summary>
        /// Reconnect timespan
        /// </summary>
        TimeSpan mReconnectTimespan = TimeSpan.FromSeconds(30);

        public Consumer(IMessageDecoder messageDecoder)
        {
            mDecoder = messageDecoder;

            //Initial a reconnect timer,but not run it.
            mTimerReconnect = new Timer(ReconnectAction, null, Timeout.Infinite, Timeout.Infinite);
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
        public event EventHandler<MessageArrivedEventArgs> MessageArrived;

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

        public void Start(MessageQueueOption option)
        {
            var convertOption = ConvertOption(option);

            Start(convertOption.Item1, convertOption.Item2, convertOption.Item3);
        }

        /// <summary>
        /// Start consume
        /// </summary>
        /// <param name="hostOption"></param>
        /// <param name="exchangeOption"></param>
        /// <param name="queueOption"></param>
        public void Start(HostOption hostOption, ExchangeOption exchangeOption, QueueOption queueOption)
        {
            BindQueue(hostOption, exchangeOption, queueOption);

            //consuming by event
            mEventConsumer = new EventingBasicConsumer(mChannel);

            //registe events
            mEventConsumer.Received += Consumer_Received;
            mEventConsumer.Registered += Consumer_Registered;
            mEventConsumer.Shutdown += Consumer_Shutdown;

            mLastConsumerTag = mChannel.BasicConsume(mQueueName, mIsAutoAck, mLastConsumerTag, mEventConsumer);
        }

        /// <summary>
        /// BindQueue
        /// </summary>
        /// <param name="hostOption"></param>
        /// <param name="exchangeOption"></param>
        /// <param name="queueOption"></param>
        protected void BindQueue(HostOption hostOption, ExchangeOption exchangeOption, QueueOption queueOption)
        {
            mHostOption = hostOption;
            mExchangeOption = exchangeOption;
            mQueueOption = queueOption;

            mQueueName = queueOption.QueueName;
            mIsAutoAck = queueOption.AutoAck;

            mLastConsumerTag = queueOption.ConsumerTag;
            if (string.IsNullOrWhiteSpace(mLastConsumerTag))
                mLastConsumerTag = string.Empty;

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
            mChannel.BasicQos(0, 1, false);
        }

        /// <summary>
        /// Stop consume
        /// </summary>
        public void Stop()
        {
            if (mEventConsumer != null)
            {
                mEventConsumer.Received -= Consumer_Received;
                mEventConsumer.Registered -= Consumer_Registered;
                mEventConsumer.Shutdown -= Consumer_Shutdown;
            }

            if (mChannel != null)
            {
                mChannel.BasicCancel(mLastConsumerTag);

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

            IsCloseMamul = true;
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
        protected void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var eventArgsModel = new MessageArrivedEventArgs(e.ConsumerTag, e.DeliveryTag, e.Exchange, e.Redelivered, e.RoutingKey, e.Body.ToArray(), Ack);

            //Invoke the event
            MessageArrived?.Invoke(sender, eventArgsModel);

            if (mIsAutoAck)
                Ack(e.DeliveryTag, false);
        }

        /// <summary>
        /// Ack message
        /// </summary>
        /// <param name="deliveryTag"></param>
        /// <param name="isMultiple"></param>
        protected void Ack(ulong deliveryTag, bool isMultiple)
        {
            mChannel.BasicAck(deliveryTag, isMultiple);
        }

        protected void Consumer_Registered(object sender, ConsumerEventArgs e)
        {
            Connected?.Invoke(sender, new BrokerConnectedEventArgs());
            if (mTimerReconnect != null)
            {
                mTimerReconnect.Change(Timeout.Infinite, Timeout.Infinite);
            }

            IsCloseMamul = false;
        }

        protected void Consumer_Shutdown(object sender, ShutdownEventArgs e)
        {
            Disconnected?.Invoke(sender, new BrokerDisconnectedEventArgs());

            if (!IsCloseMamul && mTimerReconnect != null)
            {
                mTimerReconnect.Change(TimeSpan.Zero, mReconnectTimespan);
            }
        }

        protected void ReconnectAction(object obj)
        {
            //registe events
            if (mEventConsumer != null)
            {
                mEventConsumer.Received -= Consumer_Received;
                mEventConsumer.Registered -= Consumer_Registered;
                mEventConsumer.Shutdown -= Consumer_Shutdown;
            }

            Start(mHostOption, mExchangeOption, mQueueOption);
        }

        /// <summary>
        /// Convert HostOption & ExchangeOption & QueueOption to MessageQueueOption
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        protected (HostOption, ExchangeOption, QueueOption) ConvertOption(MessageQueueOption option)
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
                ConsumerTag = option.CustomerTag,
                IsAutoDeleted = option.QueueAutoDelete,
                IsDurable = option.QueueDurable,
                IsExclusive = option.QueueExclusive,
                QueueName = option.QueueName,
                AutoAck = option.QueueAutoAck
            };

            return (hostOption, exchangeOption, queueOption);
        }
    }

    public class Consumer<T> : Consumer, IConsumer<T>
    {
        public Consumer(IMessageDecoder messageDecoder) : base(messageDecoder)
        {
            mDecoder = messageDecoder;
        }

        /// <summary>
        /// Start consume
        /// </summary>
        /// <param name="option"></param>
        public new void Start(MessageQueueOption option)
        {
            var convertOption = ConvertOption(option);

            Start(convertOption.Item1, convertOption.Item2, convertOption.Item3);
        }

        /// <summary>
        /// Start consume
        /// </summary>
        /// <param name="hostOption"></param>
        /// <param name="exchangeOption"></param>
        /// <param name="queueOption"></param>
        public new void Start(HostOption hostOption, ExchangeOption exchangeOption, QueueOption queueOption)
        {
            BindQueue(hostOption, exchangeOption, queueOption);

            //consuming by event
            mEventConsumer = new EventingBasicConsumer(mChannel);

            //registe events
            mEventConsumer.Received += Consumer_Received;
            mEventConsumer.Registered += Consumer_Registered;
            mEventConsumer.Shutdown += Consumer_Shutdown;

            mLastConsumerTag = mChannel.BasicConsume(mQueueName, mIsAutoAck, mLastConsumerTag, mEventConsumer);
        }

        /// <summary>
        /// Message arrived event
        /// </summary>
        public new event EventHandler<MessageArrivedEventArgs<T>> MessageArrived;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private new void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            //decode message
            var objectModel = mDecoder.DecodeMessage<T>(e.Body.ToArray());

            var eventArgsModel = new MessageArrivedEventArgs<T>(e.ConsumerTag, e.DeliveryTag, e.Exchange, e.Redelivered, e.RoutingKey, objectModel, Ack);

            //Invoke the event
            MessageArrived?.Invoke(sender, eventArgsModel);

            //if (mIsAutoAck)
            //    Ack(e.DeliveryTag, false);
        }
    }
}

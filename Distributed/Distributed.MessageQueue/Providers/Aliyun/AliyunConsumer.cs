using ons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Distributed.MessageQueue.Providers.Aliyun
{
    internal sealed class AliyunConsumer : Connection, IConsumer
    {
        #region Field

        private readonly PushConsumer _consumer;
        internal readonly IList<Func<IRequestMessage, SubscribeAction?>> Funcs = new List<Func<IRequestMessage, SubscribeAction?>>();
        private bool _isSubscribe;

        #endregion Field

        #region Constructor

        public AliyunConsumer(PushConsumer consumer)
        {
            _consumer = consumer;
        }

        #endregion Constructor

        #region Overrides of Connection

        protected override void Start()
        {
            _consumer.start();
        }

        protected override void Shutdown()
        {
            _consumer.shutdown();
        }

        protected override void Dispose()
        {
            _consumer.Dispose();
        }

        #endregion Overrides of Connection

        #region Implementation of IConsumer

        public void Subscribe(string topic, string expression, Func<IRequestMessage, SubscribeAction?> func)
        {
            if (!_isSubscribe)
            {
                lock (this)
                {
                    if (!_isSubscribe)
                    {
                        MessageListener listener = new PrivateMessageListener(this);
                        _consumer.subscribe(topic, expression, ref listener);
                        _isSubscribe = true;
                    }
                }
            }
            lock (this)
            {
                Funcs.Add(func);
            }
        }

        #endregion Implementation of IConsumer

        #region HelpClass

        private class PrivateMessageListener : MessageListener
        {
            private readonly AliyunConsumer _consumer;

            public PrivateMessageListener(AliyunConsumer consumer)
            {
                _consumer = consumer;
            }

            #region Overrides of MessageListener

            public override ons.Action consume(ref Message message)
            {
                var aliyunMessage = new AliyunMessage(message);

                lock (_consumer)
                {
                    var action = _consumer.Funcs.Select(i => i(aliyunMessage)).ToArray().FirstOrDefault(i => i.HasValue);
                    switch (action)
                    {
                        case null:
                        case SubscribeAction.CommitMessage:
                            return ons.Action.CommitMessage;

                        case SubscribeAction.ReconsumeLater:
                            return ons.Action.ReconsumeLater;

                        default:
                            throw new NotSupportedException($"不支持的动作类型：{action}。");
                    }
                }
            }

            #endregion Overrides of MessageListener
        }

        #endregion HelpClass
    }
}
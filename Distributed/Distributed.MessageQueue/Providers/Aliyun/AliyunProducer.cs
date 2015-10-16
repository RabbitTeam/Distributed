using ons;
using System;
using System.Threading.Tasks;

namespace Distributed.MessageQueue.Providers.Aliyun
{
    internal sealed class AliyunProducer : Connection, IProducer
    {
        #region Field

        private readonly Producer _producer;

        #endregion Field

        #region Constructor

        public AliyunProducer(Producer producer)
        {
            _producer = producer;
        }

        #endregion Constructor

        #region Overrides of Connection

        protected override void Start()
        {
            _producer.start();
        }

        protected override void Shutdown()
        {
            _producer.shutdown();
        }

        protected override void Dispose()
        {
            _producer.Dispose();
        }

        #endregion Overrides of Connection

        #region Implementation of IProducer

        public async Task<SendResult> Send(IResponseMessage message)
        {
            if (message == null)
                throw new NullReferenceException(nameof(message));

            var aliyunMessage = message as AliyunMessage;

            if (aliyunMessage == null)
                throw new NotSupportedException($"当前发布者提供程序：{GetType().FullName}，无法发送类型为：{message.GetType().FullName}的消息。");
            if (string.IsNullOrWhiteSpace(aliyunMessage.Topic))
                throw new ArgumentException("消息主题（Topic）不能为空！");
            if (string.IsNullOrWhiteSpace(aliyunMessage.Body))
                throw new ArgumentException("消息主体（Body）不能为空！");

            return await Task.Run(() =>
            {
                using (var result = _producer.send(aliyunMessage.Message))
                {
                    return new SendResult { MessageId = result.getMessageId() };
                }
            });
        }

        #endregion Implementation of IProducer
    }
}
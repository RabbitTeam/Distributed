using ons;
using System.Threading.Tasks;

namespace Distributed.MessageQueue.Providers.Aliyun
{
    public sealed class AliyunMessageQueueFactory : IMessageQueueFactory
    {
        #region Field

        private readonly ONSFactory _factory;
        private readonly ONSFactoryAPI _factoryInstance;
        private readonly ONSFactoryProperty _factoryProperty;

        #endregion Field

        #region Constructor

        public AliyunMessageQueueFactory(AliyunMessageQueueFactoryOptions options)
        {
            _factory = new ONSFactory();
            _factoryInstance = _factory.getInstance();
            _factoryProperty = GetOnsFactoryProperty(options);
        }

        #endregion Constructor

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _factoryInstance.Dispose();
            _factory.Dispose();
            _factoryProperty.Dispose();
        }

        #endregion Implementation of IDisposable

        #region Implementation of IMessageQueueFactory

        public async Task<IConsumer> CreateConsumer(string consumerId)
        {
            _factoryProperty.setFactoryProperty(_factoryProperty.getConsumerIdName(), "CID_" + consumerId);
            return await Task.Run(() => new AliyunConsumer(_factoryInstance.createPushConsumer(_factoryProperty)));
        }

        public async Task<IProducer> CreateProducer(string producerId)
        {
            _factoryProperty.setFactoryProperty(_factoryProperty.getProducerIdName(), "PID_" + producerId);
            return await Task.Run(() => new AliyunProducer(_factoryInstance.createProducer(_factoryProperty)));
        }

        #endregion Implementation of IMessageQueueFactory

        #region Private Method

        private static ONSFactoryProperty GetOnsFactoryProperty(AliyunMessageQueueFactoryOptions options)
        {
            var factoryInfo = new ONSFactoryProperty();
            factoryInfo.setFactoryProperty(factoryInfo.getPublishTopicsName(), "null");
            factoryInfo.setFactoryProperty(factoryInfo.getAccessKeyName(), options.AccessKey);
            factoryInfo.setFactoryProperty(factoryInfo.getSecretKeyName(), options.SecretKey);
            return factoryInfo;
        }

        #endregion Private Method
    }
}
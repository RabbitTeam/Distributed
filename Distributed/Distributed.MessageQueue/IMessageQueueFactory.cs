using System;
using System.Threading.Tasks;

namespace Distributed.MessageQueue
{
    public interface IMessageQueueFactory : IDisposable
    {
        Task<IConsumer> CreateConsumer(string consumerId);

        Task<IProducer> CreateProducer(string producerId);
    }
}
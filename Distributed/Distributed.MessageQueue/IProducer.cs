using System.Threading.Tasks;

namespace Distributed.MessageQueue
{
    public class SendResult
    {
        public string MessageId { get; set; }
    }

    public interface IProducer : IConnection
    {
        Task<SendResult> Send(IResponseMessage message);
    }
}
using System;

namespace Distributed.MessageQueue
{
    public interface IRequestMessage
    {
        string Body { get; }
        string Key { get; }
        string MessageId { get; }
        string Tag { get; }
        string Topic { get; }
        TimeSpan StartDeliverTime { get; }

        string GetUserProperty(string key);

        string GetSystemProperty(string key);
    }

    public interface IResponseMessage
    {
        string Body { set; }
        string Key { set; }
        string MessageId { set; }
        string Tag { set; }
        string Topic { set; }
        TimeSpan StartDeliverTime { set; }

        void PutUserProperty(string key, string value);

        void PutSystemProperty(string key, string value);
    }
}
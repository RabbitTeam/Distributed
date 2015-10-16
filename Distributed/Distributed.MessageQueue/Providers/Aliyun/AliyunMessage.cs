using ons;
using System;

namespace Distributed.MessageQueue.Providers.Aliyun
{
    public sealed class AliyunMessage : IRequestMessage, IResponseMessage
    {
        internal readonly Message Message;

        public AliyunMessage()
        {
            Message = new Message();
        }

        public AliyunMessage(Message message)
        {
            Message = message;
        }

        public string Body
        {
            get { return Message.getBody(); }
            set { Message.setBody(value); }
        }

        public string Key
        {
            get { return Message.getKey(); }
            set { Message.setKey(value); }
        }

        public string MessageId
        {
            get { return Message.getMsgID(); }
            set { Message.setMsgID(value); }
        }

        public string Tag
        {
            get { return Message.getTag(); }
            set { Message.setTag(value); }
        }

        public string Topic
        {
            get { return Message.getTopic(); }
            set { Message.setTopic(value); }
        }

        public TimeSpan StartDeliverTime
        {
            get { return TimeSpan.FromMilliseconds(Message.getStartDeliverTime()); }
            set { Message.setStartDeliverTime((long)value.TotalMilliseconds); }
        }

        #region Implementation of IRequestMessage

        public string GetUserProperty(string key)
        {
            return Message.getUserProperty(key);
        }

        public string GetSystemProperty(string key)
        {
            return Message.getSystemProperty(key);
        }

        #endregion Implementation of IRequestMessage

        #region Implementation of IResponseMessage

        public void PutUserProperty(string key, string value)
        {
            Message.putUserProperty(key, value);
        }

        public void PutSystemProperty(string key, string value)
        {
            Message.putSystemProperty(key, value);
        }

        #endregion Implementation of IResponseMessage
    }
}
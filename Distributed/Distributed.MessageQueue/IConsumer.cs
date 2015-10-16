using System;

namespace Distributed.MessageQueue
{
    public enum SubscribeAction
    {
        CommitMessage = 0,
        ReconsumeLater = 1
    }

    public interface IConsumer : IConnection
    {
        void Subscribe(string topic, string expression, Func<IRequestMessage, SubscribeAction?> func);
    }

    public static class ConsumerExtensions
    {
        public static void Subscribe(this IConsumer consumer, string topic, Func<IRequestMessage, SubscribeAction?> func)
        {
            consumer.Subscribe(topic, "*", func);
        }

        public static void Subscribe(this IConsumer consumer, string topic, string expression, Action<IRequestMessage> action)
        {
            consumer.Subscribe(topic, expression, message =>
            {
                action(message);
                return null;
            });
        }

        public static void Subscribe(this IConsumer consumer, string topic, Action<IRequestMessage> action)
        {
            consumer.Subscribe(topic, "*", action);
        }
    }
}
using Distributed.MessageQueue;
using Distributed.MessageQueue.Providers.Aliyun;
using System;

namespace Producer
{
    internal class Program
    {
        private static void Main()
        {
            using (IMessageQueueFactory messageQueueFactory = new AliyunMessageQueueFactory(new AliyunMessageQueueFactoryOptions
            {
                AccessKey = "xxxxxxxx",
                SecretKey = "xxxxxxxxxx"
            }))
            {
                using (var producer = messageQueueFactory.CreateProducer("ChunSun").Result)
                {
                    producer.Start().Wait();
                    while (true)
                    {
                        Console.WriteLine("请输入消息，输入exit退出...");
                        var content = Console.ReadLine();

                        if (string.Equals(content, "exit", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }

                        var message = new AliyunMessage
                        {
                            Topic = "ChunSun",
                            Body = content
                        };
                        var messageId = producer.Send(message).Result;
                        Console.WriteLine($"发送成功，MessageId：{messageId}");
                    }
                }
            }
        }
    }
}
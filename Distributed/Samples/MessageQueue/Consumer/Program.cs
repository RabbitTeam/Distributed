using Distributed.MessageQueue;
using Distributed.MessageQueue.Providers.Aliyun;
using System;

namespace Consumer
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
                Console.WriteLine("请输入消费者Id...");
                var consumerId = Console.ReadLine();
                using (var consumer = messageQueueFactory.CreateConsumer(consumerId).Result)
                {
                    consumer.Subscribe("Test", "*", message =>
                    {
                        Console.WriteLine($"topic：{message.Topic}");
                        Console.WriteLine($"key：{message.Key}");
                        Console.WriteLine($"msdid：{message.MessageId}");
                        Console.WriteLine($"body：{message.Body}");
                        Console.WriteLine($"tag：{message.Tag}");
                        Console.WriteLine($"getStartDeliverTime：{message.StartDeliverTime}");
                        Console.WriteLine("======================================================");
                        return null;
                    });
                    consumer.Start().Wait();
                    Console.WriteLine("正在监听，按任意键退出...");
                    Console.ReadLine();
                }
            }
        }
    }
}
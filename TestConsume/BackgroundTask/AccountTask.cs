using Confluent.Kafka;
using Manonero.MessageBus.Kafka.Abstractions;
using System.Text.Json;

namespace TestConsume.BackgroundTask
{
    public class AccountTask : IConsumingTask<string, string>
    {
        public void Execute(ConsumeResult<string, string> result)
        {
            Console.WriteLine(JsonSerializer.Serialize(result.Message.Value));
        }
    }
}

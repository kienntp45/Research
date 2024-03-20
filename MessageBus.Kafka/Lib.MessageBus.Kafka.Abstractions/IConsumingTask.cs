using Confluent.Kafka;

namespace Lib.MessageBus.Kafka.Abstractions;

public interface IConsumingTask<TKey, TValue> : IConsumingTask
{
	void Execute(ConsumeResult<TKey, TValue> result);
}
public interface IConsumingTask
{
}

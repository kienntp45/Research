using System;
using System.Collections.Generic;
using Confluent.Kafka;
using Lib.MessageBus.Kafka.Settings;

namespace Lib.MessageBus.Kafka.Abstractions;

public interface IKafkaConsumer<TKey, TValue> : IKafkaConsumer, IDisposable
{
	IConsumer<TKey, TValue> Consumer { get; }

	void Consume(Action<ConsumeResult<TKey, TValue>> callback);

	void Commit(IEnumerable<TopicPartitionOffset> topicPartitionOffsets);

	void Commit(int partition, long offset, string topic);

	void Commit(int partition, long offset, int topicIndex = 0);

	void Commit(ConsumeResult<TKey, TValue> result);
}
public interface IKafkaConsumer : IDisposable
{
	bool IsRunning { get; }

	bool IsFree { get; }

	bool IsDisposed { get; }

	ConsumerSetting Setting { get; }

	void Pause();
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Lib.MessageBus.Kafka.Settings;

namespace Lib.MessageBus.Kafka.Abstractions;

public interface IKafkaProducer<TKey, TValue> : IKafkaProducer, IDisposable
{
	IProducer<TKey, TValue> Producer { get; }

	Task<DeliveryResult<TKey, TValue>> ProduceAsync(Message<TKey, TValue> message);

	Task<DeliveryResult<TKey, TValue>> ProduceAsync(string topic, Message<TKey, TValue> message);

	Task<DeliveryResult<TKey, TValue>> ProduceAsync(TopicPartition topicPartition, Message<TKey, TValue> message);

	void Produce(Message<TKey, TValue> message, Action<DeliveryReport<TKey, TValue>> deliveryHandler = null);

	void Produce(IEnumerable<Message<TKey, TValue>> messages, Action<DeliveryReport<TKey, TValue>> deliveryHandler = null);

	void Produce(Partition partition, Message<TKey, TValue> message, Action<DeliveryReport<TKey, TValue>> deliveryHandler = null);

	void Produce(string topic, Message<TKey, TValue> message, Action<DeliveryReport<TKey, TValue>> deliveryHandler = null);

	void Produce(TopicPartition topicPartition, Message<TKey, TValue> message, Action<DeliveryReport<TKey, TValue>> deliveryHandler = null);

	IKafkaProducerTransaction<TKey, TValue> InitTransaction();

	int Poll();

	int Poll(TimeSpan timeout);

	int Flush(TimeSpan timeout);

	void Flush(CancellationToken cancellationToken = default(CancellationToken));
}
public interface IKafkaProducer : IDisposable
{
	bool IsDisposed { get; }

	ProducerSetting Setting { get; }
}

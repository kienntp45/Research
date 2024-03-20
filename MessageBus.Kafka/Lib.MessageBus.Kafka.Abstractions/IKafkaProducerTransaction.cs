using System;
using System.Collections.Generic;
using Confluent.Kafka;

namespace Lib.MessageBus.Kafka.Abstractions;

public interface IKafkaProducerTransaction<TKey, TValue> : IDisposable
{
	bool IsDisposed { get; }

	void Begin();

	void Begin(TimeSpan timeout);

	void Commit();

	void Commit(TimeSpan timeout);

	void Abort();

	void Abort(TimeSpan timeout);

	void Produce(Message<TKey, TValue> message, Action<DeliveryReport<TKey, TValue>> deliveryHandler = null);

	void Produce(IEnumerable<Message<TKey, TValue>> messages, Action<DeliveryReport<TKey, TValue>> deliveryHandler = null);

	void Produce(string topic, Message<TKey, TValue> message, Action<DeliveryReport<TKey, TValue>> deliveryHandler = null);

	void Produce(TopicPartition topicPartition, Message<TKey, TValue> message, Action<DeliveryReport<TKey, TValue>> deliveryHandler = null);

	string GetId();

	IProducer<TKey, TValue> GetProducer();
}

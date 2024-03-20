using Lib.MessageBus.Kafka.Abstractions;

namespace Lib.MessageBus.Kafka.Internal;

internal class KafkaProducerTransaction<TKey, TValue> : IKafkaProducerTransaction<TKey, TValue>, IDisposable
{
	private IProducer<TKey, TValue> _producer;

	private readonly ProducerConfig _producerConfig;

	private readonly TimeSpan _defaultTimeOut;

	private readonly TopicPartition _topicPartition;

	public bool IsDisposed { get; private set; }

	public KafkaProducerTransaction(ProducerConfig config, TopicPartition topicPartition, TimeSpan pollTimeout)
	{
		config.TransactionalId = Guid.NewGuid().ToString();
		_defaultTimeOut = TimeSpan.FromMilliseconds(config.TransactionTimeoutMs.HasValue ? config.TransactionTimeoutMs.Value : 60000);
		_producerConfig = config;
		_topicPartition = topicPartition;
	}

	public void Abort()
	{
		_producer.AbortTransaction();
	}

	public void Abort(TimeSpan timeout)
	{
		_producer.AbortTransaction(timeout);
	}

	public void Begin()
	{
		Begin(_defaultTimeOut);
	}

	public void Begin(TimeSpan timeout)
	{
		GetProducer();
		_producer.InitTransactions(timeout);
		_producer.BeginTransaction();
	}

	public void Commit()
	{
		_producer.CommitTransaction();
	}

	public void Commit(TimeSpan timeout)
	{
		_producer.CommitTransaction(timeout);
	}

	public void Produce(IEnumerable<Message<TKey, TValue>> messages, Action<DeliveryReport<TKey, TValue>> deliveryHandler = null)
	{
		Partition partition = _topicPartition.Partition;
		if (((Partition)(partition)).Value < 0)
		{
			foreach (Message<TKey, TValue> message in messages)
			{
				Produce(_topicPartition.Topic, message, deliveryHandler);
			}
			return;
		}
		foreach (Message<TKey, TValue> message2 in messages)
		{
			Produce(_topicPartition, message2, deliveryHandler);
		}
	}

	public void Produce(Message<TKey, TValue> message, Action<DeliveryReport<TKey, TValue>> deliveryHandler = null)
	{
		Partition partition = _topicPartition.Partition;
		if (((Partition)(partition)).Value < 0)
		{
			Produce(_topicPartition.Topic, message, deliveryHandler);
		}
		else
		{
			Produce(_topicPartition, message, deliveryHandler);
		}
	}

	public void Produce(string topic, Message<TKey, TValue> message, Action<DeliveryReport<TKey, TValue>> deliveryHandler = null)
	{
		_producer.Produce(topic, message, deliveryHandler);
	}

	public void Produce(TopicPartition topicPartition, Message<TKey, TValue> message, Action<DeliveryReport<TKey, TValue>> deliveryHandler = null)
	{
		_producer.Produce(topicPartition, message, deliveryHandler);
	}

	public string GetId()
	{
		return _producerConfig.TransactionalId;
	}

	public IProducer<TKey, TValue> GetProducer()
	{
		if (_producer == null)
		{
			_producer = new ProducerBuilder<TKey, TValue>((IEnumerable<KeyValuePair<string, string>>)_producerConfig).Build();
		}
		return _producer;
	}

	public void Dispose()
	{
		if (!IsDisposed)
		{
			((IDisposable)_producer)?.Dispose();
			IsDisposed = true;
		}
	}
}

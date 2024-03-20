using Lib.MessageBus.Kafka.Abstractions;
using Lib.MessageBus.Kafka.Settings;

namespace Lib.MessageBus.Kafka.Internal;

internal class KafkaProducer<TKey, TValue> : IKafkaProducer<TKey, TValue>, IKafkaProducer, IDisposable
{
	private readonly ProducerConfig _producerConfig;

	private readonly TopicPartition _topicPartition;

	private readonly TimeSpan _pollTimeout;

	public bool IsDisposed { get; private set; }

	public ProducerSetting Setting { get; private set; }

	public IProducer<TKey, TValue> Producer { get; private set; }

	public KafkaProducer(ProducerSetting setting)
	{
		Setting = setting;
		_topicPartition = setting.GetTopicPartition();
		_producerConfig = setting.GetProducerConfig();
		Producer = GetProducerBuilder(setting).Build();
		_pollTimeout = TimeSpan.FromSeconds(setting.PollTimeoutSecond);
	}

	public async Task<DeliveryResult<TKey, TValue>> ProduceAsync(Message<TKey, TValue> message)
	{
		Partition partition = _topicPartition.Partition;
		return (((Partition)(partition)).Value >= 0) ? (await ProduceAsync(_topicPartition, message)) : (await ProduceAsync(_topicPartition.Topic, message));
	}

	public async Task<DeliveryResult<TKey, TValue>> ProduceAsync(string topic, Message<TKey, TValue> message)
	{
		return await Producer.ProduceAsync(topic, message, default(CancellationToken));
	}

	public async Task<DeliveryResult<TKey, TValue>> ProduceAsync(TopicPartition topicPartition, Message<TKey, TValue> message)
	{
		return await Producer.ProduceAsync(topicPartition, message, default(CancellationToken));
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

	public void Produce(Partition partition, Message<TKey, TValue> message, Action<DeliveryReport<TKey, TValue>> deliveryHandler = null)
	{
		TopicPartition topicPartition = new TopicPartition(_topicPartition.Topic, partition);
		Produce(topicPartition, message, deliveryHandler);
	}

	public void Produce(string topic, Message<TKey, TValue> message, Action<DeliveryReport<TKey, TValue>> deliveryHandler = null)
	{
		Producer.Produce(topic, message, deliveryHandler);
	}

	public void Produce(TopicPartition topicPartition, Message<TKey, TValue> message, Action<DeliveryReport<TKey, TValue>> deliveryHandler = null)
	{
		Producer.Produce(topicPartition, message, deliveryHandler);
	}

	public IKafkaProducerTransaction<TKey, TValue> InitTransaction()
	{
		return new KafkaProducerTransaction<TKey, TValue>(_producerConfig, _topicPartition, _pollTimeout);
	}

	public int Flush(TimeSpan timeout)
	{
		return Producer.Flush(timeout);
	}

	public void Flush(CancellationToken cancellationToken = default(CancellationToken))
	{
		Producer.Flush(cancellationToken);
	}

	public int Poll()
	{
		return Producer.Poll(_pollTimeout);
	}

	public int Poll(TimeSpan timeout)
	{
		return Producer.Poll(timeout);
	}

	public void Dispose()
	{
		if (!IsDisposed)
		{
			((IDisposable)Producer).Dispose();
			IsDisposed = true;
		}
	}

	private ProducerBuilder<TKey, TValue> GetProducerBuilder(ProducerSetting setting)
	{
		ProducerBuilder<TKey, TValue> val = new ProducerBuilder<TKey, TValue>((IEnumerable<KeyValuePair<string, string>>)setting.GetProducerConfig());
		if (Utils.InternalStore.Read("producererrorhandler" + setting.Id, out var value))
		{
			Action<IProducer<TKey, TValue>, Error> errorHandler = (Action<IProducer<TKey, TValue>, Error>)value;
			val.SetErrorHandler(errorHandler);
		}
		if (Utils.InternalStore.Read("producerkeyserializer" + setting.Id, out value))
		{
			ISerializer<TKey> keySerializer = (ISerializer<TKey>)value;
			val.SetKeySerializer(keySerializer);
		}
		if (Utils.InternalStore.Read("_producerkeyserializer" + setting.Id, out value))
		{
			IAsyncSerializer<TKey> keySerializer2 = (IAsyncSerializer<TKey>)value;
			val.SetKeySerializer(keySerializer2);
		}
		if (Utils.InternalStore.Read("producerloghandler" + setting.Id, out value))
		{
			Action<IProducer<TKey, TValue>, LogMessage> logHandler = (Action<IProducer<TKey, TValue>, LogMessage>)value;
			val.SetLogHandler(logHandler);
		}
		if (Utils.InternalStore.Read("producervalueserializer" + setting.Id, out value))
		{
			ISerializer<TValue> valueSerializer = (ISerializer<TValue>)value;
			val.SetValueSerializer(valueSerializer);
		}
		if (Utils.InternalStore.Read("_producervalueserializer" + setting.Id, out value))
		{
			IAsyncSerializer<TValue> valueSerializer2 = (IAsyncSerializer<TValue>)value;
			val.SetValueSerializer(valueSerializer2);
		}
		if (Utils.InternalStore.Read("producerdefaultpartitioner" + setting.Id, out value))
		{
			PartitionerDelegate defaultPartitioner = (PartitionerDelegate)value;
			val.SetDefaultPartitioner(defaultPartitioner);
		}
		if (Utils.InternalStore.Read("producerpartitioner" + setting.Id, out value))
		{
			PartitionerDelegate val2 = (PartitionerDelegate)value;
			val.SetPartitioner(_topicPartition.Topic, val2);
		}
		if (Utils.InternalStore.Read("producerstatisticshandler" + setting.Id, out value))
		{
			Action<IProducer<TKey, TValue>, string> statisticsHandler = (Action<IProducer<TKey, TValue>, string>)value;
			val.SetStatisticsHandler(statisticsHandler);
		}
		return val;
	}
}

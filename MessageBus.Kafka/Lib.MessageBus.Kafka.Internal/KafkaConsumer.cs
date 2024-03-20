using Lib.MessageBus.Kafka.Abstractions;
using Lib.MessageBus.Kafka.Settings;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Lib.MessageBus.Kafka.Internal;

internal class KafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue>, IKafkaConsumer, IDisposable
{
	private class ConsumeInfo
	{
		internal string BootstrapServers { get; set; }

		internal string GroupId { get; set; }

		internal IEnumerable<TopicPartitionOffset> TopicPartitionOffsets { get; set; }

		internal IEnumerable<string> Topics { get; set; }

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			IEnumerable<string> topics = Topics;
			StringBuilder stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler;
			if (topics != null && topics.Count() > 0)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder3 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(28, 1, stringBuilder2);
				handler.AppendLiteral("topic[s]: ");
				handler.AppendFormatted(string.Join(", ", Topics));
				handler.AppendLiteral(" | type: Subscribe");
				stringBuilder3.Append(handler);
			}
			IEnumerable<TopicPartitionOffset> topicPartitionOffsets = TopicPartitionOffsets;
			if (topicPartitionOffsets != null && topicPartitionOffsets.Count() > 0)
			{
				var value = TopicPartitionOffsets.Select(delegate(TopicPartitionOffset tpo)
				{
					string topic = tpo.Topic;
					Partition partition = tpo.Partition;
					int value3 = ((Partition)(partition)).Value;
					Offset offset = tpo.Offset;
					long? offset2;
					if (((Offset)(offset)).Value < 0)
					{
						offset2 = null;
					}
					else
					{
						offset = tpo.Offset;
						offset2 = ((Offset)(offset)).Value;
					}
					return new
					{
						Topic = topic,
						Partition = value3,
						Offset = offset2
					};
				});
				string value2 = JsonSerializer.Serialize(value);
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(" and ");
				}
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder4 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(40, 1, stringBuilder2);
				handler.AppendLiteral("topicPartitionOffset[s]: ");
				handler.AppendFormatted(value2);
				handler.AppendLiteral(" | type: Assign");
				stringBuilder4.Append(handler);
			}
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(36, 2, stringBuilder2);
			handler.AppendLiteral(" - groupId: ");
			handler.AppendFormatted(GroupId);
			handler.AppendLiteral(" | bootstrapServer[s]: ");
			handler.AppendFormatted(BootstrapServers);
			handler.AppendLiteral(".");
			stringBuilder5.Append(handler);
			return stringBuilder.ToString();
		}
	}

	private readonly ILogger _logger;

	private readonly string[] _topics;

	private readonly bool _enablePartitionEof;

	private readonly TimeSpan _consumeTimeout;

	private ConsumeInfo _consumeInfo = null;

	public bool IsDisposed { get; private set; }

	public bool IsFree { get; private set; }

	public bool IsRunning { get; private set; }

	public ConsumerSetting Setting { get; private set; }

	public IConsumer<TKey, TValue> Consumer { get; private set; }

	public KafkaConsumer(ConsumerSetting setting, ILogger logger)
	{
		Consumer = GetConsumerBuilder(setting).Build();
		Setting = setting;
		_logger = logger;
		_topics = Setting.GetTopics();
		_enablePartitionEof = setting.EnablePartitionEof.HasValue && setting.EnablePartitionEof.Value;
		_consumeTimeout = TimeSpan.FromSeconds(Setting.ConsumeTimeoutSecond);
		IsFree = true;
	}

	public void Consume(Action<ConsumeResult<TKey, TValue>> callback)
	{
		if (!IsRunning && !IsDisposed)
		{
			if (_consumeInfo == null)
			{
				_consumeInfo = SubscribeOrAssign();
			}
			LogInfo($"Now consuming {_consumeInfo}");
			IsRunning = true;
			if (_enablePartitionEof)
			{
				ConsumeLoopEnablePartitionEof(callback);
			}
			else
			{
				ConsumeLoop(callback);
			}
			if (IsDisposed)
			{
				Consumer.Close();
				((IDisposable)Consumer).Dispose();
			}
		}
	}

	private void ConsumeLoop(Action<ConsumeResult<TKey, TValue>> callback)
	{
		int num = 0;
		long num2 = 0L;
		string empty = string.Empty;
		while (IsRunning)
		{
			try
			{
				ConsumeResult<TKey, TValue> val = Consumer.Consume(_consumeTimeout);
				if (val != null)
				{
					IsFree = false;
					Partition partition = val.Partition;
					num = ((Partition)(partition)).Value;
					Offset offset = val.Offset;
					num2 = ((Offset)(offset)).Value;
					empty = val.Topic;
					callback(val);
				}
				else
				{
					IsFree = true;
				}
			}
			catch (Exception ex)
			{
				LogError(ex);
			}
		}
	}

	private void ConsumeLoopEnablePartitionEof(Action<ConsumeResult<TKey, TValue>> callback)
	{
		ConsumeResult<TKey, TValue> val = null;
		int num = 0;
		long num2 = 0L;
		string empty = string.Empty;
		while (IsRunning)
		{
			try
			{
				ConsumeResult<TKey, TValue> val2 = Consumer.Consume(_consumeTimeout);
				if (val2 != null)
				{
					IsFree = false;
					if (val != null)
					{
						Partition partition = val.Partition;
						num = ((Partition)(partition)).Value;
						Offset offset = val.Offset;
						num2 = ((Offset)(offset)).Value;
						empty = val.Topic;
						val.IsPartitionEOF = val2.IsPartitionEOF;
						callback(val);
					}
					val = ((!val2.IsPartitionEOF) ? val2 : null);
				}
				else
				{
					IsFree = true;
				}
			}
			catch (Exception ex)
			{
				LogError(ex);
			}
		}
	}

	public void Commit(ConsumeResult<TKey, TValue> result)
	{
		Consumer.Commit(result);
	}

	public void Commit(IEnumerable<TopicPartitionOffset> topicPartitionOffsets)
	{
		Consumer.Commit(topicPartitionOffsets);
	}

	public void Commit(int partition, long offset, string topic)
	{
		Consumer.Commit((IEnumerable<TopicPartitionOffset>)(object)new TopicPartitionOffset[1]
		{
			new TopicPartitionOffset(topic, partition, offset)
		});
	}

	public void Commit(int partition, long offset, int topicIndex = 0)
	{
		if (topicIndex < _topics.Length)
		{
			Commit(partition, offset, _topics[topicIndex]);
		}
	}

	public void Pause()
	{
		if (!IsDisposed && IsRunning)
		{
			IsRunning = false;
			IsFree = true;
			string value = string.Join(", ", _topics);
			LogInfo($"Paused consume topic[s]: {value} | groupId: {Setting.GroupId} | bootstrapServer[s]: {Setting.BootstrapServers}.");
		}
	}

	public void Dispose()
	{
		if (IsRunning)
		{
			IsRunning = false;
			string value = string.Join(", ", _topics);
			LogInfo($"Stoped consume topic[s]: {value} | groupId: {Setting.GroupId} | bootstrapServer[s]: {Setting.BootstrapServers}.");
		}
		if (!IsDisposed)
		{
			IsFree = false;
			IsDisposed = true;
		}
	}

	private ConsumeInfo SubscribeOrAssign()
	{
		IEnumerable<TopicPartitionOffset> topicPartitionOffsets = Setting.GetTopicPartitionOffsets();
		ConsumeInfo consumeInfo = new ConsumeInfo
		{
			BootstrapServers = Setting.BootstrapServers,
			GroupId = Setting.GroupId
		};
		IEnumerable<IGrouping<string, TopicPartitionOffset>> source = from tpo in topicPartitionOffsets
			group tpo by tpo.Topic;
		List<TopicPartitionOffset> list = source.Where((IGrouping<string, TopicPartitionOffset> group) => group.Any(delegate(TopicPartitionOffset tpo)
		{
			Partition partition2 = tpo.Partition;
			return ((Partition)(partition2)).Value >= ((Partition)(Partition.Any)).Value;
		})).SelectMany((IGrouping<string, TopicPartitionOffset> group) => group).ToList();
		List<string> list2 = (from @group in source
			where !@group.Any(delegate(TopicPartitionOffset tpo)
			{
				Partition partition = tpo.Partition;
				return ((Partition)(partition)).Value >= ((Partition)(Partition.Any)).Value;
			})
			select @group.Key).ToList();
		if (list2.Count > 0)
		{
			consumeInfo.Topics = list2;
			Consumer.Subscribe((IEnumerable<string>)list2);
		}
		if (list.Count > 0)
		{
			consumeInfo.TopicPartitionOffsets = list;
			Consumer.Assign((IEnumerable<TopicPartitionOffset>)list);
		}
		return consumeInfo;
	}

	private void LogInfo(string message)
	{
		if (_logger == null)
		{
			Console.WriteLine("info: " + typeof(KafkaConsumer<, >).FullName);
			Console.Out.WriteLine($"{string.Empty,6}{message}");
		}
		else
		{
			_logger.LogInformation(message);
		}
	}

	private void LogError(Exception ex)
	{
		if (_logger == null)
		{
			Console.WriteLine("fail: " + typeof(KafkaConsumer<, >).FullName);
			Console.Error.WriteLine($"{string.Empty,6}{ex.ToString()}");
		}
		else
		{
			_logger.LogError(ex, ex.Message);
		}
	}

	private ConsumerBuilder<TKey, TValue> GetConsumerBuilder(ConsumerSetting setting)
	{
		ConsumerBuilder<TKey, TValue> val = new ConsumerBuilder<TKey, TValue>((IEnumerable<KeyValuePair<string, string>>)setting.GetConsumerConfig());
		if (Utils.InternalStore.Read("consumererrorhandler" + setting.Id, out var value))
		{
			Action<IConsumer<TKey, TValue>, Error> errorHandler = (Action<IConsumer<TKey, TValue>, Error>)value;
			val.SetErrorHandler(errorHandler);
		}
		if (Utils.InternalStore.Read("consumerkeydeserializer" + setting.Id, out value))
		{
			IDeserializer<TKey> keyDeserializer = (IDeserializer<TKey>)value;
			val.SetKeyDeserializer(keyDeserializer);
		}
		if (Utils.InternalStore.Read("consumerloghandler" + setting.Id, out value))
		{
			Action<IConsumer<TKey, TValue>, LogMessage> logHandler = (Action<IConsumer<TKey, TValue>, LogMessage>)value;
			val.SetLogHandler(logHandler);
		}
		if (Utils.InternalStore.Read("consumervaluedeserializer" + setting.Id, out value))
		{
			IDeserializer<TValue> valueDeserializer = (IDeserializer<TValue>)value;
			val.SetValueDeserializer(valueDeserializer);
		}
		if (Utils.InternalStore.Read("consumeroffsetscommittedhandler" + setting.Id, out value))
		{
			Action<IConsumer<TKey, TValue>, CommittedOffsets> offsetsCommittedHandler = (Action<IConsumer<TKey, TValue>, CommittedOffsets>)value;
			val.SetOffsetsCommittedHandler(offsetsCommittedHandler);
		}
		if (Utils.InternalStore.Read("consumerpartitionsassignedhandler" + setting.Id, out value))
		{
			Func<IConsumer<TKey, TValue>, List<TopicPartition>, IEnumerable<TopicPartitionOffset>> partitionsAssignedHandler = (Func<IConsumer<TKey, TValue>, List<TopicPartition>, IEnumerable<TopicPartitionOffset>>)value;
			val.SetPartitionsAssignedHandler(partitionsAssignedHandler);
		}
		if (Utils.InternalStore.Read("_consumerpartitionsassignedhandler" + setting.Id, out value))
		{
			Action<IConsumer<TKey, TValue>, List<TopicPartition>> partitionsAssignedHandler2 = (Action<IConsumer<TKey, TValue>, List<TopicPartition>>)value;
			val.SetPartitionsAssignedHandler(partitionsAssignedHandler2);
		}
		if (Utils.InternalStore.Read("consumerpartitionslosthandler" + setting.Id, out value))
		{
			Func<IConsumer<TKey, TValue>, List<TopicPartitionOffset>, IEnumerable<TopicPartitionOffset>> partitionsLostHandler = (Func<IConsumer<TKey, TValue>, List<TopicPartitionOffset>, IEnumerable<TopicPartitionOffset>>)value;
			val.SetPartitionsLostHandler(partitionsLostHandler);
		}
		if (Utils.InternalStore.Read("_consumerpartitionslosthandler" + setting.Id, out value))
		{
			Action<IConsumer<TKey, TValue>, List<TopicPartitionOffset>> partitionsLostHandler2 = (Action<IConsumer<TKey, TValue>, List<TopicPartitionOffset>>)value;
			val.SetPartitionsLostHandler(partitionsLostHandler2);
		}
		if (Utils.InternalStore.Read("consumerpartitionsrevokedhandler" + setting.Id, out value))
		{
			Func<IConsumer<TKey, TValue>, List<TopicPartitionOffset>, IEnumerable<TopicPartitionOffset>> partitionsRevokedHandler = (Func<IConsumer<TKey, TValue>, List<TopicPartitionOffset>, IEnumerable<TopicPartitionOffset>>)value;
			val.SetPartitionsRevokedHandler(partitionsRevokedHandler);
		}
		if (Utils.InternalStore.Read("_consumerpartitionsrevokedhandler" + setting.Id, out value))
		{
			Action<IConsumer<TKey, TValue>, List<TopicPartitionOffset>> partitionsRevokedHandler2 = (Action<IConsumer<TKey, TValue>, List<TopicPartitionOffset>>)value;
			val.SetPartitionsRevokedHandler(partitionsRevokedHandler2);
		}
		if (Utils.InternalStore.Read("consumerstatisticshandler" + setting.Id, out value))
		{
			Action<IConsumer<TKey, TValue>, string> statisticsHandler = (Action<IConsumer<TKey, TValue>, string>)value;
			val.SetStatisticsHandler(statisticsHandler);
		}
		return val;
	}
}

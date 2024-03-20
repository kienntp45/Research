using Confluent.Kafka;
using Lib.MessageBus.Kafka.Settings;
using System.Collections;

namespace Lib.MessageBus.Kafka.Extensions;

internal static class ConsumerSettingExtensions
{
	internal static ConsumerConfig GetKafkaConsumerConfig(this ConsumerSetting @this)
	{
		ConsumerConfig val = new ConsumerConfig
		{
			GroupId = @this.GroupId,
			SessionTimeoutMs = @this.SessionTimeoutMs,
			QueuedMinMessages = @this.QueuedMinMessages,
			AutoOffsetReset = @this.AutoOffsetReset,
			EnableAutoCommit = @this.EnableAutoCommit,
			EnableAutoOffsetStore = @this.EnableAutoOffsetStore,
			EnablePartitionEof = @this.EnablePartitionEof,
			FetchMaxBytes = @this.FetchMaxBytes,
			FetchMinBytes = @this.FetchMinBytes,
			FetchWaitMaxMs = @this.FetchWaitMaxMs,
			MaxPartitionFetchBytes = @this.MaxPartitionFetchBytes,
			PartitionAssignmentStrategy = @this.PartitionAssignmentStrategy,
			MaxPollIntervalMs = @this.MaxPollIntervalMs,
			IsolationLevel = @this.IsolationLevel,
			QueuedMaxMessagesKbytes = @this.QueuedMaxMessagesKbytes,
			GroupInstanceId = @this.GroupInstanceId,
			CheckCrcs = @this.CheckCrcs,
			AutoCommitIntervalMs = @this.AutoCommitIntervalMs,
			CoordinatorQueryIntervalMs = @this.CoordinatorQueryIntervalMs,
			FetchErrorBackoffMs = @this.FetchErrorBackoffMs,
			HeartbeatIntervalMs = @this.HeartbeatIntervalMs
		};
		Utils.SetPropertyValuesForClientConfig((ClientConfig)(object)val, @this);
		return val;
	}

	internal static string[] GetTopicsFromTopicSetting(this ConsumerSetting @this)
	{
		HashSet<string> hashSet = new HashSet<string>();
		string[] array = @this.Topic.Split(',');
		foreach (string text in array)
		{
			string[] array2 = text.Split(':');
			string text2 = array2[0].Trim();
			if (text2.Length > 0)
			{
				hashSet.Add(text2);
			}
		}
		return hashSet.ToArray();
	}

	internal static ConsumerSetting CloneSetting(this ConsumerSetting @this)
	{
		ConsumerSetting consumerSetting = new ConsumerSetting
		{
			QueuedMinMessages = @this.QueuedMinMessages,
			AutoOffsetReset = @this.AutoOffsetReset,
			EnableAutoCommit = @this.EnableAutoCommit,
			EnablePartitionEof = @this.EnablePartitionEof,
			EnableAutoOffsetStore = @this.EnableAutoOffsetStore,
			SessionTimeoutMs = @this.SessionTimeoutMs,
			GroupId = @this.GroupId,
			FetchMaxBytes = @this.FetchMaxBytes,
			FetchMinBytes = @this.FetchMinBytes,
			FetchWaitMaxMs = @this.FetchWaitMaxMs,
			MaxPartitionFetchBytes = @this.MaxPartitionFetchBytes,
			PartitionAssignmentStrategy = @this.PartitionAssignmentStrategy,
			ConsumeTimeoutSecond = @this.ConsumeTimeoutSecond,
			MaxPollIntervalMs = @this.MaxPollIntervalMs,
			IsolationLevel = @this.IsolationLevel,
			QueuedMaxMessagesKbytes = @this.QueuedMaxMessagesKbytes,
			GroupInstanceId = @this.GroupInstanceId,
			CheckCrcs = @this.CheckCrcs,
			AutoCommitIntervalMs = @this.AutoCommitIntervalMs,
			CoordinatorQueryIntervalMs = @this.CoordinatorQueryIntervalMs,
			FetchErrorBackoffMs = @this.FetchErrorBackoffMs,
			HeartbeatIntervalMs = @this.HeartbeatIntervalMs
		};
		Utils.CopyBaseSetting(consumerSetting, @this);
		return consumerSetting;
	}

	internal static List<TopicPartitionOffset> GetTopicPartitionOffsetFromTopicSetting(this ConsumerSetting @this)
	{
		List<TopicPartitionOffset> list = new List<TopicPartitionOffset>();
		string[] array = @this.Topic.Split(',');
		foreach (string text in array)
		{
			string[] array2 = text.Split(':');
			string topic = array2[0].Trim();
			if (topic.Length <= 0 || list.Any((TopicPartitionOffset tpo) => tpo.Topic == topic))
			{
				continue;
			}
			if (array2.Length > 1)
			{
				string text2 = array2[1].Trim();
				string text3 = text2.ToUpper();
				string text4 = text3;
				int result;
				IEnumerable<Partition> enumerable = (IEnumerable<Partition>)((text4 == "ALL") ? GetPartitionsOfTopic(@this.BootstrapServers, topic) : ((!(text4 == "ANY")) ? ((IEnumerable)(from p in text2.Split(' ')
					select (Partition)(int.TryParse(p, out result) ? new Partition(result) : Partition.Any) into p
					where ((Partition)(p)).Value >= 0
					select p).Distinct()) : ((IEnumerable)new Partition[1] { Partition.Any })));
				if (enumerable.Count() > 0)
				{
					foreach (Partition item in enumerable)
					{
						list.Add(new TopicPartitionOffset(topic, item, Offset.Unset));
					}
				}
				else
				{
					list.Add(new TopicPartitionOffset(topic, new Partition(int.MinValue), Offset.Unset));
				}
			}
			else
			{
				list.Add(new TopicPartitionOffset(topic, new Partition(int.MinValue), Offset.Unset));
			}
		}
		return list;
	}

	private static Partition[] GetPartitionsOfTopic(string bootstrapsServer, string topic)
	{
		AdminClientConfig val = new AdminClientConfig
		{
			BootstrapServers = bootstrapsServer
		};
		IAdminClient val2 = new AdminClientBuilder((IEnumerable<KeyValuePair<string, string>>)val).Build();
		try
		{
			Metadata metadata = val2.GetMetadata(topic, TimeSpan.FromSeconds(6.0));
			TopicMetadata val3 = metadata.Topics.Single();
			return ((IEnumerable<PartitionMetadata>)val3.Partitions).Select((Func<PartitionMetadata, Partition>)((PartitionMetadata partitionMetadata) => new Partition(partitionMetadata.PartitionId))).ToArray();
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}
}

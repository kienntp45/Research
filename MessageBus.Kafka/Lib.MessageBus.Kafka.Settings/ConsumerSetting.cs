using Lib.MessageBus.Kafka.Extensions;

namespace Lib.MessageBus.Kafka.Settings
{
    public class ConsumerSetting : BaseSetting
    {
        public string GroupId { get; set; } = Guid.NewGuid().ToString();


        public AutoOffsetReset? AutoOffsetReset { get; set; }

        public bool? EnableAutoCommit { get; set; }

        public bool? EnablePartitionEof { get; set; }

        public bool? EnableAutoOffsetStore { get; set; }

        public int? SessionTimeoutMs { get; set; }

        public int? QueuedMinMessages { get; set; }

        public int? FetchMaxBytes { get; set; }

        public int? FetchMinBytes { get; set; }

        public int? FetchWaitMaxMs { get; set; }

        public int? MaxPartitionFetchBytes { get; set; }

        public PartitionAssignmentStrategy? PartitionAssignmentStrategy { get; set; }

        public int? MaxPollIntervalMs { get; set; }

        public IsolationLevel? IsolationLevel { get; set; }

        public int? QueuedMaxMessagesKbytes { get; set; }

        public int ConsumeTimeoutSecond { get; set; }

        public string GroupInstanceId { get; set; }

        public bool? CheckCrcs { get; set; }

        public int? AutoCommitIntervalMs { get; set; }

        public int? CoordinatorQueryIntervalMs { get; set; }

        public int? FetchErrorBackoffMs { get; set; }

        public int? HeartbeatIntervalMs { get; set; }

        private List<TopicPartitionOffset> TopicPartitionOffsets { get; set; }

        private string[] Topics { get; set; }

        public string[] GetTopics()
        {
            if (Topics == null)
            {
                Topics = this.GetTopicsFromTopicSetting();
            }
            return Topics;
        }

        public static ConsumerSetting MapValue(IConfiguration configuration, string defaultBootstrapServers = null)
        {
            return Utils.GetConsumerSettingFromConfiguration(configuration, defaultBootstrapServers);
        }

        public IEnumerable<TopicPartitionOffset> GetTopicPartitionOffsets()
        {
            if (TopicPartitionOffsets == null)
            {
                TopicPartitionOffsets = this.GetTopicPartitionOffsetFromTopicSetting();
            }
            return TopicPartitionOffsets;
        }

        public void SetPartitionOffset(int partition, long offset, int topicIndex = 0)
        {
            if (topicIndex >= 0 && topicIndex < GetTopics().Length)
            {
                string topic = GetTopics()[topicIndex];
                SetPartitionOffset(partition, offset, topic);
            }
        }

        public void CommitOffsets(Dictionary<int, long> partitionOffsets, int topicIndex = 0)
        {
            if (topicIndex >= 0 && topicIndex < GetTopics().Length)
            {
                string text = GetTopics()[topicIndex];
                if (!string.IsNullOrWhiteSpace(text))
                {
                    Utils.CommitOffsets(text, this.GetKafkaConsumerConfig(), partitionOffsets);
                }
            }
        }

        public void SetPartitionOffset(int partition, long offset, string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                return;
            }
            if (TopicPartitionOffsets == null)
            {
                TopicPartitionOffsets = this.GetTopicPartitionOffsetFromTopicSetting();
            }
            int num = TopicPartitionOffsets.FindIndex(delegate (TopicPartitionOffset tpo)
            {
                int result;
                if (tpo.Topic == topic)
                {
                    Partition partition2 = tpo.Partition;
                    result = ((((Partition)(partition2)).Value == partition) ? 1 : 0);
                }
                else
                {
                    result = 0;
                }
                return (byte)result != 0;
            });
            if (num >= 0)
            {
                TopicPartitionOffsets[num] = new TopicPartitionOffset(topic, new Partition(partition), new Offset(offset));
            }
        }

        public ConsumerConfig GetConsumerConfig()
        {
            return this.GetKafkaConsumerConfig();
        }

        public ConsumerSetting Clone()
        {
            return this.CloneSetting();
        }

        public void SetErrorHandler<TKey, TValue>(Action<IConsumer<TKey, TValue>, Error> handler)
        {
            Utils.SetSettingHandler("consumererrorhandler" + base.Id, handler);
        }

        public void SetKeyDeserializer<TKey>(IDeserializer<TKey> deserializer)
        {
            Utils.SetSettingHandler("consumerkeydeserializer" + base.Id, deserializer);
        }

        public void SetValueDeserializer<TValue>(IDeserializer<TValue> deserializer)
        {
            Utils.SetSettingHandler("consumervaluedeserializer" + base.Id, deserializer);
        }

        public void SetLogHandler<TKey, TValue>(Action<IConsumer<TKey, TValue>, LogMessage> handler)
        {
            Utils.SetSettingHandler("consumerloghandler" + base.Id, handler);
        }

        public void SetOffsetsCommittedHandler<TKey, TValue>(Action<IConsumer<TKey, TValue>, CommittedOffsets> handler)
        {
            Utils.SetSettingHandler("consumeroffsetscommittedhandler" + base.Id, handler);
        }

        public void SetPartitionsAssignedHandler<TKey, TValue>(Func<IConsumer<TKey, TValue>, List<TopicPartition>, IEnumerable<TopicPartitionOffset>> handler)
        {
            Utils.SetSettingHandler("consumerpartitionsassignedhandler" + base.Id, handler);
        }

        public void SetPartitionsAssignedHandler<TKey, TValue>(Action<IConsumer<TKey, TValue>, List<TopicPartition>> handler)
        {
            Utils.SetSettingHandler("_consumerpartitionsassignedhandler" + base.Id, handler);
        }

        public void SetPartitionsLostHandler<TKey, TValue>(Func<IConsumer<TKey, TValue>, List<TopicPartitionOffset>, IEnumerable<TopicPartitionOffset>> handler)
        {
            Utils.SetSettingHandler("consumerpartitionslosthandler" + base.Id, handler);
        }

        public void SetPartitionsLostHandler<TKey, TValue>(Action<IConsumer<TKey, TValue>, List<TopicPartitionOffset>> handler)
        {
            Utils.SetSettingHandler("_consumerpartitionslosthandler" + base.Id, handler);
        }

        public void SetPartitionsRevokedHandler<TKey, TValue>(Func<IConsumer<TKey, TValue>, List<TopicPartitionOffset>, IEnumerable<TopicPartitionOffset>> handler)
        {
            Utils.SetSettingHandler("consumerpartitionsrevokedhandler" + base.Id, handler);
        }

        public void SetPartitionsRevokedHandler<TKey, TValue>(Action<IConsumer<TKey, TValue>, List<TopicPartitionOffset>> handler)
        {
            Utils.SetSettingHandler("_consumerpartitionsrevokedhandler" + base.Id, handler);
        }

        public void SetStatisticsHandler<TKey, TValue>(Action<IConsumer<TKey, TValue>, string> handler)
        {
            Utils.SetSettingHandler("consumerstatisticshandler" + base.Id, handler);
        }

        public override int GetHashCode()
        {
            return base.Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(ConsumerSetting))
            {
                return false;
            }
            ConsumerSetting consumerSetting = (ConsumerSetting)obj;
            return base.Id == consumerSetting.Id;
        }
    }

}

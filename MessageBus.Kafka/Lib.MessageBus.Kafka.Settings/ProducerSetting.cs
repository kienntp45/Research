using Lib.MessageBus.Kafka.Extensions;

namespace Lib.MessageBus.Kafka.Settings;

public class ProducerSetting : BaseSetting
{
    public bool? EnableDeliveryReports { get; set; }

    public string DeliveryReportFields { get; set; }

    public int? TransactionTimeoutMs { get; set; }

    public Partitioner? Partitioner { get; set; }

    public CompressionType? CompressionType { get; set; }

    public int? CompressionLevel { get; set; }

    public int? QueueBufferingMaxMessages { get; set; }

    public int? QueueBufferingMaxKbytes { get; set; }

    public int? QueueBufferingBackpressureThreshold { get; set; }

    public int? BatchSize { get; set; }

    public int? BatchNumMessages { get; set; }

    public int? MessageSendMaxRetries { get; set; }

    public int? MessageTimeoutMs { get; set; }

    public int? RetryBackoffMs { get; set; }

    public double? LingerMs { get; set; }

    public Acks? Acks { get; set; }

    public int PollTimeoutSecond { get; set; }

    public bool? EnableIdempotence { get; set; }

    public bool? EnableBackgroundPoll { get; set; }

    public bool? EnableGaplessGuarantee { get; set; }

    public int? RequestTimeoutMs { get; set; }

    public int? StatisticsIntervalMs { get; set; }

    public int? StickyPartitioningLingerMs { get; set; }

    private TopicPartition TopicPartition { get; set; }

    public static ProducerSetting MapValue(IConfiguration configuration, string defaultBootstrapServers = null)
    {
        return Utils.GetProducerSettingFromConfiguration(configuration, defaultBootstrapServers);
    }

    public ProducerConfig GetProducerConfig()
    {
        return this.GetKafkaProducerConfig();
    }

    public ProducerSetting Clone()
    {
        return this.CloneSetting();
    }

    public TopicPartition GetTopicPartition()
    {
        if (TopicPartition == (TopicPartition)null)
        {
            TopicPartition = this.GetTopicPartitionTopicSetting();
        }
        return TopicPartition;
    }

    public void SetErrorHandler<TKey, TValue>(Action<IProducer<TKey, TValue>, Error> handler)
    {
        Utils.SetSettingHandler("producererrorhandler" + base.Id, handler);
    }

    public void SetKeySerializer<TKey>(ISerializer<TKey> serializer)
    {
        Utils.SetSettingHandler("producerkeyserializer" + base.Id, serializer);
    }

    public void SetKeySerializer<TKey>(IAsyncSerializer<TKey> serializer)
    {
        Utils.SetSettingHandler("_producerkeyserializer" + base.Id, serializer);
    }

    public void SetValueSerializer<TValue>(ISerializer<TValue> serializer)
    {
        Utils.SetSettingHandler("producervalueserializer" + base.Id, serializer);
    }

    public void SetValueSerializer<TValue>(IAsyncSerializer<TValue> serializer)
    {
        Utils.SetSettingHandler("_producervalueserializer" + base.Id, serializer);
    }

    public void SetLogHandler<TKey, TValue>(Action<IProducer<TKey, TValue>, LogMessage> handler)
    {
        Utils.SetSettingHandler("producerloghandler" + base.Id, handler);
    }

    public void SetDefaultPartitioner(PartitionerDelegate partitioner)
    {
        Utils.SetSettingHandler("producerdefaultpartitioner" + base.Id, partitioner);
    }

    public void SetPartitioner(PartitionerDelegate partitioner)
    {
        Utils.SetSettingHandler("producerpartitioner" + base.Id, partitioner);
    }

    public void SetStatisticsHandler<TKey, TValue>(Action<IProducer<TKey, TValue>, string> handler)
    {
        Utils.SetSettingHandler("producerstatisticshandler" + base.Id, handler);
    }

    public override int GetHashCode()
    {
        return base.Id.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj.GetType() != typeof(ProducerSetting))
        {
            return false;
        }
        ProducerSetting producerSetting = (ProducerSetting)obj;
        return base.Id == producerSetting.Id;
    }
}

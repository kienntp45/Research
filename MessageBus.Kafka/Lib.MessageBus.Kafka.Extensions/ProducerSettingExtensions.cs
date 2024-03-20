using Confluent.Kafka;
using Lib.MessageBus.Kafka.Internal;
using Lib.MessageBus.Kafka.Settings;

namespace Lib.MessageBus.Kafka.Extensions;

internal static class ProducerSettingExtensions
{
	internal static ProducerConfig GetKafkaProducerConfig(this ProducerSetting @this)
	{
		ProducerConfig val = new ProducerConfig
		{
			LingerMs = @this.LingerMs,
			Acks = @this.Acks,
			QueueBufferingMaxMessages = @this.QueueBufferingMaxMessages,
			QueueBufferingMaxKbytes = @this.QueueBufferingMaxKbytes,
			QueueBufferingBackpressureThreshold = @this.QueueBufferingBackpressureThreshold,
			MessageSendMaxRetries = @this.MessageSendMaxRetries,
			RetryBackoffMs = @this.RetryBackoffMs,
			MessageTimeoutMs = @this.MessageTimeoutMs,
			TransactionTimeoutMs = @this.TransactionTimeoutMs,
			Partitioner = @this.Partitioner,
			CompressionType = @this.CompressionType,
			CompressionLevel = @this.CompressionLevel,
			BatchNumMessages = @this.BatchNumMessages,
			BatchSize = @this.BatchSize,
			EnableDeliveryReports = @this.EnableDeliveryReports,
			DeliveryReportFields = (string.IsNullOrWhiteSpace(@this.DeliveryReportFields) ? "all" : @this.DeliveryReportFields),
			EnableBackgroundPoll = @this.EnableBackgroundPoll,
			EnableIdempotence = @this.EnableIdempotence,
			EnableGaplessGuarantee = @this.EnableGaplessGuarantee,
			RequestTimeoutMs = @this.RequestTimeoutMs,
			StatisticsIntervalMs = @this.StatisticsIntervalMs,
			StickyPartitioningLingerMs = @this.StickyPartitioningLingerMs
		};
		Utils.SetPropertyValuesForClientConfig((ClientConfig)(object)val, @this);
		return val;
	}

	internal static TopicPartition GetTopicPartitionTopicSetting(this ProducerSetting @this)
	{
		int num = @this.Topic.IndexOf(':');
		if (num == -1)
		{
			return new TopicPartition(@this.Topic.Trim(), Partition.Any);
		}
		string text = @this.Topic.Substring(0, num).Trim();
		if (int.TryParse(@this.Topic.Substring(num + 1), out var result) && result >= 0)
		{
			return new TopicPartition(text, new Partition(result));
		}
		return new TopicPartition(text, Partition.Any);
	}

	internal static ProducerSetting CloneSetting(this ProducerSetting @this)
	{
		ProducerSetting producerSetting = new ProducerSetting
		{
			QueueBufferingMaxMessages = @this.QueueBufferingMaxMessages,
			QueueBufferingMaxKbytes = @this.QueueBufferingMaxKbytes,
			QueueBufferingBackpressureThreshold = @this.QueueBufferingBackpressureThreshold,
			MessageSendMaxRetries = @this.MessageSendMaxRetries,
			RetryBackoffMs = @this.RetryBackoffMs,
			MessageTimeoutMs = @this.MessageTimeoutMs,
			LingerMs = @this.LingerMs,
			TransactionTimeoutMs = @this.TransactionTimeoutMs,
			Acks = @this.Acks,
			Partitioner = @this.Partitioner,
			CompressionType = @this.CompressionType,
			CompressionLevel = @this.CompressionLevel,
			BatchSize = @this.BatchSize,
			BatchNumMessages = @this.BatchNumMessages,
			PollTimeoutSecond = @this.PollTimeoutSecond,
			EnableDeliveryReports = @this.EnableDeliveryReports,
			DeliveryReportFields = @this.DeliveryReportFields,
			EnableIdempotence = @this.EnableIdempotence,
			EnableBackgroundPoll = @this.EnableBackgroundPoll,
			EnableGaplessGuarantee = @this.EnableGaplessGuarantee,
			RequestTimeoutMs = @this.RequestTimeoutMs,
			StatisticsIntervalMs = @this.StatisticsIntervalMs,
			StickyPartitioningLingerMs = @this.StickyPartitioningLingerMs
		};
		Utils.CopyBaseSetting(producerSetting, @this);
		return producerSetting;
	}
}

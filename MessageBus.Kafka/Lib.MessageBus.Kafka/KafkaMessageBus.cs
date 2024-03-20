using Lib.MessageBus.Kafka.Abstractions;
using Lib.MessageBus.Kafka.Internal;

namespace Lib.MessageBus.Kafka;

public static class KafkaMessageBus
{
	public static IKafkaConsumerManager ConsumerManager => Utils.KafkaConsumerManager;

	public static IKafkaProducerManager ProducerManager => Utils.KafkaProducerManager;
}

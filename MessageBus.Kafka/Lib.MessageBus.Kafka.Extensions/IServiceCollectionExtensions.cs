using Lib.MessageBus.Kafka.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Lib.MessageBus.Kafka.Extensions;

public static class IServiceCollectionExtensions
{
	public static IServiceCollection AddKafkaConsumers(this IServiceCollection services)
	{
		return Utils.AddKafkaConsumers(services);
	}

	public static IServiceCollection AddKafkaConsumers(this IServiceCollection services, Action<IKafkaConsumerManager> consumerManagerBuilder)
	{
		return Utils.AddKafkaConsumers(services, consumerManagerBuilder);
	}

	public static IServiceCollection AddKafkaProducers(this IServiceCollection services)
	{
		return Utils.AddKafkaProducers(services);
	}

	public static IServiceCollection AddKafkaProducers(this IServiceCollection services, Action<IKafkaProducerManager> producerManagerBuilder)
	{
		return Utils.AddKafkaProducers(services, producerManagerBuilder);
	}
}

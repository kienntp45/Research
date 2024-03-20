using Lib.MessageBus.Kafka.Abstractions;
using Microsoft.Extensions.Hosting;

namespace Lib.MessageBus.Kafka.Extensions;

public static class IHostExtensions
{
	public static IHost UseKafkaMessageBus(this IHost host)
	{
		return Utils.UseKafkaMessageBus(host);
	}

	public static IHost UseKafkaMessageBus(this IHost host, Action<IKafkaConsumerManager> action)
	{
		return Utils.UseKafkaMessageBus(host, action);
	}

	public static IHost UseKafkaMessageBus(this IHost host, Action<IKafkaConsumerManager, IServiceProvider> action)
	{
		return Utils.UseKafkaMessageBus(host, action);
	}
}

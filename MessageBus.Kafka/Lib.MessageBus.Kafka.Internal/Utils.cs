using Confluent.Kafka.Admin;
using Lib.MessageBus.Kafka.Abstractions;
using Lib.MessageBus.Kafka.Extensions;
using Lib.MessageBus.Kafka.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.RegularExpressions;

namespace Lib.MessageBus.Kafka.Internal;

internal static class Utils
{
	internal const string ConsumerPrefix = "consumer";

	internal const string ProducerPrefix = "producer";

	internal const string ConsumerErrorHandlerPrefix = "consumererrorhandler";

	internal const string ConsumerLogHandlerPrefix = "consumerloghandler";

	internal const string ConsumerKeyDeserializerHandlerPrefix = "consumerkeydeserializer";

	internal const string ConsumerValueDeserializerHandlerPrefix = "consumervaluedeserializer";

	internal const string ConsumerOffsetsCommittedHandlerPrefix = "consumeroffsetscommittedhandler";

	internal const string ConsumerPartitionsAssignedHandlerPrefix = "consumerpartitionsassignedhandler";

	internal const string ConsumerPartitionsLostHandlerPrefix = "consumerpartitionslosthandler";

	internal const string ConsumerPartitionsRevokedHandlerPrefix = "consumerpartitionsrevokedhandler";

	internal const string ConsumerStatisticsHandlerPrefix = "consumerstatisticshandler";

	internal const string ProducerErrorHandlerPrefix = "producererrorhandler";

	internal const string ProducerLogHandlerPrefix = "producerloghandler";

	internal const string ProducerKeySerializerHandlerPrefix = "producerkeyserializer";

	internal const string ProducerValueSerializerHandlerPrefix = "producervalueserializer";

	internal const string ProducerDefaultPartitionerPrefix = "producerdefaultpartitioner";

	internal const string ProducerPartitionerPrefix = "producerpartitioner";

	internal const string ProducerStatisticsHandlerPrefix = "producerstatisticshandler";

	internal const string KCMSettingPrefix = "kcmsetting";

	internal const string KCMConsumerPrefix = "kcmconsumer";

	internal const string KCMTaskPrefix = "kcmtask";

	internal const string KPMProducerPrefix = "kpmproducer";

	internal const string KPMSettingPrefix = "kpmsetting";

	internal static InternalStore InternalStore = new InternalStore(64);

	internal const int DefaultTimeoutSecond = 3;

	internal static IKafkaConsumerManager KafkaConsumerManager = new KafkaConsumerManager();

	internal static IKafkaProducerManager KafkaProducerManager = new KafkaProducerManager();

	internal static ProducerSetting GetProducerSettingFromConfiguration(IConfiguration configuration, string defaultBootstrapServers = null)
	{
		ProducerSetting producerSetting = new ProducerSetting
		{
			QueueBufferingMaxMessages = configuration.GetValue<int?>("QueueBufferingMaxMessages"),
			QueueBufferingMaxKbytes = configuration.GetValue<int?>("QueueBufferingMaxKbytes"),
			QueueBufferingBackpressureThreshold = configuration.GetValue<int?>("QueueBufferingBackpressureThreshold"),
			MessageSendMaxRetries = configuration.GetValue<int?>("MessageSendMaxRetries"),
			RetryBackoffMs = configuration.GetValue<int?>("RetryBackoffMs"),
			MessageTimeoutMs = configuration.GetValue<int?>("MessageTimeoutMs"),
			LingerMs = configuration.GetValue<int?>("LingerMs"),
			TransactionTimeoutMs = configuration.GetValue<int?>("TransactionTimeoutMs"),
			Acks = configuration.GetValue<Acks?>("Acks"),
			Partitioner = configuration.GetValue<Partitioner?>("Partitioner"),
			CompressionType = configuration.GetValue<CompressionType?>("CompressionType"),
			CompressionLevel = configuration.GetValue<int?>("CompressionLevel"),
			BatchSize = configuration.GetValue<int?>("BatchSize"),
			BatchNumMessages = configuration.GetValue<int?>("BatchNumMessages"),
			PollTimeoutSecond = configuration.GetValue("PollTimeoutSecond", 3),
			EnableDeliveryReports = configuration.GetValue<bool?>("EnableDeliveryReports"),
			DeliveryReportFields = configuration.GetValue("DeliveryReportFields", "all"),
			EnableIdempotence = configuration.GetValue<bool?>("EnableIdempotence"),
			EnableBackgroundPoll = configuration.GetValue<bool?>("EnableBackgroundPoll"),
			EnableGaplessGuarantee = configuration.GetValue<bool?>("EnableGaplessGuarantee"),
			RequestTimeoutMs = configuration.GetValue<int?>("RequestTimeoutMs"),
			StatisticsIntervalMs = configuration.GetValue<int?>("StatisticsIntervalMs"),
			StickyPartitioningLingerMs = configuration.GetValue<int?>("StickyPartitioningLingerMs")
		};
		SetPropertyValuesForBaseSetting(producerSetting, configuration, defaultBootstrapServers);
		producerSetting.Topic = Regex.Replace(producerSetting.Topic, "\\s+", string.Empty);
		return producerSetting;
	}

	internal static ConsumerSetting GetConsumerSettingFromConfiguration(IConfiguration configuration, string defaultBootstrapServers = null)
	{
		ConsumerSetting consumerSetting = new ConsumerSetting
		{
			QueuedMinMessages = configuration.GetValue<int?>("QueuedMinMessages"),
			AutoOffsetReset = configuration.GetValue<AutoOffsetReset?>("AutoOffsetReset"),
			EnableAutoCommit = configuration.GetValue<bool?>("EnableAutoCommit"),
			EnablePartitionEof = configuration.GetValue<bool?>("EnablePartitionEof"),
			EnableAutoOffsetStore = configuration.GetValue<bool?>("EnableAutoOffsetStore"),
			SessionTimeoutMs = configuration.GetValue<int?>("SessionTimeoutMs"),
			FetchMaxBytes = configuration.GetValue<int?>("FetchMaxBytes"),
			FetchMinBytes = configuration.GetValue<int?>("FetchMinBytes"),
			FetchWaitMaxMs = configuration.GetValue<int?>("FetchWaitMaxMs"),
			MaxPartitionFetchBytes = configuration.GetValue<int?>("MaxPartitionFetchBytes"),
			PartitionAssignmentStrategy = configuration.GetValue<PartitionAssignmentStrategy?>("PartitionAssignmentStrategy"),
			ConsumeTimeoutSecond = configuration.GetValue("ConsumeTimeoutSecond", 3),
			MaxPollIntervalMs = configuration.GetValue<int?>("MaxPollIntervalMs"),
			IsolationLevel = configuration.GetValue<IsolationLevel?>("IsolationLevel"),
			QueuedMaxMessagesKbytes = configuration.GetValue<int?>("QueuedMaxMessagesKbytes"),
			GroupInstanceId = configuration.GetValue<string>("GroupInstanceId"),
			CheckCrcs = configuration.GetValue<bool?>("CheckCrcs"),
			AutoCommitIntervalMs = configuration.GetValue<int?>("AutoCommitIntervalMs"),
			CoordinatorQueryIntervalMs = configuration.GetValue<int?>("CoordinatorQueryIntervalMs"),
			FetchErrorBackoffMs = configuration.GetValue<int?>("FetchErrorBackoffMs"),
			HeartbeatIntervalMs = configuration.GetValue<int?>("HeartbeatIntervalMs")
		};
		SetPropertyValuesForBaseSetting(consumerSetting, configuration, defaultBootstrapServers);
		consumerSetting.Topic = Regex.Replace(consumerSetting.Topic.Trim(), "\\s+", " ");
		consumerSetting.GroupId = configuration.GetValue("GroupId", consumerSetting.Id).Trim();
		return consumerSetting;
	}

	private static void SetPropertyValuesForBaseSetting(BaseSetting setting, IConfiguration configuration, string defaultBootstrapServers)
	{
		setting.Id = configuration.GetValue("Id", Guid.NewGuid().ToString());
		setting.BootstrapServers = configuration.GetValue("BootstrapServers", defaultBootstrapServers);
		setting.ConnectionsMaxIdleMs = configuration.GetValue<int?>("ConnectionsMaxIdleMs");
		setting.Debug = configuration.GetValue<string>("Debug");
		setting.MaxInFlight = configuration.GetValue<int?>("MaxInFlight");
		setting.MessageCopyMaxBytes = configuration.GetValue<int?>("MessageCopyMaxBytes");
		setting.MessageMaxBytes = configuration.GetValue<int?>("MessageMaxBytes");
		setting.MetadataMaxAgeMs = configuration.GetValue<int?>("MetadataMaxAgeMs");
		setting.ReceiveMessageMaxBytes = configuration.GetValue<int?>("ReceiveMessageMaxBytes");
		setting.ReconnectBackoffMaxMs = configuration.GetValue<int?>("ReconnectBackoffMaxMs");
		setting.ReconnectBackoffMs = configuration.GetValue<int?>("ReconnectBackoffMs");
		setting.SocketConnectionSetupTimeoutMs = configuration.GetValue<int?>("SocketConnectionSetupTimeoutMs");
		setting.SocketKeepaliveEnable = configuration.GetValue<bool?>("SocketKeepaliveEnable");
		setting.SocketMaxFails = configuration.GetValue<int?>("SocketMaxFails");
		setting.SocketNagleDisable = configuration.GetValue<bool?>("SocketNagleDisable");
		setting.SocketReceiveBufferBytes = configuration.GetValue<int?>("SocketReceiveBufferBytes");
		setting.SocketSendBufferBytes = configuration.GetValue<int?>("SocketSendBufferBytes");
		setting.SocketTimeoutMs = configuration.GetValue<int?>("SocketTimeoutMs");
		setting.Topic = configuration.GetValue<string>("Topic");
		setting.AllowAutoCreateTopics = configuration.GetValue<bool?>("AllowAutoCreateTopics");
		setting.ApiVersionFallbackMs = configuration.GetValue<int?>("ApiVersionFallbackMs");
		setting.ApiVersionRequestTimeoutMs = configuration.GetValue<int?>("ApiVersionRequestTimeoutMs");
		setting.ApiVersionRequest = configuration.GetValue<bool?>("ApiVersionRequest");
		if (string.IsNullOrWhiteSpace(setting.BootstrapServers))
		{
			throw new ArgumentNullException("Kafka bootstrapservers must not null or empty!");
		}
		if (string.IsNullOrWhiteSpace(setting.Topic))
		{
			throw new ArgumentNullException("Topic must not null or empty!");
		}
		setting.BootstrapServers = Regex.Replace(setting.BootstrapServers, "\\s+", string.Empty);
	}

	internal static void SetPropertyValuesForClientConfig(ClientConfig config, BaseSetting setting)
	{
		config.BootstrapServers = setting.BootstrapServers;
		config.ConnectionsMaxIdleMs = setting.ConnectionsMaxIdleMs;
		config.Debug = setting.Debug;
		config.MaxInFlight = setting.MaxInFlight;
		config.MessageCopyMaxBytes = setting.MessageCopyMaxBytes;
		config.MessageMaxBytes = setting.MessageMaxBytes;
		config.MetadataMaxAgeMs = setting.MetadataMaxAgeMs;
		config.ReceiveMessageMaxBytes = setting.ReceiveMessageMaxBytes;
		config.ReconnectBackoffMaxMs = setting.ReconnectBackoffMaxMs;
		config.ReconnectBackoffMs = setting.ReconnectBackoffMs;
		config.SocketConnectionSetupTimeoutMs = setting.SocketConnectionSetupTimeoutMs;
		config.SocketKeepaliveEnable = setting.SocketKeepaliveEnable;
		config.SocketMaxFails = setting.SocketMaxFails;
		config.SocketNagleDisable = setting.SocketNagleDisable;
		config.SocketReceiveBufferBytes = setting.SocketReceiveBufferBytes;
		config.SocketSendBufferBytes = setting.SocketSendBufferBytes;
		config.SocketTimeoutMs = setting.SocketTimeoutMs;
		config.AllowAutoCreateTopics = setting.AllowAutoCreateTopics;
		config.ApiVersionFallbackMs = setting.ApiVersionFallbackMs;
		config.ApiVersionRequestTimeoutMs = setting.ApiVersionRequestTimeoutMs;
		config.ApiVersionRequest = setting.ApiVersionRequest;
	}

	internal static void CopyBaseSetting(BaseSetting target, BaseSetting source)
	{
		target.Id = source.Id;
		target.Topic = source.Topic;
		target.BootstrapServers = source.BootstrapServers;
		target.MessageMaxBytes = source.MessageMaxBytes;
		target.ReceiveMessageMaxBytes = source.ReceiveMessageMaxBytes;
		target.MessageCopyMaxBytes = source.MessageCopyMaxBytes;
		target.MaxInFlight = source.MaxInFlight;
		target.MetadataMaxAgeMs = source.MetadataMaxAgeMs;
		target.ConnectionsMaxIdleMs = source.ConnectionsMaxIdleMs;
		target.ReconnectBackoffMaxMs = source.ReconnectBackoffMaxMs;
		target.ReconnectBackoffMs = source.ReconnectBackoffMs;
		target.Debug = source.Debug;
		target.SocketConnectionSetupTimeoutMs = source.SocketConnectionSetupTimeoutMs;
		target.SocketKeepaliveEnable = source.SocketKeepaliveEnable;
		target.SocketMaxFails = source.SocketMaxFails;
		target.SocketNagleDisable = source.SocketNagleDisable;
		target.SocketReceiveBufferBytes = source.SocketReceiveBufferBytes;
		target.SocketSendBufferBytes = source.SocketSendBufferBytes;
		target.SocketTimeoutMs = source.SocketTimeoutMs;
		target.AllowAutoCreateTopics = source.AllowAutoCreateTopics;
		target.ApiVersionFallbackMs = source.ApiVersionFallbackMs;
		target.ApiVersionRequestTimeoutMs = source.ApiVersionRequestTimeoutMs;
		target.ApiVersionRequest = source.ApiVersionRequest;
	}

	internal static void SetSettingHandler(string id, object handler)
	{
		if (!string.IsNullOrWhiteSpace(id) && handler != null)
		{
			InternalStore.AddOrUpdate(id, handler);
		}
	}

	internal static IServiceCollection AddKafkaConsumers(IServiceCollection services)
	{
		services.AddSingleton(KafkaConsumerManager);
		return services;
	}

	internal static IServiceCollection AddKafkaConsumers(IServiceCollection services, Action<IKafkaConsumerManager> consumerManagerBuilder)
	{
		AddKafkaConsumers(services);
		consumerManagerBuilder(KafkaConsumerManager);
		IEnumerable<Type> enumerable = from kpv in InternalStore.GetStore()
			where kpv.Key.StartsWith("kcmtask")
			select (Type)kpv.Value;
		foreach (Type item in enumerable)
		{
			services.AddSingleton(item);
		}
		return services;
	}

	internal static IServiceCollection AddKafkaProducers(IServiceCollection services)
	{
		services.AddSingleton(KafkaProducerManager);
		return services;
	}

	internal static IServiceCollection AddKafkaProducers(IServiceCollection services, Action<IKafkaProducerManager> producerManagerBuilder)
	{
		AddKafkaProducers(services);
		producerManagerBuilder(KafkaProducerManager);
		return services;
	}

	internal static IHost UseKafkaMessageBus(IHost host)
	{
		IServiceProvider services = host.Services;
		services.GetService<IKafkaConsumerManager>()?.SetDependencies(services);
		return host;
	}

	internal static IHost UseKafkaMessageBus(IHost host, Action<IKafkaConsumerManager> action)
	{
		UseKafkaMessageBus(host);
		IKafkaConsumerManager service = host.Services.GetService<IKafkaConsumerManager>();
		if (service != null)
		{
			action(service);
		}
		return host;
	}

	internal static IHost UseKafkaMessageBus(IHost host, Action<IKafkaConsumerManager, IServiceProvider> action)
	{
		UseKafkaMessageBus(host);
		IKafkaConsumerManager service = host.Services.GetService<IKafkaConsumerManager>();
		if (service != null)
		{
			action(service, host.Services);
		}
		return host;
	}

	internal static void CommitOffsets(string topic, ConsumerConfig consumerConfig, Dictionary<int, long> partitionOffsets)
	{
		IEnumerable<TopicPartitionOffset> enumerable = ((IEnumerable<KeyValuePair<int, long>>)partitionOffsets).Select((Func<KeyValuePair<int, long>, TopicPartitionOffset>)((KeyValuePair<int, long> po) => new TopicPartitionOffset(topic, po.Key, po.Value + 1)));
		int num = 0;
		bool flag = false;
		while (!flag && num < 10)
		{
			try
			{
				IConsumer<Ignore, Ignore> val = new ConsumerBuilder<Ignore, Ignore>((IEnumerable<KeyValuePair<string, string>>)consumerConfig).Build();
				try
				{
					val.Commit(enumerable);
					flag = true;
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			catch (KafkaException val2)
			{
				KafkaException val3 = val2;
				if ((int)val3.Error.Code == 25)
				{
					IAdminClient val4 = new AdminClientBuilder((IEnumerable<KeyValuePair<string, string>>)new AdminClientConfig
					{
						BootstrapServers = ((ClientConfig)consumerConfig).BootstrapServers
					}).Build();
					try
					{
						DescribeConsumerGroupsResult result = val4.DescribeConsumerGroupsAsync((IEnumerable<string>)new string[1] { consumerConfig.GroupId }, (DescribeConsumerGroupsOptions)null).GetAwaiter().GetResult();
						if (result.ConsumerGroupDescriptions.Count > 0)
						{
							ConsumerGroupDescription val5 = result.ConsumerGroupDescriptions[0];
							if ((int)val5.State == 3 || (int)val5.State == 1 || (int)val5.State == 2)
							{
								Thread.Sleep(1000);
								continue;
							}
							if (++num > 10)
							{
								throw;
							}
							Thread.Sleep(1000);
						}
						else
						{
							if (++num > 10)
							{
								throw;
							}
							Thread.Sleep(1000);
						}
					}
					catch (Exception)
					{
						if (++num <= 10)
						{
							Thread.Sleep(1000);
							continue;
						}
						throw;
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				else
				{
					if (++num > 10)
					{
						throw;
					}
					Thread.Sleep(1000);
				}
			}
		}
	}
}

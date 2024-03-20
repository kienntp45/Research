using System;
using Lib.MessageBus.Kafka.Settings;
using Microsoft.Extensions.Configuration;

namespace Lib.MessageBus.Kafka.Abstractions;

public interface IKafkaConsumerManager
{
	internal void SetDependencies(IServiceProvider services);

	IKafkaConsumer<TKey, TValue> GetConsumer<TKey, TValue>(string id);

	IKafkaConsumer<TKey, TValue> GetConsumer<TKey, TValue>(ConsumerSetting consumerSetting);

	IKafkaConsumer<TKey, TValue> GetConsumer<TKey, TValue>(Action<ConsumerSetting> consumerSettingBuilder);

	IKafkaConsumer<TKey, TValue> GetConsumer<TKey, TValue>(IConfiguration configuration);

	void PauseConsumer(string id);

	void StopConsumer(string id);

	void RunConsumer(ConsumerSetting setting, IConsumingTask task);

	void RunConsumer(Action<ConsumerSetting> consumerSettingBuilder, IConsumingTask task);

	void RunConsumer(IConfiguration configuration, IConsumingTask task);

	void RunConsumer(ConsumerSetting setting);

	void RunConsumer(Action<ConsumerSetting> consumerSettingBuilder);

	void RunConsumer(IConfiguration configuration);

	void RunConsumer(string id, IConsumingTask task);

	void RunConsumer(string id);

	void AddConsumer(ConsumerSetting consumerSetting);

	void AddConsumer(Action<ConsumerSetting> consumerSettingBuilder);

	void AddConsumer(IConfiguration configuration);

	void AddConsumer<TTask>(ConsumerSetting consumerSetting) where TTask : IConsumingTask;

	void AddConsumer<TTask>(Action<ConsumerSetting> consumerSettingBuilder) where TTask : IConsumingTask;

	void AddConsumer<TTask>(IConfiguration configuration) where TTask : IConsumingTask;

	void AddConsumer<TTask>(string id) where TTask : IConsumingTask;

	void AddConsumer(ConsumerSetting consumerSetting, Type taskType);

	void AddConsumer(Action<ConsumerSetting> consumerSettingBuilder, Type taskType);

	void AddConsumer(IConfiguration configuration, Type taskType);

	void AddConsumer(string id, Type taskType);

	void RemoveConsumer(string id);
}

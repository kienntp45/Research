using System;
using System.Threading;
using Lib.MessageBus.Kafka.Settings;
using Microsoft.Extensions.Configuration;

namespace Lib.MessageBus.Kafka.Abstractions;

public interface IKafkaProducerManager
{
	void AddProducer(ProducerSetting setting);

	void AddProducer(Action<ProducerSetting> producerSettingBuilder);

	void AddProducer(IConfiguration configuration);

	void RemoveProducer(string id, int flushTimeoutMilliseconds = 0);

	void RemoveProducer(string id, CancellationToken flushCancellationToken);

	IKafkaProducer<TKey, TValue> GetProducer<TKey, TValue>(string id);

	IKafkaProducer<TKey, TValue> GetProducer<TKey, TValue>(ProducerSetting setting);

	IKafkaProducer<TKey, TValue> GetProducer<TKey, TValue>(Action<ProducerSetting> producerSettingBuilder);

	IKafkaProducer<TKey, TValue> GetProducer<TKey, TValue>(IConfiguration configuration);
}

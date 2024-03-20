using Lib.MessageBus.Kafka.Abstractions;
using Lib.MessageBus.Kafka.Settings;
using System.Collections.Concurrent;
using System.Reflection;

namespace Lib.MessageBus.Kafka.Internal;

internal class KafkaProducerManager : IKafkaProducerManager
{
	public IKafkaProducer<TKey, TValue> GetProducer<TKey, TValue>(string id)
	{
		if (string.IsNullOrWhiteSpace(id) || !Utils.InternalStore.Read("kpmsetting" + id, out var value))
		{
			return null;
		}
		return GetProducer<TKey, TValue>((ProducerSetting)value);
	}

	public IKafkaProducer<TKey, TValue> GetProducer<TKey, TValue>(ProducerSetting setting)
	{
		if (setting == null || string.IsNullOrWhiteSpace(setting.Id))
		{
			return null;
		}
		object value = null;
		if (Utils.InternalStore.Read("kpmsetting" + setting.Id, out var value2) && !Utils.InternalStore.Read("kpmproducer" + setting.Id, out value))
		{
			IKafkaProducer<TKey, TValue> kafkaProducer = new KafkaProducer<TKey, TValue>((ProducerSetting)value2);
			Utils.InternalStore.Add("kpmproducer" + setting.Id, kafkaProducer);
			return kafkaProducer;
		}
		if (value == null || ((IKafkaProducer<TKey, TValue>)value).IsDisposed)
		{
			setting.PollTimeoutSecond = ((setting.PollTimeoutSecond > 0) ? setting.PollTimeoutSecond : 3);
			return new KafkaProducer<TKey, TValue>(setting);
		}
		return (IKafkaProducer<TKey, TValue>)value;
	}

	public IKafkaProducer<TKey, TValue> GetProducer<TKey, TValue>(IConfiguration configuration)
	{
		return GetProducer<TKey, TValue>(ProducerSetting.MapValue(configuration));
	}

	public IKafkaProducer<TKey, TValue> GetProducer<TKey, TValue>(Action<ProducerSetting> producerSettingBuilder)
	{
		ProducerSetting producerSetting = new ProducerSetting();
		producerSettingBuilder(producerSetting);
		return GetProducer<TKey, TValue>(producerSetting);
	}

	public void AddProducer(ProducerSetting setting)
	{
		if (!Utils.InternalStore.GetStore().ContainsKey("kpmsetting" + setting.Id))
		{
			setting.PollTimeoutSecond = ((setting.PollTimeoutSecond > 0) ? setting.PollTimeoutSecond : 3);
			Utils.InternalStore.Add("kpmsetting" + setting.Id, setting);
		}
	}

	public void AddProducer(Action<ProducerSetting> producerSettingBuilder)
	{
		ProducerSetting producerSetting = new ProducerSetting();
		producerSettingBuilder(producerSetting);
		AddProducer(producerSetting);
	}

	public void AddProducer(IConfiguration configuration)
	{
		AddProducer(ProducerSetting.MapValue(configuration));
	}

	public void RemoveProducer(string id, int flushTimeoutMilliseconds = 0)
	{
		Utils.InternalStore.ChangeWithCustom(delegate(ConcurrentDictionary<string, object> store)
		{
			store.TryRemove("kpmsetting" + id, out var _);
			if (store.TryRemove("kpmproducer" + id, out var value2))
			{
				if (flushTimeoutMilliseconds > 0)
				{
					MethodInfo method = value2.GetType().GetMethod("Flush", new Type[1] { typeof(TimeSpan) });
					method.Invoke(value2, new object[1] { TimeSpan.FromMilliseconds(flushTimeoutMilliseconds) });
				}
				IKafkaProducer kafkaProducer = (IKafkaProducer)value2;
				kafkaProducer.Dispose();
			}
		});
	}

	public void RemoveProducer(string id, CancellationToken flushCancellationToken)
	{
		Utils.InternalStore.ChangeWithCustom(delegate(ConcurrentDictionary<string, object> store)
		{
			store.TryRemove("kpmsetting" + id, out var _);
			if (store.TryRemove("kpmproducer" + id, out var value2))
			{
				MethodInfo method = value2.GetType().GetMethod("Flush", new Type[1] { typeof(CancellationToken) });
				method.Invoke(value2, new object[1] { flushCancellationToken });
				IKafkaProducer kafkaProducer = (IKafkaProducer)value2;
				kafkaProducer.Dispose();
			}
		});
	}
}

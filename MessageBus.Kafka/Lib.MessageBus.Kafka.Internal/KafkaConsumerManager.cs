using Lib.MessageBus.Kafka.Abstractions;
using Lib.MessageBus.Kafka.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;

namespace Lib.MessageBus.Kafka.Internal;

internal class KafkaConsumerManager : IKafkaConsumerManager
{
	private IServiceProvider _serviceProvider = null;

	public IKafkaConsumer<TKey, TValue> GetConsumer<TKey, TValue>(string id)
	{
		if (string.IsNullOrWhiteSpace(id) || !Utils.InternalStore.Read("kcmsetting" + id, out var value))
		{
			return null;
		}
		return GetConsumer<TKey, TValue>((ConsumerSetting)value);
	}

	public IKafkaConsumer<TKey, TValue> GetConsumer<TKey, TValue>(ConsumerSetting setting)
	{
		if (setting == null || string.IsNullOrWhiteSpace(setting.Id))
		{
			return null;
		}
		if (!Utils.InternalStore.Read("kcmconsumer" + setting.Id, out var value) || ((IKafkaConsumer<TKey, TValue>)value).IsDisposed)
		{
			value = CreateConsumerObject(setting, typeof(KafkaConsumer<TKey, TValue>));
		}
		return (IKafkaConsumer<TKey, TValue>)value;
	}

	public IKafkaConsumer<TKey, TValue> GetConsumer<TKey, TValue>(Action<ConsumerSetting> consumerSettingBuilder)
	{
		ConsumerSetting consumerSetting = new ConsumerSetting();
		consumerSettingBuilder(consumerSetting);
		return GetConsumer<TKey, TValue>(consumerSetting);
	}

	public IKafkaConsumer<TKey, TValue> GetConsumer<TKey, TValue>(IConfiguration configuration)
	{
		return GetConsumer<TKey, TValue>(ConsumerSetting.MapValue(configuration));
	}

	public void StopConsumer(string id)
	{
		Utils.InternalStore.ChangeWithCustom(delegate(ConcurrentDictionary<string, object> store)
		{
			RemoveAndDisposeConsumer(store, id);
		});
	}

	private void RemoveAndDisposeConsumer(ConcurrentDictionary<string, object> store, string id)
	{
		if (store.TryRemove("kcmconsumer" + id, out var value))
		{
			IKafkaConsumer kafkaConsumer = (IKafkaConsumer)value;
			kafkaConsumer.Dispose();
		}
	}

	public void PauseConsumer(string id)
	{
		Utils.InternalStore.ChangeWithCustom(delegate(ConcurrentDictionary<string, object> store)
		{
			if (store.TryGetValue("kcmconsumer" + id, out var value))
			{
				IKafkaConsumer kafkaConsumer = (IKafkaConsumer)value;
				kafkaConsumer.Pause();
			}
		});
	}

	public void AddConsumer(ConsumerSetting consumerSetting)
	{
		if (consumerSetting != null && !string.IsNullOrWhiteSpace(consumerSetting.Id))
		{
			string id = consumerSetting.Id;
			AddConsumerSetting(id, consumerSetting);
		}
	}

	public void AddConsumer(Action<ConsumerSetting> consumerSettingBuilder)
	{
		ConsumerSetting consumerSetting = new ConsumerSetting();
		consumerSettingBuilder(consumerSetting);
		AddConsumer(consumerSetting);
	}

	public void AddConsumer(IConfiguration configuration)
	{
		AddConsumer(ConsumerSetting.MapValue(configuration));
	}

	public void AddConsumer(ConsumerSetting consumerSetting, Type taskType)
	{
		if (consumerSetting != null && !string.IsNullOrWhiteSpace(consumerSetting.Id) && taskType.IsClass && taskType.GetInterfaces().Contains(typeof(IConsumingTask)))
		{
			string id = consumerSetting.Id;
			AddTaskType(id, taskType);
			AddConsumerSetting(id, consumerSetting);
		}
	}

	public void AddConsumer(Action<ConsumerSetting> consumerSettingBuilder, Type taskType)
	{
		ConsumerSetting consumerSetting = new ConsumerSetting();
		consumerSettingBuilder(consumerSetting);
		AddConsumer(consumerSetting, taskType);
	}

	public void AddConsumer(IConfiguration configuration, Type taskType)
	{
		AddConsumer(ConsumerSetting.MapValue(configuration), taskType);
	}

	public void AddConsumer(string id, Type taskType)
	{
		if (!string.IsNullOrWhiteSpace(id) && taskType.IsClass && taskType.GetInterfaces().Contains(typeof(IConsumingTask)))
		{
			AddTaskType(id, taskType);
		}
	}

	public void AddConsumer<TTask>(ConsumerSetting consumerSetting) where TTask : IConsumingTask
	{
		Type typeFromHandle = typeof(TTask);
		AddConsumer(consumerSetting, typeFromHandle);
	}

	public void AddConsumer<TTask>(Action<ConsumerSetting> consumerSettingBuilder) where TTask : IConsumingTask
	{
		Type typeFromHandle = typeof(TTask);
		AddConsumer(consumerSettingBuilder, typeFromHandle);
	}

	public void AddConsumer<TTask>(IConfiguration configuration) where TTask : IConsumingTask
	{
		Type typeFromHandle = typeof(TTask);
		AddConsumer(configuration, typeFromHandle);
	}

	public void AddConsumer<TTask>(string id) where TTask : IConsumingTask
	{
		Type typeFromHandle = typeof(TTask);
		AddConsumer(id, typeFromHandle);
	}

	public void RemoveConsumer(string id)
	{
		Utils.InternalStore.ChangeWithCustom(delegate(ConcurrentDictionary<string, object> store)
		{
			store.TryRemove("kcmtask" + id, out var value);
			store.TryRemove("kcmsetting" + id, out value);
			RemoveAndDisposeConsumer(store, id);
		});
	}

	public void RunConsumer(string id, IConsumingTask consumingTask)
	{
		if (!string.IsNullOrWhiteSpace(id) && Utils.InternalStore.Read("kcmsetting" + id, out var value))
		{
			ConsumerSetting setting = (ConsumerSetting)value;
			RunConsumer(setting, consumingTask);
		}
	}

	public void RunConsumer(string id)
	{
		if (!string.IsNullOrWhiteSpace(id) && Utils.InternalStore.Read("kcmtask" + id, out var value) && _serviceProvider != null)
		{
			Type serviceType = (Type)value;
			IConsumingTask consumingTask = (IConsumingTask)_serviceProvider.GetRequiredService(serviceType);
			RunConsumer(id, consumingTask);
		}
	}

	public void RunConsumer(ConsumerSetting setting, IConsumingTask consumingTask)
	{
		Type taskInterfaceType = consumingTask.GetType().GetInterfaces().FirstOrDefault((Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IConsumingTask<, >));
		Type[] genericArgs = ((taskInterfaceType != null) ? taskInterfaceType.GetGenericArguments() : new Type[0]);
		if (setting == null || string.IsNullOrWhiteSpace(setting.Id) || genericArgs.Length != 2)
		{
			return;
		}
		Utils.InternalStore.ChangeWithCustom(delegate(ConcurrentDictionary<string, object> store)
		{
			Type type2 = genericArgs[0];
			Type type3 = genericArgs[1];
			Type type4 = typeof(KafkaConsumer<, >).MakeGenericType(type2, type3);
			Type type5 = typeof(ConsumeResult<, >).MakeGenericType(type2, type3);
			Type delegateType = typeof(Action<>).MakeGenericType(type5);
			MethodInfo method = taskInterfaceType.GetMethod("Execute");
			MethodInfo consumeMethodInfo = type4.GetMethod("Consume");
			if (!store.TryGetValue("kcmconsumer" + setting.Id, out var kafkaConsumer))
			{
				kafkaConsumer = CreateConsumerObject(setting, type4);
			}
			else if ((bool)type4.GetProperty("IsDisposed").GetValue(kafkaConsumer))
			{
				kafkaConsumer = CreateConsumerObject(setting, type4);
			}
			else if ((bool)type4.GetProperty("IsRunning").GetValue(kafkaConsumer))
			{
				return;
			}
			object[] consumeMethodParams = new object[1] { method.CreateDelegate(delegateType, consumingTask) };
			Task.Factory.StartNew(() => consumeMethodInfo.Invoke(kafkaConsumer, consumeMethodParams), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
			while (!(bool)type4.GetProperty("IsRunning").GetValue(kafkaConsumer))
			{
			}
			AddConsumerSetting("kcmsetting" + setting.Id, setting);
			AddTaskType("kcmtask" + setting.Id, consumingTask.GetType());
			store.AddOrUpdate("kcmconsumer" + setting.Id, kafkaConsumer, (string key, object value) => kafkaConsumer);
		});
	}

	public void RunConsumer(ConsumerSetting setting)
	{
		if (setting != null && !string.IsNullOrWhiteSpace(setting.Id) && Utils.InternalStore.Read("kcmtask" + setting.Id, out var value) && _serviceProvider != null)
		{
			Type serviceType = (Type)value;
			IConsumingTask consumingTask = (IConsumingTask)_serviceProvider.GetRequiredService(serviceType);
			RunConsumer(setting, consumingTask);
		}
	}

	public void RunConsumer(Action<ConsumerSetting> consumerSettingBuilder)
	{
		ConsumerSetting consumerSetting = new ConsumerSetting();
		consumerSettingBuilder(consumerSetting);
		RunConsumer(consumerSetting);
	}

	public void RunConsumer(IConfiguration configuration, IConsumingTask task)
	{
		RunConsumer(ConsumerSetting.MapValue(configuration), task);
	}

	public void RunConsumer(IConfiguration configuration)
	{
		RunConsumer(ConsumerSetting.MapValue(configuration));
	}

	public void RunConsumer(Action<ConsumerSetting> consumerSettingBuilder, IConsumingTask task)
	{
		ConsumerSetting consumerSetting = new ConsumerSetting();
		consumerSettingBuilder(consumerSetting);
		RunConsumer(consumerSetting, task);
	}

	private void AddConsumerSetting(string id, ConsumerSetting setting)
	{
		if (!string.IsNullOrWhiteSpace(id))
		{
			setting.ConsumeTimeoutSecond = ((setting.ConsumeTimeoutSecond > 0) ? setting.ConsumeTimeoutSecond : 3);
			Utils.InternalStore.Add("kcmsetting" + id, setting);
		}
	}

	private void AddTaskType(string id, Type taskType)
	{
		if (!string.IsNullOrWhiteSpace(id))
		{
			Utils.InternalStore.Add("kcmtask" + id, taskType);
		}
	}

	private object CreateConsumerObject(ConsumerSetting setting, Type consumerType)
	{
		setting.ConsumeTimeoutSecond = ((setting.ConsumeTimeoutSecond > 0) ? setting.ConsumeTimeoutSecond : 3);
		ILogger logger = _serviceProvider?.GetService<ILoggerFactory>()?.CreateLogger(consumerType);
		return Activator.CreateInstance(consumerType, setting, logger);
	}

	void IKafkaConsumerManager.SetDependencies(IServiceProvider services)
	{
		if (_serviceProvider == null)
		{
			_serviceProvider = services;
		}
	}
}

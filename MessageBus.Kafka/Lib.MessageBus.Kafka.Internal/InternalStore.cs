using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Lib.MessageBus.Kafka.Internal;

internal class InternalStore
{
	private readonly ConcurrentDictionary<string, object> store;

	private readonly Channel<KeyValuePair<StoreCommandType, object>> storeQueue;

	public InternalStore(int capacity)
	{
		store = new ConcurrentDictionary<string, object>();
		BoundedChannelOptions options = new BoundedChannelOptions(capacity)
		{
			FullMode = BoundedChannelFullMode.Wait
		};
		storeQueue = Channel.CreateBounded<KeyValuePair<StoreCommandType, object>>(options);
		Task.Factory.StartNew((Func<Task>)PullLoop, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
	}

	public bool Read(string key, out object value)
	{
		MakeEmptyQueue();
		return store.TryGetValue(key, out value);
	}

	public void Add(string key, object value)
	{
		KeyValuePair<StoreCommandType, object> item = new KeyValuePair<StoreCommandType, object>(StoreCommandType.Add, new KeyValuePair<string, object>(key, value));
		storeQueue.Writer.WriteAsync(item);
	}

	public void AddOrUpdate(string key, object value)
	{
		KeyValuePair<StoreCommandType, object> item = new KeyValuePair<StoreCommandType, object>(StoreCommandType.AddOrUpdate, new KeyValuePair<string, object>(key, value));
		storeQueue.Writer.WriteAsync(item);
	}

	public void Remove(string key)
	{
		KeyValuePair<StoreCommandType, object> item = new KeyValuePair<StoreCommandType, object>(StoreCommandType.Remove, key);
		storeQueue.Writer.WriteAsync(item);
	}

	public void ChangeWithCustom(Action<ConcurrentDictionary<string, object>> changeHandler)
	{
		KeyValuePair<StoreCommandType, object> item = new KeyValuePair<StoreCommandType, object>(StoreCommandType.ChangeWithCustom, changeHandler);
		storeQueue.Writer.WriteAsync(item);
	}

	public ConcurrentDictionary<string, object> GetStore()
	{
		MakeEmptyQueue();
		return store;
	}

	private void MakeEmptyQueue()
	{
		while (storeQueue.Reader.CanCount && storeQueue.Reader.Count > 0)
		{
			HandleData(storeQueue.Reader.ReadAsync().GetAwaiter().GetResult());
		}
	}

	private void HandleData(KeyValuePair<StoreCommandType, object> data)
	{
		switch (data.Key)
		{
		case StoreCommandType.Add:
		{
			KeyValuePair<string, object> keyValuePair = (KeyValuePair<string, object>)data.Value;
			store.TryAdd(keyValuePair.Key, keyValuePair.Value);
			break;
		}
		case StoreCommandType.AddOrUpdate:
		{
			KeyValuePair<string, object> addOrUpdateValue = (KeyValuePair<string, object>)data.Value;
			store.AddOrUpdate(addOrUpdateValue.Key, addOrUpdateValue.Value, (string key, object value) => addOrUpdateValue.Value);
			break;
		}
		case StoreCommandType.Remove:
		{
			string key2 = (string)data.Value;
			store.TryRemove(key2, out var _);
			break;
		}
		case StoreCommandType.ChangeWithCustom:
		{
			Action<ConcurrentDictionary<string, object>> action = (Action<ConcurrentDictionary<string, object>>)data.Value;
			action(store);
			break;
		}
		}
	}

	private async Task PullLoop()
	{
		while (await storeQueue.Reader.WaitToReadAsync())
		{
			HandleData(await storeQueue.Reader.ReadAsync());
		}
	}
}

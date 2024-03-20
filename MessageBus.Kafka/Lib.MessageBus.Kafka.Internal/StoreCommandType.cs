namespace Lib.MessageBus.Kafka.Internal;

internal enum StoreCommandType
{
	None,
	Add,
	AddOrUpdate,
	Remove,
	ChangeWithCustom
}

using Confluent.Kafka;

namespace LibraryKafka.Library.Kafka.Interface;

public interface  IConsumeTask<TKey, TValue>
{
    void Execute(ConsumerBuilder<TKey, TValue> build);
}

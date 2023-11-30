using Confluent.Kafka;

namespace LibraryKafka.Library.Kafka.Settings;

public class AppSetting
{
    public string BootstrapServers { get; init; }
    public ProducerConfig ProducerSetting { get; init; }
    public ConsumerConfig[] ConsumerSettings { get; init; }
}

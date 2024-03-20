using Confluent.Kafka;
using Manonero.MessageBus.Kafka.Abstractions;

namespace TestConsume
{
    public static class HostExtension
    {
        public static IHost RunConsumer(this IHost host)
        {
            var service = host.Services;
            RunAccountConsumer(service);
            return host;
        }

        private static void RunAccountConsumer(IServiceProvider services)
        {
            var appSetting = services.GetRequiredService<AppSetting>();
            var consumerSetting = appSetting.GetConsumeConfig("Account");
            var consumerManager = services.GetRequiredService<IKafkaConsumerManager>();
            var allPartitions = GetPartitionsOfTopic(consumerSetting.BootstrapServers, consumerSetting.Topic);
            //var partitionOffset = GetPartitionOffset<Cash>(allPartitions, dbContextFactory, CashEntityTypeConfiguration.TableName, defaultSchema,
            //    CashEntityTypeConfiguration.ClientCodeColumeName);
            consumerSetting.SetPartitionOffset(GetPartitionOffset());
            consumerManager.RunConsumer<string, string>(consumerSetting);
        }

        public static int[] GetPartitionsOfTopic(string bootstrapServers, string topic)
        {
            var adminConfig = new AdminClientConfig
            {
                BootstrapServers = bootstrapServers
            };
            using (var adminClient = new AdminClientBuilder(adminConfig).Build())
            {
                var metadata = adminClient.GetMetadata(topic, TimeSpan.FromSeconds(5));
                var topicMetadata = metadata.Topics.Single();
                return topicMetadata.Partitions.Select(partitionMetaData => partitionMetaData.PartitionId).ToArray();
            }
        }

        public static Dictionary<int, long?> GetPartitionOffset()
        {
            return new Dictionary<int, long?>() { { 0, 0L } };
        }
    }
}

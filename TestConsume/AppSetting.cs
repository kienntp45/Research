using Manonero.MessageBus.Kafka.Settings;

namespace TestConsume
{
    public class AppSetting
    {
        public ConsumerSetting[] ConsumerSettings { get; init; }
        public static AppSetting MapValue(IConfiguration configuration)
        {
            var consumerConfigurations = configuration.GetSection(nameof(ConsumerSetting)).GetChildren();

            var consumerSettings = new List<ConsumerSetting>();

            foreach (var consumerConfiguration in consumerConfigurations)
            {
                var consume = ConsumerSetting.MapValue(consumerConfiguration, "localhost");
                if (!consumerSettings.Contains(consume))
                    consumerSettings.Add(consume);
            }

            return new AppSetting()
            {
                ConsumerSettings = consumerSettings.ToArray(),
            };
        }

        public ConsumerSetting GetConsumeConfig(string id) => ConsumerSettings.FirstOrDefault(e => e.Id.Equals(id));
    }
}

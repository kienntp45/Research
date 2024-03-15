namespace Post.Cmd.Api.Producers
{
    public class EventProducer : IEventProducer
    {
        private readonly IProducer<string, string> _producer;

        public EventProducer(ProducerConfig config)
        {
            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task Producer<T>(string topic, T @event) where T : BaseEvent
        {
            var msg = new Message<string, string>() { Key = Guid.NewGuid().ToString(), Value = JsonSerializer.Serialize(@event) };
            var deliveryResult = await _producer.ProduceAsync(topic, msg);
            if (deliveryResult.Status == PersistenceStatus.NotPersisted)
                throw new Exception($"Could not produce @{@event.GetType().Name} message to topic due to the following reason");
        }
    }
}

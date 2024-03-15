namespace CQRS.Core.Producers
{
    public interface IEventProducer
    {
        Task Producer<T>(string topic, T @event) where T : BaseEvent;
    }
}

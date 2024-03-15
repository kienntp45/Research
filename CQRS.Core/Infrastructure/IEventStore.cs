namespace CQRS.Core.Infrastructure
{
    public interface IEventStore
    {
        Task SaveEventStoreAsync(Guid aggregateId, IEnumerable<BaseEvent> events, int expectedVersion);
        Task<List<BaseEvent>> GetEventAsync(Guid aggregateId);
    }
}

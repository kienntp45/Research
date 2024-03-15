using CQRS.Core.Handlers;
using Post.Cmd.Domain.Aggregates;

namespace Post.Cmd.Infrastructure.Handlers
{
    public class EventSourcingHandler : IEventSourcingHandler<PostAggregate>
    {
        private readonly IEventStore _eventStore;

        public EventSourcingHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task<PostAggregate> GetByIdAsync(Guid aggregateId)
        {
            var aggregate = new PostAggregate();
            var events = await _eventStore.GetEventAsync(aggregateId);

            if (events == null || !events.Any()) return aggregate;

            aggregate.ReplayEvent(events);
            aggregate.Version = events.Select(e => e.Version).Max();
            return aggregate;
        }

        public async Task SaveAsync(AggregateRoot aggregateRoot)
        {
            await _eventStore.SaveEventStoreAsync(aggregateRoot.Id, aggregateRoot.GetUnCommittedChanges(), aggregateRoot.Version);
            aggregateRoot.MarkChangesAsCommitted();
        }
    }
}

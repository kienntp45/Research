using CQRS.Core.Exceptions;
using CQRS.Core.Producers;
using Post.Cmd.Domain.Aggregates;
using System.Data;

namespace Post.Cmd.Infrastructure.Stores
{
    public class EventStore : IEventStore
    {
        private readonly IEventStoreRepository _eventStore;
        private readonly IEventProducer _producer;

        public EventStore(IEventStoreRepository eventStore, IEventProducer producer)
        {
            _eventStore = eventStore;
            _producer = producer;
        }

        public async Task<List<BaseEvent>> GetEventAsync(Guid aggregateId)
        {
          var eventStrem = await _eventStore.FindByAggregateId(aggregateId);
            if (eventStrem == null || !eventStrem.Any())
                    throw new AggregateNotFoundException("Incorrect post ID provided");

            return eventStrem.OrderBy(e=>e.Version).Select(e=>e.EventData).ToList();
        }

        public async Task SaveEventStoreAsync(Guid aggregateId, IEnumerable<BaseEvent> events, int expectedVersion)
        {
            var eventStrem = await _eventStore.FindByAggregateId(aggregateId);

            if (expectedVersion != 1 && eventStrem[^1].Version != expectedVersion)
                throw new ConcurrencyException();

            var version = expectedVersion;

            foreach (var @event in events)
            {
                version++;
                @event.Version = version;  
                var eventType  = @event.GetType().Name;
                var eventModel = new EventModel()
                {
                    TimeStamp = DateTime.Now,
                    AggregateIdentifier = aggregateId,
                    AggregateType = nameof(PostAggregate),
                    Version = version,
                    EventType = eventType,
                    EventData = @event
                };

                await _eventStore.SaveAsync(eventModel);

                await _producer.Producer("", @event);
            }
        }
    }
}

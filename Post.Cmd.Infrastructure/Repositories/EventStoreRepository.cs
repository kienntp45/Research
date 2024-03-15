namespace Post.Cmd.Infrastructure.Repositories
{
    public class EventStoreRepository : IEventStoreRepository
    {
        private readonly IMongoCollection<EventModel> _events;

        public EventStoreRepository(IOptions<MongoDbConfig> config)
        {
            var eventStoreClient = new MongoClient(config.Value.ConnectionString);
            var mongoDb = eventStoreClient.GetDatabase(config.Value.Database);

            _events = mongoDb.GetCollection<EventModel>(config.Value.Collection);
        }
        public async Task<List<EventModel>> FindByAggregateId(Guid aggregateId)
        {
            return await _events.Find(e => e.AggregateIdentifier == aggregateId).ToListAsync();
        }

        public Task SaveAsync(EventModel @event)
        {
            return _events.InsertOneAsync(@event);
        }
    }
}

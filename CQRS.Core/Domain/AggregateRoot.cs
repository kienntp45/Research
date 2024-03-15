namespace CQRS.Core.Domain
{
    public abstract class AggregateRoot
    {
        public Guid _id;
        private readonly List<BaseEvent> _changes = new();

        public Guid Id { get { return _id; } }

        public int Version { get; set; } = -1;

        public IEnumerable<BaseEvent> GetUnCommittedChanges()
        {
            return _changes;
        }

        public void MarkChangesAsCommitted()
        {
            _changes.Clear();
        }

        public void ApplyChange(BaseEvent @event, bool isNew)
        {
            var method = this.GetType().GetMethod("Apply", new Type[] { @event.GetType() });
            if (method == null) throw new NullReferenceException($"The Apply method was not found in the aggregate for {@event.GetType().Name}!");

            method.Invoke(this, new object[] { @event});

            if(isNew)
            {
                _changes.Add(@event);
            }
        }

        public void RaiseEvent(BaseEvent @event)
        {
            ApplyChange(@event, true);
        } 

        public void ReplayEvent(IEnumerable<BaseEvent> events)
        {
            foreach (var @event in events)
            {
                ApplyChange(@event, false);
            }
        }
    }
}

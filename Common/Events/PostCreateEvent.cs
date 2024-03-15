using CQRS.Core.Events;

namespace Common.Events
{
    public class PostCreateEvent : BaseEvent
    {
        public PostCreateEvent() : base(nameof(PostCreateEvent))
        {
        }

        public string Author { get; set; }
        public string Message { get; set; }
        public DateTime DatePosted { get; set; }
    }
}

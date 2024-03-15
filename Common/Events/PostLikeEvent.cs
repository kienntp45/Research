using CQRS.Core.Events;

namespace Common.Events
{
    public class PostLikeEvent : BaseEvent
    {
        public PostLikeEvent() : base(nameof(PostLikeEvent))
        {
        }
    }
}

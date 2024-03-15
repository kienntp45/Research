using Amazon.SecurityToken.Model.Internal.MarshallTransformations;
using Common.Events;
using CQRS.Core.Domain;
using CQRS.Core.Messages;

namespace Post.Cmd.Domain.Aggregates
{
    public class PostAggregate : AggregateRoot
    {
        private bool _active;
        private string _author;
        private readonly Dictionary<Guid, Tuple<string, string>> _comments = new();

        public bool Active { get { return _active; } set { _active = value; } }

        public PostAggregate()
        {
        }

        public PostAggregate(Guid id, string author, string message)
        {
            RaiseEvent(new PostCreateEvent
            {
                Author = author,
                Message = message,
                Id = id,
                DatePosted = DateTime.UtcNow,
            });
        }

        public void Apply(PostCreateEvent @event)
        {
            _id = @event.Id;
            _author = @event.Author;
            _active = true;
        }

        public void EditMessage(string message)
        {
            if (!_active)
                throw new InvalidOperationException("you cannot edit the message of an inactive post!");

            if (string.IsNullOrWhiteSpace(message))
                throw new InvalidOperationException($"the value of {nameof(message)} cannot be null or empty");

            RaiseEvent(new MessageUpdatedEvent
            {
                Id = _id,
                Message = message
            });
        }

        public void Apply(MessageUpdatedEvent @event)
        {
            _id = @event.Id;
        }

        public void LikePost()
        {
            if (!_active) throw new InvalidOperationException("you cannot likw of an inactive post!");

            RaiseEvent(new PostLikeEvent
            {
                Id = _id,
            });
        }

        public void Apply(PostLikeEvent @event)
        {
            _id = @event.Id;
        }

        public void AddComment(string comment, string username)
        {
            if (!_active)
                throw new InvalidOperationException("you cannot add a comment to an inactive post!");

            if (string.IsNullOrWhiteSpace(comment))
                throw new InvalidOperationException($"the value of {nameof(comment)} cannot be null or empty");

            RaiseEvent(new CommentAddedEvent
            {
                Id = _id,
                CommentId = Guid.NewGuid(),
                Username = username,
                Comment = comment,
                CommentDate = DateTime.Now,
            });
        }

        public void Apply(CommentAddedEvent @event)
        {
            _id = @event.Id;
            _comments.Add(@event.CommentId, new Tuple<string, string>(@event.Comment, @event.Username));
        }

        public void EditCommnet(Guid commentId, string message, string username)
        {
            if (!_active)
                throw new InvalidOperationException("you cannot edit a comment for an inactive post!");

            if (_comments[commentId].Item2.Equals(username))
                throw new InvalidOperationException("You cann't allowed to edit a comment that was made by other user");

            RaiseEvent(new CommentUpdateEvent
            {
                Id = _id,
                CommentId = commentId,
                Comment = message, 
                EditDate = DateTime.Now,
                Username = username,
            });
        }

        public void Apply(CommentUpdateEvent @event)
        {
            _id = @event.Id;
            _comments[@event.CommentId] = new Tuple<string, string>(@event.Comment, @event.Username);
        }

        public void DeleteCommnet(Guid commentId, string username)
        {
            if (!_active)
                throw new InvalidOperationException("you cannot edit a comment for an inactive post!");

            if (_comments[commentId].Item2.Equals(username))
                throw new InvalidOperationException("You cann't allowed to edit a comment that was made by other user");

            RaiseEvent(new CommentRemovedEvent { Id = _id, CommentId = commentId, });
        }

        public void Apply(CommentRemovedEvent @event)
        {
            _id = @event.Id;
            _comments.Remove(@event.CommentId);
        }

        public void DeletePost(string username)
        {
            if (!_active)
            {
                throw new InvalidOperationException("The post has already been removed!");
            }

            if (!_author.Equals(username, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new InvalidOperationException("You are not allowed to delete a post that was made by someone else!");
            }

            RaiseEvent(new PostRemovedEvent
            {
                Id = _id
            });
        }

        public void Apply(PostRemovedEvent @event)
        {
            _id = @event.Id;
            _active = false;
        }
    }
}

namespace Post.Cmd.Api.Commands
{
    public class CommandHandler : ICommandHandler
    {
        private readonly IEventSourcingHandler<PostAggregate> _eventStore;

        public CommandHandler(IEventSourcingHandler<PostAggregate> eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task HandleAsync(NewPostCommand command)
        { 
            var aggregate = new PostAggregate(command.Id, command.Author, command.Message);
            await _eventStore.SaveAsync(aggregate);
        }

        public async Task HandleAsync(EditMessageCommand command)
        {
            var aggragate = await _eventStore.GetByIdAsync(command.Id);
            aggragate.EditMessage(command.Message);

            await _eventStore.SaveAsync(aggragate);
        }

        public async Task HandleAsync(LikePostCommand command)
        {
            var aggregate = await _eventStore.GetByIdAsync(command.Id);
            aggregate.LikePost();

            await _eventStore.SaveAsync(aggregate);
        }

        public async Task HandleAsync(AddCommentCommand command)
        {
            var aggregate = await _eventStore.GetByIdAsync(command.Id);
            aggregate.AddComment(command.Comment, command.Username);

            await _eventStore.SaveAsync(aggregate);
        }

        public async Task HandleAsync(EditCommentCommand command)
        {
            var aggregate = await _eventStore.GetByIdAsync(command.Id);
            aggregate.EditCommnet(command.Id, command.Comment, command.Username);

            await _eventStore.SaveAsync(aggregate);
        }

        public async Task HandleAsync(RemoveCommentCommand command)
        {
            var aggregate = await _eventStore.GetByIdAsync(command.Id); 
            aggregate.DeleteCommnet(command.Id, command.Username);

            await _eventStore.SaveAsync(aggregate);
        }

        public async Task HandleAsync(DeletePostCommand command)
        {
            var aggregate = await _eventStore.GetByIdAsync(command.Id);
            aggregate.DeletePost(command.Username);

            await _eventStore.SaveAsync(aggregate);
        } 
    }
}

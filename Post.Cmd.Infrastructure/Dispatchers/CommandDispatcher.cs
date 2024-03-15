namespace Post.Cmd.Infrastructure.Dispatchers
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly Dictionary<Type, Func<BaseCommand, Task>> _handler = new();
        public void RegisterHandler<T>(Func<T, Task> handler) where T : BaseCommand
        {
            if (_handler.ContainsKey(typeof(T)))
                throw new IndexOutOfRangeException("you can not register same command handler twice");
            _handler.Add(typeof(T), e => handler((T)e));
        }

        public async void SendAsync(BaseCommand command)
        {
            if (_handler.TryGetValue(command.GetType(), out Func<BaseCommand, Task>? handler))
            {
                await handler(command);
            }
            else
            {
                throw new ArgumentNullException(nameof(handler), "No command handler was register");
            }
        }
    }
}

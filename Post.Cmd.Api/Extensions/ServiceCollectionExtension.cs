using Post.Cmd.Api.Producers;

namespace Post.Cmd.Api.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddConfig(configuration);
        services.AddScope();
        services.AddDispacherHandle();
        services.AddProducerConfig(configuration);
        return services;
    }

    public static IServiceCollection AddScope(this IServiceCollection services)
    {
        services.AddScoped<IEventStoreRepository, EventStoreRepository>();
        services.AddScoped<IEventStore, EventStore>();
        services.AddScoped<IEventSourcingHandler<PostAggregate>, EventSourcingHandler>();
        services.AddScoped<ICommandHandler, CommandHandler>();
        services.AddScoped<IEventProducer, EventProducer>();
        return services;
    }

    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbConfig>(configuration.GetSection(nameof(MongoDbConfig)));
        return services;
    }

    public static IServiceCollection AddDispacherHandle(this IServiceCollection services)
    {
        var commandHandle = services.BuildServiceProvider().GetRequiredService<ICommandHandler>();
        var dispacher = new CommandDispatcher();

        dispacher.RegisterHandler<NewPostCommand>(commandHandle.HandleAsync);
        dispacher.RegisterHandler<EditMessageCommand>(commandHandle.HandleAsync);
        dispacher.RegisterHandler<LikePostCommand>(commandHandle.HandleAsync);
        dispacher.RegisterHandler<AddCommentCommand>(commandHandle.HandleAsync);
        dispacher.RegisterHandler<NewPostCommand>(commandHandle.HandleAsync);
        dispacher.RegisterHandler<NewPostCommand>(commandHandle.HandleAsync);
        dispacher.RegisterHandler<NewPostCommand>(commandHandle.HandleAsync);
        return services;
    }

    public static IServiceCollection AddProducerConfig(this IServiceCollection services, IConfiguration configuration)
    {
        var producerConfig = configuration.GetSection(nameof(KafkaProducerConfig)).Get<KafkaProducerConfig>();
        services.AddSingleton(producerConfig);
        return services;
    }
}

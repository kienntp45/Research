using demoRedis;
using demoRedis.Reponsitory;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var redisConfig = builder.Configuration.GetSection(nameof(RedisSetting)).Get<RedisSetting>();
var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfig.GetRedisOptions());
builder.Services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
builder.Services.AddSingleton<IRedisRepository>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RedisRepository>>();
    var redis = sp.GetRequiredService<IConnectionMultiplexer>();
    return new RedisRepository(logger, redis);
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

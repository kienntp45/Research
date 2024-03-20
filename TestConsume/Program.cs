using Manonero.MessageBus.Kafka.Extensions;
using TestConsume;
using TestConsume.BackgroundTask;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var appSetting = AppSetting.MapValue(builder.Configuration);
builder.Services.AddSingleton(appSetting);
builder.Services.AddKafkaConsumers(builder =>
{
    builder.AddConsumer<AccountTask>(appSetting.GetConsumeConfig("Account"));
});
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseAuthorization();

app.MapControllers();
app.RunConsumer();

app.Run();

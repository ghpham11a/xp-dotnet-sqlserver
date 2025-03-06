using XpDotnetSqlServer.Repositories;
using XpDotnetSqlServer.Services;
using StackExchange.Redis;
using Confluent.Kafka;
using XpDotnetSqlServer.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddScoped<IAccountsRepository, AccountsRepository>();

// 1. Register the ConnectionMultiplexer (the Redis client) as a singleton
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configurationOptions = ConfigurationOptions.Parse(builder.Configuration["Redis:ConnectionString"]);
    // e.g. "localhost:6379" or "my-redis-server:6379"
    return ConnectionMultiplexer.Connect(configurationOptions);
});

// Producer
builder.Services.AddSingleton(sp =>
{
    var bootstrapServers = builder.Configuration["Kafka:BootstrapServers"];
    return new KafkaProducerService(bootstrapServers);
});

// Consumer as a Hosted Service
builder.Services.AddHostedService(sp =>
{
    var logger = sp.GetRequiredService<ILogger<KafkaConsumerService>>();
    var bootstrapServers = builder.Configuration["Kafka:BootstrapServers"];
    var topic = builder.Configuration["Kafka:Topic"];
    var groupId = builder.Configuration["Kafka:GroupId"];

    return new KafkaConsumerService(logger, bootstrapServers, topic, groupId);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

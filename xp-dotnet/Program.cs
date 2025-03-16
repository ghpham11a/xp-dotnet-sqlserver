using XpDotnetSqlServer.Repositories;
using XpDotnetSqlServer.Services;
using StackExchange.Redis;
using Confluent.Kafka;
using XpDotnetSqlServer.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddScoped<IAccountsRepository, AccountsRepository>();

// 1. Register the ConnectionMultiplexer (the Redis client) as a singleton
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
   var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST");
   var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT");
   var redisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD");
   var connectionString = $"{redisHost}:{redisPort}";

   // Parse the connection string from configuration, e.g. "localhost:6379"
   var configurationOptions = ConfigurationOptions.Parse(connectionString);

   // Set the password from configuration (ensure it's stored securely, e.g., in secrets or environment variables)
   configurationOptions.Password = redisPassword;

   return ConnectionMultiplexer.Connect(configurationOptions);
});

//// Producer
builder.Services.AddSingleton(sp =>
{
   var bootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS");
   var saslUsername = Environment.GetEnvironmentVariable("KAFKA_SASL_USERNAME");
   var saslPassword = Environment.GetEnvironmentVariable("KAFKA_SASL_PASSWORD");

   var config = new ProducerConfig
   {
       BootstrapServers = bootstrapServers,
       SecurityProtocol = SecurityProtocol.SaslPlaintext,
       SaslMechanism = SaslMechanism.Plain,                
       SaslUsername = saslUsername,
       SaslPassword = saslPassword
   };

   return new KafkaProducerService(bootstrapServers, saslUsername, saslPassword);
});

// Consumer as a Hosted Service
builder.Services.AddHostedService(sp =>
{
    var logger = sp.GetRequiredService<ILogger<KafkaConsumerService>>();
    var bootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS");
    var topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");
    var groupId = Environment.GetEnvironmentVariable("KAFKA_GROUP_ID");
    var saslUsername = Environment.GetEnvironmentVariable("KAFKA_SASL_USERNAME");
    var saslPassword = Environment.GetEnvironmentVariable("KAFKA_SASL_PASSWORD");

    return new KafkaConsumerService(logger, bootstrapServers, topic, groupId, saslUsername, saslPassword);
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

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

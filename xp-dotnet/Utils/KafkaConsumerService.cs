using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace XpDotnetSqlServer.Utils
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly ConsumerConfig _config;
        private readonly string _topic;

        public KafkaConsumerService(ILogger<KafkaConsumerService> logger, string bootstrapServers, string topic, string groupId, string userName, string password)
        {
            _logger = logger;
            _topic = topic;

            _config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                SecurityProtocol = SecurityProtocol.SaslPlaintext,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = userName,
                SaslPassword = password
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var consumer = new ConsumerBuilder<string, string>(_config).Build();
            consumer.Subscribe(_topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = consumer.Consume(TimeSpan.FromMilliseconds(100));
                    if (cr != null)
                    {
                        _logger.LogInformation($"Received message: {cr.Message.Value} with key: {cr.Message.Key}");
                        consumer.Commit(cr);
                    }
                    await Task.Delay(50, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Shutting down Kafka consumer.");
            }
            finally
            {
                consumer.Close();
            }
        }
    }
}

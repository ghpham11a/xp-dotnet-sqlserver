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

        public KafkaConsumerService(ILogger<KafkaConsumerService> logger, string bootstrapServers, string topic, string groupId)
        {
            _logger = logger;
            _topic = topic;
            _config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
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
                    var cr = consumer.Consume(stoppingToken);
                    _logger.LogInformation($"Received message: {cr.Message.Value} with key: {cr.Message.Key}");

                    // Process the message here
                    // For example, you could store data in Redis

                    // Manually commit offsets if desired
                    consumer.Commit(cr);
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

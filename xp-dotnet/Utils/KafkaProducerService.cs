﻿using Confluent.Kafka;
using static Confluent.Kafka.ConfigPropertyNames;

namespace XpDotnetSqlServer.Utils
{
    public class KafkaProducerService
    {
        private readonly IProducer<string, string> _producer;

        public KafkaProducerService(string bootstrapServers, string username, string password)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                SecurityProtocol = SecurityProtocol.SaslPlaintext,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = username,
                SaslPassword = password,
            };
            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task ProduceAsync(string topic, string key, string value)
        {
            var message = new Message<string, string> { Key = key, Value = value };
            await _producer.ProduceAsync(topic, message);
            // You might also want to handle delivery reports or exceptions
        }
    }
}

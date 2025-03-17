

using StackExchange.Redis;
using XpDotnetSqlServer.Models;
using XpDotnetSqlServer.Repositories;
using XpDotnetSqlServer.Utils;

namespace XpDotnetSqlServer.Services
{
    public class AccountsService : IAccountsService
    {
        private readonly IConfiguration _configuration;
        private readonly IAccountsRepository _accountsRepository;

        //// Add fields for the additional dependencies
        private readonly KafkaProducerService _kafkaProducer;
        private readonly IConnectionMultiplexer _redis;

        public AccountsService(
            IConfiguration configuration,
            IAccountsRepository accountsRepository,
            IConnectionMultiplexer redis,
            KafkaProducerService kafkaProducer
        )
        {
            _configuration = configuration;
            _accountsRepository = accountsRepository;
            _kafkaProducer = kafkaProducer;
            _redis = redis;
        }

        public async Task<IEnumerable<Account>> GetAllAsync()
        {

            // Optionally send a test message on startup(to a dedicated health check topic)
            try
            {
                // var deliveryResult = await _kafkaProducer.ProduceAsync("accounts-topic", "test", "test-value").GetAwaiter().GetResult();
                var deliveryResult = _kafkaProducer.ProduceAsync(
                    "accounts-topic",
                    "test-key",
                    "test-value"
                ).GetAwaiter();
                deliveryResult.GetResult();
                Console.WriteLine($"Kafka producer test message delivered to {deliveryResult}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kafka producer failed to deliver test message: {ex.Message}");
                // Consider handling or throwing exception
            }

            return await _accountsRepository.GetAllAsync();
        }

        public async Task<Account?> GetByIdAsync(int id)
        {
            return await _accountsRepository.GetByIdAsync(id);
        }

        public async Task<int> CreateAsync(Account newAccount)
        {
            return await _accountsRepository.CreateAsync(newAccount);
        }

        public async Task<bool> UpdateAsync(int id, Account updatedAccount)
        {
            return await _accountsRepository.UpdateAsync(id, updatedAccount);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _accountsRepository.DeleteAsync(id);
        }
    }
}

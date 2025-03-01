using Microsoft.Data.SqlClient;
using System.Security.Principal;
using XpDotnetSqlServer.Models;
using XpDotnetSqlServer.Repositories;

namespace XpDotnetSqlServer.Services
{
    public class AccountsService : IAccountsService
    {
        private readonly IConfiguration _configuration;
        private readonly IAccountsRepository _accountsRepository;

        public AccountsService(IConfiguration configuration, IAccountsRepository accountsRepository)
        {
            _configuration = configuration;
            _accountsRepository = accountsRepository;   
        }

        public async Task<IEnumerable<Account>> GetAllAsync()
        {
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

using XpDotnetSqlServer.Models;

namespace XpDotnetSqlServer.Repositories
{
    public interface IAccountsRepository
    {
        Task<IEnumerable<Account>> GetAllAsync();
        Task<Account?> GetByIdAsync(int id);
        Task<int> CreateAsync(Account newAccount);
        Task<bool> UpdateAsync(int id, Account updatedAccount);
        Task<bool> DeleteAsync(int id);
    }
}

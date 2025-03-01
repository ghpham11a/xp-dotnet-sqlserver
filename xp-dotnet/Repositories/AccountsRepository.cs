using Microsoft.Data.SqlClient;
using XpDotnetSqlServer.Models;

namespace XpDotnetSqlServer.Repositories
{
    public class AccountsRepository : IAccountsRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public AccountsRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = Environment.GetEnvironmentVariable("SQLSERVER_CONNECTION") ?? _configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            var results = new List<Account>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                @"SELECT Id, Email, DateOfBirth, AccountNumber, Balance, CreatedAt 
                  FROM Accounts", conn);

            await conn.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                results.Add(new Account
                {
                    Id = reader.GetInt32(0),
                    Email = reader.GetString(1),
                    DateOfBirth = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                    AccountNumber = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Balance = reader.GetDecimal(4),
                    CreatedAt = reader.GetDateTime(5)
                });
            }

            return results;
        }

        public async Task<Account?> GetByIdAsync(int id)
        {
            Account? account = null;

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                @"SELECT Id, Email, DateOfBirth, AccountNumber, Balance, CreatedAt
                  FROM Accounts
                  WHERE Id = @Id", conn);

            cmd.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            if (await reader.ReadAsync().ConfigureAwait(false))
            {
                account = new Account
                {
                    Id = reader.GetInt32(0),
                    Email = reader.GetString(1),
                    DateOfBirth = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                    AccountNumber = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Balance = reader.GetDecimal(4),
                    CreatedAt = reader.GetDateTime(5)
                };
            }

            return account;
        }

        public async Task<int> CreateAsync(Account newAccount)
        {

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                @"INSERT INTO Accounts (Email, DateOfBirth, AccountNumber, Balance, CreatedAt)
                  OUTPUT INSERTED.Id
                  VALUES (@Email, @DateOfBirth, @AccountNumber, @Balance, @CreatedAt)", conn);

            cmd.Parameters.AddWithValue("@Email", newAccount.Email);
            cmd.Parameters.AddWithValue("@DateOfBirth", (object?)newAccount.DateOfBirth ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AccountNumber", (object?)newAccount.AccountNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Balance", newAccount.Balance);
            cmd.Parameters.AddWithValue("@CreatedAt", newAccount.CreatedAt);

            await conn.OpenAsync().ConfigureAwait(false);
            var insertedId = (int)await cmd.ExecuteScalarAsync().ConfigureAwait(false);

            return insertedId;
        }

        public async Task<bool> UpdateAsync(int id, Account updatedAccount)
        {

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                @"UPDATE Accounts
                  SET Email = @Email,
                      DateOfBirth = @DateOfBirth,
                      AccountNumber = @AccountNumber,
                      Balance = @Balance
                  WHERE Id = @Id", conn);

            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Email", updatedAccount.Email);
            cmd.Parameters.AddWithValue("@DateOfBirth", (object?)updatedAccount.DateOfBirth ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AccountNumber", (object?)updatedAccount.AccountNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Balance", updatedAccount.Balance);

            await conn.OpenAsync().ConfigureAwait(false);
            var rowsAffected = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                @"DELETE FROM Accounts
                  WHERE Id = @Id", conn);

            cmd.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync().ConfigureAwait(false);
            var rowsAffected = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

            return rowsAffected > 0;
        }
    }
}

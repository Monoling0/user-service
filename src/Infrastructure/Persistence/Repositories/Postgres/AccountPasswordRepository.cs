using Npgsql;
using UserService.Application.Abstractions.Persistence.Repositories;
using UserService.Application.Abstractions.Persistence.Requests;

namespace Infrastructure.Persistence.Repositories.Postgres;

public class AccountPasswordRepository : IAccountPasswordRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public AccountPasswordRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<long> SavePasswordAsync(
        SavePasswordRepositoryRequest request,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
                           insert into account_passwords (password_hash)
                           values (:password_hash)
                           returning password_id
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("password_hash", request.PasswordHash);

        object? id = await command.ExecuteScalarAsync(cancellationToken);

        if (id == null)
        {
            throw new InvalidOperationException("Expected a long value from SQL returning instruction but got null");
        }

        return (long)id;
    }

    public async Task<string?> GetPasswordHashAsync(long passwordId, CancellationToken cancellationToken)
    {
        const string sql = """
                           select password_hash
                           from account_passwords
                           where (password_id = :password_id)
                           """;
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("password_id", passwordId);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        return !await reader.ReadAsync(cancellationToken)
            ? null
            : reader.GetString(0);
    }

    public async Task<bool> UpdatePassword(long passwordId, string newHash, CancellationToken cancellationToken)
    {
        const string sql = """
                           update account_passwords
                           set password_hash = :password_hash
                           where password_id = :password_id
                           """;

        await using NpgsqlConnection connection =
            await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("password_id", passwordId);
        command.Parameters.AddWithValue("password_hash", newHash);

        int affectedRows =
            await command.ExecuteNonQueryAsync(cancellationToken);

        return affectedRows > 0;
    }
}
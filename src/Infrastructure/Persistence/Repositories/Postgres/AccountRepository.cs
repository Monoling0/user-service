using Npgsql;
using System.Runtime.CompilerServices;
using UserService.Application.Abstractions.Persistence.Repositories;
using UserService.Application.Abstractions.Persistence.Requests;
using UserService.Application.Models.Accounts;

namespace Infrastructure.Persistence.Repositories.Postgres;

public class AccountRepository : IAccountRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public AccountRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<long> CreateAsync(
        CreateAccountRepositoryRequest request,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
                           insert into accounts (role, password_id, email, account_created_at, account_updated_at)
                           values (:role, :password_id, :email, :account_created_at, :account_updated_at)
                           returning account_id
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("role", request.Role);
        command.Parameters.AddWithValue("password_id", request.PasswordId);
        command.Parameters.AddWithValue("email", request.Email);
        command.Parameters.AddWithValue("account_created_at", request.AccountCreatedAt.ToUniversalTime());
        command.Parameters.AddWithValue("account_updated_at", request.AccountCreatedAt.ToUniversalTime());

        object? id = await command.ExecuteScalarAsync(cancellationToken);

        if (id == null)
        {
            throw new InvalidOperationException("Expected a long value from SQL returning instruction but got null");
        }

        return (long)id;
    }

    public async Task<bool> ExistsAsync(long accountId, CancellationToken cancellationToken)
    {
        const string sql = """
                           select account_id
                           from accounts
                           where (account_id = :account_id)
                           """;
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("account_id", accountId);

        object? id = await command.ExecuteScalarAsync(cancellationToken);

        return id != null;
    }

    public async Task<Account?> GetAccountAsync(long accountId, CancellationToken cancellationToken)
    {
        const string sql = """
                           select account_id, role, password_id, email, account_created_at, account_updated_at
                           from accounts
                           where (account_id = :account_id)
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("account_id", accountId);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return !await reader.ReadAsync(cancellationToken)
            ? null
            : new Account(
                AccountId: reader.GetInt64(0),
                Role: reader.GetFieldValue<Roles>(1),
                PasswordId: reader.GetInt64(2),
                Email: reader.GetString(3),
                AccountCreatedAt: reader.GetFieldValue<DateTimeOffset>(4),
                AccountUpdatedAt: reader.GetFieldValue<DateTimeOffset>(5));
    }

    public async IAsyncEnumerable<Account> GetAllAccountsAsync(
        GetAllAccountsRepositoryRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
                           select account_id, role, password_id, email, account_created_at, account_updated_at
                           from accounts
                           where (:last_seen_id is null or account_id > :last_seen_id)
                           order by account_id
                           limit :limit
                           """;

        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter<long?>("last_seen_id", request.LastSeenId),
            new NpgsqlParameter<int>("limit", request.PageSize),
        };

        await using NpgsqlConnection connection =
            await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddRange(parameters.ToArray());

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new Account(
                AccountId: reader.GetInt64(0),
                Role: reader.GetFieldValue<Roles>(1),
                PasswordId: reader.GetInt64(2),
                Email: reader.GetString(3),
                AccountCreatedAt: reader.GetFieldValue<DateTimeOffset>(4),
                AccountUpdatedAt: reader.GetFieldValue<DateTimeOffset>(5));
        }
    }

    public async Task<bool> ExistsEmailAsync(string email, CancellationToken cancellationToken)
    {
        const string sql = """
                           select account_id
                           from accounts
                           where (email = :email)
                           """;
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("email", email);

        object? id = await command.ExecuteScalarAsync(cancellationToken);

        return id != null;
    }

    public async Task<bool> UpdateAccountAsync(
        UpdateAccountRepositoryRequest request,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           update accounts
                           set email = coalesce(:new_email, email),
                               account_updated_at = :updated_at
                           where (account_id = :account_id)
                           """;

        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter<long>("account_id", request.AccountId),
            new NpgsqlParameter<string?>("new_email", request.Email.HasValue ? request.Email.Value : null),
            new NpgsqlParameter<DateTimeOffset>("updated_at", DateTimeOffset.Now.ToUniversalTime()),
        };

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddRange(parameters.ToArray());

        int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

        return rowsAffected > 0;
    }
}
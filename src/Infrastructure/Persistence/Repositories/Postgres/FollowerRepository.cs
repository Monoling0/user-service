using Npgsql;
using System.Runtime.CompilerServices;
using UserService.Application.Abstractions.Persistence.Repositories;
using UserService.Application.Abstractions.Persistence.Requests;

namespace Infrastructure.Persistence.Repositories.Postgres;

public class FollowerRepository : IFollowerRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public FollowerRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task AddFollowerAsync(
        AddFollowerRepositoryRequest request,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
                           insert into followers (follower_id, followee_id, created_at)
                           values (:follower_id, :followee_id, :created_at)
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("follower_id", request.FollowerId);
        command.Parameters.AddWithValue("followee_id", request.FolloweeId);
        command.Parameters.AddWithValue("created_at", request.CreatedAt.ToUniversalTime());

        await command.ExecuteScalarAsync(cancellationToken);
    }

    public async Task<bool> IsSubscribedAsync(long followerId, long followeeId, CancellationToken cancellationToken)
    {
        const string sql = """
                           select follower_id
                           from followers
                           where (follower_id = :follower_id
                                      and followee_id = :followee_id)
                           """;
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("follower_id", followerId);
        command.Parameters.AddWithValue("followee_id", followeeId);

        object? id = await command.ExecuteScalarAsync(cancellationToken);

        return id != null;
    }

    public async IAsyncEnumerable<long> GetAllFollowersIdsAsync(
        GetAllFollowersRepositoryRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
                           select follower_id
                           from followers
                           where (followee_id = :followee_id)
                           and (:last_seen_id is null or follower_id > :last_seen_id)
                           order by follower_id
                           limit :limit
                           """;

        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter<long>("followee_id", request.FolloweeId),
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
            yield return reader.GetInt64(0);
        }
    }
}
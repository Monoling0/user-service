using Npgsql;
using System.Runtime.CompilerServices;
using UserService.Application.Abstractions.Persistence.Repositories;
using UserService.Application.Abstractions.Persistence.Requests;
using UserService.Application.Models.StudentProfiles;

namespace Infrastructure.Persistence.Repositories.Postgres;

public class StudentProfileRepository : IStudentProfileRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public StudentProfileRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task CreateAsync(
        CreateStudentProfileRepositoryRequest request,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
                           insert into student_profiles (account_id, nickname, profile_photo_url)
                           values (:account_id, :nickname, :profile_photo_url)
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);

        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter<long>("account_id", request.AccountId),
            new NpgsqlParameter<string>("nickname", request.Nickname),
            new NpgsqlParameter<string?>("profile_photo_url", request.ProfilePhotoUrl),
        };
        command.Parameters.AddRange(parameters.ToArray());

        await command.ExecuteScalarAsync(cancellationToken);
    }

    public async Task<StudentProfile?> GetStudentProfileAsync(
        long accountId,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           select account_id, nickname, profile_photo_url
                           from student_profiles
                           where (account_id = :account_id)
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("account_id", accountId);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return !await reader.ReadAsync(cancellationToken)
            ? null
            : new StudentProfile(
                AccountId: reader.GetInt64(0),
                Nickname: reader.GetString(1),
                ProfilePhotoUrl: reader.IsDBNull(2) ? null : reader.GetString(2));
    }

    public async IAsyncEnumerable<StudentProfile> GetAllStudentProfilesAsync(
        GetAllStudentProfilesRepositoryRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
                           select account_id, nickname, profile_photo_url
                           from student_profiles
                           where (:ids is null or account_id = any(:ids))
                               and (:last_seen_id is null or account_id > :last_seen_id)
                           order by account_id
                           limit :limit
                           """;

        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter<long?>("last_seen_id", request.LastSeenId),
            new NpgsqlParameter<int>("limit", request.PageSize),
            new NpgsqlParameter<long[]?>("ids", request.Ids),
        };

        await using NpgsqlConnection connection =
            await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddRange(parameters.ToArray());

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new StudentProfile(
                AccountId: reader.GetInt64(0),
                Nickname: reader.GetString(1),
                ProfilePhotoUrl: reader.GetFieldValue<string?>(2));
        }
    }

    public async Task<bool> ExistsNicknameAsync(
        string nickname,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           select account_id
                           from student_profiles
                           where (nickname = :nickname)
                           """;
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("nickname", nickname);

        object? id = await command.ExecuteScalarAsync(cancellationToken);

        return id != null;
    }

    public async Task<bool> UpdateStudentProfile(UpdateStudentProfileRepositoryRequest request, CancellationToken cancellationToken)
    {
        const string sql = """
                           update student_profiles
                           set nickname = coalesce(:new_nickname, nickname),
                               profile_photo_url =
                                   case
                                       when :photo_set = true then :photo_value
                                       else profile_photo_url
                                   end
                           where (account_id = :account_id)
                           """;

        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter<long>("account_id", request.AccountId),
            new NpgsqlParameter<bool>("photo_set", request.ProfilePhotoUrl.HasValue),
            new NpgsqlParameter<string?>("new_profile_photo_url", request.ProfilePhotoUrl.Value),
        };

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddRange(parameters.ToArray());

        int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

        return rowsAffected > 0;
    }
}
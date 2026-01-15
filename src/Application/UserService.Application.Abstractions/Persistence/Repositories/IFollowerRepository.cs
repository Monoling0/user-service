using UserService.Application.Abstractions.Persistence.Requests;

namespace UserService.Application.Abstractions.Persistence.Repositories;

public interface IFollowerRepository
{
    Task AddFollowerAsync(
        AddFollowerRepositoryRequest request,
        CancellationToken cancellationToken);

    Task<bool> IsSubscribedAsync(
        long followerId,
        long followeeId,
        CancellationToken cancellationToken);

    IAsyncEnumerable<long> GetAllFollowersIdsAsync(
        GetAllFollowersRepositoryRequest request,
        CancellationToken cancellationToken);
}
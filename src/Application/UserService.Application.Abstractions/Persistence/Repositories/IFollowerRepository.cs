using UserService.Application.Abstractions.Persistence.Requests;

namespace UserService.Application.Abstractions.Persistence.Repositories;

public interface IFollowerRepository
{
    Task AddFollower(
        AddFollowerRepositoryRequest request,
        CancellationToken cancellationToken = default);
}
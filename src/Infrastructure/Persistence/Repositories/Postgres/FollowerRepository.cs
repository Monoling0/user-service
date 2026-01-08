using UserService.Application.Abstractions.Persistence.Repositories;
using UserService.Application.Abstractions.Persistence.Requests;

namespace Infrastructure.Persistence.Repositories.Postgres;

public class FollowerRepository : IFollowerRepository
{
    public Task AddFollower(AddFollowerRepositoryRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
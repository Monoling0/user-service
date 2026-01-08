using UserService.Application.Abstractions.Persistence.Repositories;
using UserService.Application.Abstractions.Persistence.Requests;

namespace Infrastructure.Persistence.Repositories.Postgres;

public class AccountPasswordRepository : IAccountPasswordRepository
{
    public Task<long> SavePasswordAsync(SavePasswordRepositoryRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
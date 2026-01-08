using UserService.Application.Abstractions.Persistence.Repositories;
using UserService.Application.Abstractions.Persistence.Requests;

namespace Infrastructure.Persistence.Repositories.Postgres;

public class RoleRepository : IRoleRepository
{
    public Task<long> CreateRoleAsync(CreateRoleRepositoryRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
using UserService.Application.Abstractions.Persistence.Requests;

namespace UserService.Application.Abstractions.Persistence.Repositories;

public interface IRoleRepository
{
    Task<long> CreateRoleAsync(
        CreateRoleRepositoryRequest request,
        CancellationToken cancellationToken = default);
}
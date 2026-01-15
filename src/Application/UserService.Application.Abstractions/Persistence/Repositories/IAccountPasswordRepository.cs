using UserService.Application.Abstractions.Persistence.Requests;

namespace UserService.Application.Abstractions.Persistence.Repositories;

public interface IAccountPasswordRepository
{
    Task<long> SavePasswordAsync(
        SavePasswordRepositoryRequest request,
        CancellationToken cancellationToken);

    Task<string?> GetPasswordHashAsync(
        long passwordId,
        CancellationToken cancellationToken);

    Task<bool> UpdatePassword(
        long passwordId,
        string newHash,
        CancellationToken cancellationToken);
}
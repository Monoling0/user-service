using UserService.Application.Abstractions.Persistence.Requests;

namespace UserService.Application.Abstractions.Persistence.Repositories;

public interface IAccountRepository
{
    Task<long> CreateAsync(
        CreateAccountRepositoryRequest request,
        CancellationToken cancellationToken = default);
}
using UserService.Application.Abstractions.Persistence.Repositories;
using UserService.Application.Abstractions.Persistence.Requests;

namespace Infrastructure.Persistence.Repositories.Postgres;

public class AccountRepository : IAccountRepository
{
    public Task<long> CreateAsync(CreateAccountRepositoryRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
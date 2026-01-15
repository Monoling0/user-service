using UserService.Application.Abstractions.Persistence.Requests;
using UserService.Application.Models.Accounts;

namespace UserService.Application.Abstractions.Persistence.Repositories;

public interface IAccountRepository
{
    Task<long> CreateAsync(
        CreateAccountRepositoryRequest request,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(
        long accountId,
        CancellationToken cancellationToken);

    Task<Account?> GetAccountAsync(
        long accountId,
        CancellationToken cancellationToken);

    IAsyncEnumerable<Account> GetAllAccountsAsync(
        GetAllAccountsRepositoryRequest request,
        CancellationToken cancellationToken);

    Task<bool> ExistsEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task<bool> UpdateAccountAsync(
        UpdateAccountRepositoryRequest request,
        CancellationToken cancellationToken);
}
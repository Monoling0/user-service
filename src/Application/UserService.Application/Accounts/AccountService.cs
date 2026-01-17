using System.Transactions;
using UserService.Application.Abstractions.Persistence.Repositories;
using UserService.Application.Abstractions.Persistence.Requests;
using UserService.Application.Accounts.Handlers;
using UserService.Application.Contracts;
using UserService.Application.Contracts.Operations;
using UserService.Application.Models.Accounts;
using UserService.Application.Models.Common;

namespace UserService.Application.Accounts;

public class AccountService : IAccountService
{
    private const int MinimalPageSize = 1;

    private readonly IAccountRepository _accountRepository;
    private readonly IAccountPasswordRepository _accountPasswordRepository;

    private readonly StudentHandler _studentHandler;

    public AccountService(
        IAccountRepository accountRepository,
        IAccountPasswordRepository accountPasswordRepository,
        StudentHandler studentHandler)
    {
        _accountRepository = accountRepository;
        _accountPasswordRepository = accountPasswordRepository;
        _studentHandler = studentHandler;
    }

    public Task<RegisterStudent.Result> RegisterStudentAsync(
        RegisterStudent.Request request,
        CancellationToken cancellationToken)
    {
        return _studentHandler.RegisterStudentAsync(request, cancellationToken);
    }

    public async Task<AddCreator.Result> AddCreatorAsync(
        AddCreator.Request request,
        CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        if (await _accountRepository.ExistsEmailAsync(request.Email, cancellationToken))
        {
            return new AddCreator.Result.EmailAlreadyExists();
        }

        var savePasswordRepositoryRequest = new SavePasswordRepositoryRequest(
            request.PasswordHash);
        long passwordId =
            await _accountPasswordRepository.SavePasswordAsync(savePasswordRepositoryRequest, cancellationToken);

        DateTimeOffset createdAt = DateTime.Now;
        var createAccountRepositoryRequest = new CreateAccountRepositoryRequest(
            Roles.Creator,
            passwordId,
            request.Email,
            createdAt);

        long accountId = await _accountRepository.CreateAsync(createAccountRepositoryRequest, cancellationToken);

        transaction.Complete();

        return new AddCreator.Result.Success(accountId);
    }

    public Task<CreateSubscription.Result> CreateSubscriptionAsync(
        CreateSubscription.Request request,
        CancellationToken cancellationToken)
    {
        return _studentHandler.CreateSubscriptionAsync(request, cancellationToken);
    }

    public async Task<bool> ExistsAccountAsync(long accountId, CancellationToken cancellationToken)
    {
        return await _accountRepository.ExistsAsync(accountId, cancellationToken);
    }

    public async Task<GetAccount.Result> GetAccountAsync(long accountId, CancellationToken cancellationToken)
    {
        Account? account = await _accountRepository.GetAccountAsync(accountId, cancellationToken);

        return account == null ? new GetAccount.Result.AccountNotFound() : new GetAccount.Result.Success(account);
    }

    public Task<GetStudentProfileData.Result> GetStudentProfileDataAsync(
        long accountId,
        CancellationToken cancellationToken)
    {
        return _studentHandler.GetStudentProfileDataAsync(accountId, cancellationToken);
    }

    public async Task<GetPasswordHash.Result> GetPasswordHashAsync(long passwordId, CancellationToken cancellationToken)
    {
        string? hash = await _accountPasswordRepository.GetPasswordHashAsync(passwordId, cancellationToken);

        return hash == null ? new GetPasswordHash.Result.PasswordNotFound() : new GetPasswordHash.Result.Success(hash);
    }

    public async Task<GetAllAccounts.Result> GetAllAccountsAsync(
        GetAllAccounts.Request request,
        CancellationToken cancellationToken)
    {
        if (request.PageSize < MinimalPageSize)
        {
            return new GetAllAccounts.Result.InvalidPageSize();
        }

        long? lastSeenId = request.PageToken?.LastSeenId;
        var getAllAccountsRepositoryRequest = new GetAllAccountsRepositoryRequest(
            request.PageSize,
            request.Ids,
            request.Role,
            lastSeenId);
        List<Account> page =
            await _accountRepository
                .GetAllAccountsAsync(getAllAccountsRepositoryRequest, cancellationToken)
                .ToListAsync(cancellationToken);
        if (page.Count < request.PageSize)
        {
            return new GetAllAccounts.Result.Success(page, null);
        }

        long lastSeenIdToReturn = page[^1].AccountId;

        return new GetAllAccounts.Result.Success(page, new PageToken(lastSeenIdToReturn));
    }

    public Task<GetAllStudentProfiles.Result> GetAllStudentProfilesAsync(
        GetAllStudentProfiles.Request request,
        CancellationToken cancellationToken)
    {
        return _studentHandler.GetAllStudentProfilesAsync(request, cancellationToken);
    }

    public Task<GetFollowers.Result> GetFollowersAsync(
        GetFollowers.Request request,
        CancellationToken cancellationToken)
    {
        return _studentHandler.GetFollowersAsync(request, cancellationToken);
    }

    public async Task<UpdateAccount.Result> UpdateAccountAsync(
        UpdateAccount.Request request,
        CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        Account? account = await _accountRepository
            .GetAccountAsync(request.AccountId, cancellationToken);
        if (account == null)
        {
            return new UpdateAccount.Result.AccountNotFound();
        }

        long passwordId = account.PasswordId;
        string passwordHash = await _accountPasswordRepository.GetPasswordHashAsync(passwordId, cancellationToken) ??
                              throw new InvalidOperationException(
                                  $"Failed to get password hash for account ID {account.AccountId}");
        bool hasChanges =
            (request.Email.HasValue && request.Email.Value != account.Email)
            || (request.PasswordHash.HasValue && request.PasswordHash.Value != passwordHash);

        if (!hasChanges)
        {
            return new UpdateAccount.Result.NoChanges();
        }

        if (request.Email.HasValue &&
            await _accountRepository.ExistsEmailAsync(request.Email.Value, cancellationToken))
        {
            return new UpdateAccount.Result.EmailAlreadyExists();
        }

        if (request.PasswordHash.HasValue)
        {
            await _accountPasswordRepository
                .UpdatePassword(passwordId, request.PasswordHash.Value, cancellationToken);
        }

        if (!request.Email.HasValue)
        {
            transaction.Complete();

            return new UpdateAccount.Result.Success();
        }

        var updateRequest = new UpdateAccountRepositoryRequest(
            request.AccountId,
            request.Email);

        await _accountRepository.UpdateAccountAsync(updateRequest, cancellationToken);

        transaction.Complete();

        return new UpdateAccount.Result.Success();
    }

    public Task<UpdateStudentProfile.Result> UpdateStudentProfileAsync(
        UpdateStudentProfile.Request request,
        CancellationToken cancellationToken)
    {
        return _studentHandler.UpdateStudentProfileAsync(request, cancellationToken);
    }
}
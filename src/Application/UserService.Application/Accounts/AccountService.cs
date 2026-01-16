using System.Transactions;
using UserService.Application.Abstractions.Persistence.Repositories;
using UserService.Application.Abstractions.Persistence.Requests;
using UserService.Application.Contracts;
using UserService.Application.Contracts.Operations;
using UserService.Application.Models.Accounts;
using UserService.Application.Models.Common;
using UserService.Application.Models.StudentProfiles;

namespace UserService.Application.Accounts;

public class AccountService : IAccountService
{
    private const int MinimalPageSize = 1;

    private readonly IAccountRepository _accountRepository;
    private readonly IAccountPasswordRepository _accountPasswordRepository;
    private readonly IStudentProfileRepository _studentProfileRepository;
    private readonly IFollowerRepository _followerRepository;

    public AccountService(
        IAccountRepository accountRepository,
        IAccountPasswordRepository accountPasswordRepository,
        IStudentProfileRepository studentProfileRepository,
        IFollowerRepository followerRepository)
    {
        _accountRepository = accountRepository;
        _accountPasswordRepository = accountPasswordRepository;
        _studentProfileRepository = studentProfileRepository;
        _followerRepository = followerRepository;
    }

    public async Task<RegisterStudent.Result> RegisterStudentAsync(
        RegisterStudent.Request request,
        CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        if (await _accountRepository.ExistsEmailAsync(request.Email, cancellationToken))
        {
            return new RegisterStudent.Result.EmailAlreadyExists();
        }

        if (await _studentProfileRepository.ExistsNicknameAsync(request.Nickname, cancellationToken))
        {
            return new RegisterStudent.Result.NicknameAlreadyExists();
        }

        var savePasswordRepositoryRequest = new SavePasswordRepositoryRequest(
            request.PasswordHash);
        long passwordId =
            await _accountPasswordRepository.SavePasswordAsync(savePasswordRepositoryRequest, cancellationToken);

        DateTimeOffset createdAt = DateTime.Now;
        var createAccountRepositoryRequest = new CreateAccountRepositoryRequest(
            Roles.Student,
            passwordId,
            request.Email,
            createdAt);
        long accountId = await _accountRepository.CreateAsync(createAccountRepositoryRequest, cancellationToken);

        var createStudentProfileRepositoryRequest = new CreateStudentProfileRepositoryRequest(
            accountId,
            request.Nickname,
            request.ProfilePhotoUrl);
        await _studentProfileRepository.CreateAsync(createStudentProfileRepositoryRequest, cancellationToken);

        transaction.Complete();

        return new RegisterStudent.Result.Success(accountId);
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

    public async Task<CreateSubscription.Result> CreateSubscriptionAsync(
        CreateSubscription.Request request,
        CancellationToken cancellationToken)
    {
        if (request.FollowerId == request.FolloweeId)
        {
            return new CreateSubscription.Result.IdenticalIds();
        }

        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        if (!await _accountRepository.ExistsAsync(request.FollowerId, cancellationToken))
        {
            return new CreateSubscription.Result.FollowerNotFound();
        }

        if (!await _accountRepository.ExistsAsync(request.FolloweeId, cancellationToken))
        {
            return new CreateSubscription.Result.FolloweeNotFound();
        }

        if (await _followerRepository.IsSubscribedAsync(request.FollowerId, request.FolloweeId, cancellationToken))
        {
            return new CreateSubscription.Result.AlreadySubscribed();
        }

        var addFollowerRepositoryRequest = new AddFollowerRepositoryRequest(
            request.FollowerId,
            request.FolloweeId,
            DateTimeOffset.Now);
        await _followerRepository.AddFollowerAsync(addFollowerRepositoryRequest, cancellationToken);

        transaction.Complete();

        return new CreateSubscription.Result.Success();
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

    public async Task<GetStudentProfileData.Result> GetStudentProfileDataAsync(
        long accountId,
        CancellationToken cancellationToken)
    {
        StudentProfile? studentProfile =
            await _studentProfileRepository.GetStudentProfileAsync(accountId, cancellationToken);

        return studentProfile == null
            ? new GetStudentProfileData.Result.StudentProfileNotFound()
            : new GetStudentProfileData.Result.Success(studentProfile);
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

    public async Task<GetAllStudentProfiles.Result> GetAllStudentProfilesAsync(
        GetAllStudentProfiles.Request request,
        CancellationToken cancellationToken)
    {
        if (request.PageSize < MinimalPageSize)
        {
            return new GetAllStudentProfiles.Result.InvalidPageSize();
        }

        long? lastSeenId = request.PageToken?.LastSeenId;
        var getAllStudentProfilesRepositoryRequest = new GetAllStudentProfilesRepositoryRequest(
            request.PageSize,
            request.Ids,
            lastSeenId);
        List<StudentProfile> page =
            await _studentProfileRepository
                .GetAllStudentProfilesAsync(getAllStudentProfilesRepositoryRequest, cancellationToken)
                .ToListAsync(cancellationToken);
        if (page.Count < request.PageSize)
        {
            return new GetAllStudentProfiles.Result.Success(page, null);
        }

        long lastSeenIdToReturn = page[^1].AccountId;

        return new GetAllStudentProfiles.Result.Success(page, new PageToken(lastSeenIdToReturn));
    }

    public async Task<GetFollowers.Result> GetFollowersAsync(
        GetFollowers.Request request,
        CancellationToken cancellationToken)
    {
        if (request.PageSize < MinimalPageSize)
        {
            return new GetFollowers.Result.InvalidPageSize();
        }

        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        var getAllFollowersRepositoryRequest = new GetAllFollowersRepositoryRequest(
            request.StudentId,
            request.PageSize,
            request.PageToken?.LastSeenId);
        List<long> ids =
            await _followerRepository
                .GetAllFollowersIdsAsync(getAllFollowersRepositoryRequest, cancellationToken)
                .ToListAsync(cancellationToken);

        var getAllStudentProfilesRequest = new GetAllStudentProfilesRepositoryRequest(
            request.PageSize,
            ids.ToArray(),
            request.PageToken?.LastSeenId);
        List<StudentProfile> page =
            await _studentProfileRepository
                .GetAllStudentProfilesAsync(getAllStudentProfilesRequest, cancellationToken)
                .ToListAsync(cancellationToken);

        if (ids.Count < request.PageSize)
        {
            return new GetFollowers.Result.Success(page, null);
        }

        long lastSeenIdToReturn = ids[^1];

        transaction.Complete();

        return new GetFollowers.Result.Success(page, new PageToken(lastSeenIdToReturn));
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
            return new UpdateAccount.Result.Success();
        }

        var updateRequest = new UpdateAccountRepositoryRequest(
            request.AccountId,
            request.Email);

        await _accountRepository.UpdateAccountAsync(updateRequest, cancellationToken);

        transaction.Complete();

        return new UpdateAccount.Result.Success();
    }

    public async Task<UpdateStudentProfile.Result> UpdateStudentProfileAsync(
        UpdateStudentProfile.Request request,
        CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        StudentProfile? studentProfile = await _studentProfileRepository
            .GetStudentProfileAsync(request.AccountId, cancellationToken);
        if (studentProfile == null)
        {
            return new UpdateStudentProfile.Result.StudentNotFound();
        }

        bool hasChanges =
            (request.Nickname.HasValue && request.Nickname.Value != studentProfile.Nickname)
            || (request.ProfilePhotoUrl.HasValue && request.ProfilePhotoUrl.Value != studentProfile.ProfilePhotoUrl);

        if (!hasChanges)
        {
            return new UpdateStudentProfile.Result.NoChanges();
        }

        if (request.Nickname.HasValue &&
            await _studentProfileRepository.ExistsNicknameAsync(request.Nickname.Value, cancellationToken))
        {
            return new UpdateStudentProfile.Result.NicknameAlreadyExists();
        }

        if (!request.ProfilePhotoUrl.HasValue)
        {
            return new UpdateStudentProfile.Result.Success();
        }

        var updateRequest = new UpdateStudentProfileRepositoryRequest(
            request.AccountId,
            request.Nickname,
            request.ProfilePhotoUrl);

        await _studentProfileRepository.UpdateStudentProfile(updateRequest, cancellationToken);

        transaction.Complete();

        return new UpdateStudentProfile.Result.Success();
    }
}
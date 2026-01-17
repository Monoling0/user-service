using System.Transactions;
using UserService.Application.Abstractions.Persistence.Repositories;
using UserService.Application.Abstractions.Persistence.Requests;
using UserService.Application.Contracts.Operations;
using UserService.Application.Models.Accounts;
using UserService.Application.Models.Common;
using UserService.Application.Models.StudentProfiles;

namespace UserService.Application.Accounts.Handlers;

public class StudentHandler
{
    private const int MinimalPageSize = 1;

    private readonly IAccountRepository _accountRepository;
    private readonly IAccountPasswordRepository _accountPasswordRepository;
    private readonly IStudentProfileRepository _studentProfileRepository;
    private readonly IFollowerRepository _followerRepository;

    public StudentHandler(
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

        Account? followee = await _accountRepository
            .GetAccountAsync(request.StudentId, cancellationToken);
        if (followee == null)
        {
            return new GetFollowers.Result.AccountNotFound();
        }

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
            transaction.Complete();

            return new GetFollowers.Result.Success(page, null);
        }

        long lastSeenIdToReturn = ids[^1];

        transaction.Complete();

        return new GetFollowers.Result.Success(page, new PageToken(lastSeenIdToReturn));
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

        var updateRequest = new UpdateStudentProfileRepositoryRequest(
            request.AccountId,
            request.Nickname,
            request.ProfilePhotoUrl);

        await _studentProfileRepository.UpdateStudentProfile(updateRequest, cancellationToken);

        transaction.Complete();

        return new UpdateStudentProfile.Result.Success();
    }
}
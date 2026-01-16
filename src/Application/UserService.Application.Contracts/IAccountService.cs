using UserService.Application.Contracts.Operations;

namespace UserService.Application.Contracts;

public interface IAccountService
{
    Task<RegisterStudent.Result> RegisterStudentAsync(
        RegisterStudent.Request request,
        CancellationToken cancellationToken);

    Task<AddCreator.Result> AddCreatorAsync(
        AddCreator.Request request,
        CancellationToken cancellationToken);

    Task<CreateSubscription.Result> CreateSubscriptionAsync(
        CreateSubscription.Request request,
        CancellationToken cancellationToken);

    Task<bool> ExistsAccountAsync(
        long accountId,
        CancellationToken cancellationToken);

    Task<GetAccount.Result> GetAccountAsync(
        long accountId,
        CancellationToken cancellationToken);

    Task<GetStudentProfileData.Result> GetStudentProfileDataAsync(
        long accountId,
        CancellationToken cancellationToken);

    Task<GetPasswordHash.Result> GetPasswordHashAsync(
        long passwordId,
        CancellationToken cancellationToken);

    Task<GetAllAccounts.Result> GetAllAccountsAsync(
        GetAllAccounts.Request request,
        CancellationToken cancellationToken);

    Task<GetAllStudentProfiles.Result> GetAllStudentProfilesAsync(
        GetAllStudentProfiles.Request request,
        CancellationToken cancellationToken);

    Task<GetFollowers.Result> GetFollowersAsync(
        GetFollowers.Request request,
        CancellationToken cancellationToken);

    Task<UpdateAccount.Result> UpdateAccountAsync(
        UpdateAccount.Request request,
        CancellationToken cancellationToken);

    Task<UpdateStudentProfile.Result> UpdateStudentProfileAsync(
        UpdateStudentProfile.Request request,
        CancellationToken cancellationToken);
}
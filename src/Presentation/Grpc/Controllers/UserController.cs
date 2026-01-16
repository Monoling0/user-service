using Accounts.UserService.Contracts;
using Grpc.Core;
using Grpc.Extensions;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using UserService.Application.Contracts;
using UserService.Application.Contracts.Operations;
using GrpcUserService = Accounts.UserService.Contracts.UserService;

namespace Grpc.Controllers;

public class UserController : GrpcUserService.UserServiceBase
{
    private readonly IAccountService _accountService;

    public UserController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public override async Task<RegisterStudentResponse> RegisterStudent(
        RegisterStudentRequest request,
        ServerCallContext context)
    {
        var registerStudentServerRequest = new RegisterStudent.Request(
            request.PasswordHash,
            request.Email,
            request.Nickname,
            request.ProfilePhotoUrl);

        RegisterStudent.Result registerStudentServerResult = await _accountService.RegisterStudentAsync(
            registerStudentServerRequest,
            context.CancellationToken);

        return registerStudentServerResult switch
        {
            RegisterStudent.Result.Success success => new RegisterStudentResponse
            {
                AccountId = success.AccountId,
            },

            RegisterStudent.Result.NicknameAlreadyExists _ => throw new RpcException(new Status(
                StatusCode.AlreadyExists,
                $"Nickname {request.Nickname} already exists")),

            RegisterStudent.Result.EmailAlreadyExists _ => throw new RpcException(new Status(
                StatusCode.AlreadyExists,
                $"Email {request.Email} already exists")),

            _ => throw new UnreachableException(),
        };
    }

    public override async Task<AddCreatorResponse> AddCreator(
        AddCreatorRequest request,
        ServerCallContext context)
    {
        var addCreatorServerRequest = new AddCreator.Request(
            request.PasswordHash,
            request.Email);

        AddCreator.Result addCreatorServerResult = await _accountService.AddCreatorAsync(
            addCreatorServerRequest,
            context.CancellationToken);

        return addCreatorServerResult switch
        {
            AddCreator.Result.Success success => new AddCreatorResponse
            {
                AccountId = success.AccountId,
            },

            AddCreator.Result.EmailAlreadyExists _ => throw new RpcException(new Status(
                StatusCode.AlreadyExists,
                $"Email {request.Email} already exists")),

            _ => throw new UnreachableException(),
        };
    }

    public override async Task<CreateSubscriptionResponse> CreateSubscription(
        CreateSubscriptionRequest request,
        ServerCallContext context)
    {
        var createSubscriptionServerRequest = new CreateSubscription.Request(
            request.FollowerId,
            request.FolloweeId);

        CreateSubscription.Result createSubscriptionServerResult = await _accountService.CreateSubscriptionAsync(
            createSubscriptionServerRequest,
            context.CancellationToken);

        return createSubscriptionServerResult switch
        {
            CreateSubscription.Result.Success _ => new CreateSubscriptionResponse(),

            CreateSubscription.Result.FollowerNotFound _ => throw new RpcException(new Status(
                StatusCode.NotFound,
                $"Follower {request.FollowerId} doesn't exist")),

            CreateSubscription.Result.FolloweeNotFound _ => throw new RpcException(new Status(
                StatusCode.NotFound,
                $"Followee {request.FolloweeId} doesn't exist")),

            CreateSubscription.Result.AlreadySubscribed _ => throw new RpcException(new Status(
                StatusCode.AlreadyExists,
                $"{request.FollowerId} is already subscribed to {request.FolloweeId}")),

            CreateSubscription.Result.IdenticalIds _ => throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Cannot subscribe to oneself")),

            _ => throw new UnreachableException(),
        };
    }

    public override async Task<ExistsAccountResponse> ExistsAccount(
        ExistsAccountRequest request,
        ServerCallContext context)
    {
        bool exists = await _accountService.ExistsAccountAsync(
            request.AccountId,
            context.CancellationToken);

        return new ExistsAccountResponse
        {
            Exists = exists,
        };
    }

    public override async Task<GetAccountResponse> GetAccount(
        GetAccountRequest request,
        ServerCallContext context)
    {
        GetAccount.Result getAccountServerResult = await _accountService.GetAccountAsync(
            request.AccountId,
            context.CancellationToken);

        return getAccountServerResult switch
        {
            GetAccount.Result.Success success => new GetAccountResponse
            {
                Account = success.Account.ToProto(),
            },

            GetAccount.Result.AccountNotFound _ => throw new RpcException(new Status(
                StatusCode.NotFound,
                $"Account with ID {request.AccountId} doesn't exist")),

            _ => throw new UnreachableException(),
        };
    }

    public override async Task<GetStudentProfileDataResponse> GetStudentProfileData(
        GetStudentProfileDataRequest request,
        ServerCallContext context)
    {
        GetStudentProfileData.Result getStudentProfileServerResult = await _accountService.GetStudentProfileDataAsync(
            request.AccountId,
            context.CancellationToken);

        return getStudentProfileServerResult switch
        {
            GetStudentProfileData.Result.Success success => new GetStudentProfileDataResponse
            {
                StudentProfile = success.StudentProfile.ToProto(),
            },

            GetStudentProfileData.Result.StudentProfileNotFound _ => throw new RpcException(new Status(
                StatusCode.NotFound,
                $"Student profile with ID {request.AccountId} doesn't exist")),

            _ => throw new UnreachableException(),
        };
    }

    public override async Task<GetPasswordHashResponse> GetPasswordHash(
        GetPasswordHashRequest request,
        ServerCallContext context)
    {
        GetPasswordHash.Result getPasswordHashServerResult = await _accountService.GetPasswordHashAsync(
            request.PasswordId,
            context.CancellationToken);

        return getPasswordHashServerResult switch
        {
            GetPasswordHash.Result.Success success => new GetPasswordHashResponse
            {
                PasswordHash = success.PasswordHash,
            },

            GetPasswordHash.Result.PasswordNotFound _ => throw new RpcException(new Status(
                StatusCode.NotFound,
                $"Password with given ID doesn't exist")),

            _ => throw new UnreachableException(),
        };
    }

    public override async Task<GetAllAccountsResponse> GetAllAccounts(
        GetAllAccountsRequest request,
        ServerCallContext context)
    {
        var getAllAccountsServerRequest = new GetAllAccounts.Request(
            request.PageSize,
            request.Ids.HasValue ? request.Ids.Ids_.ToArray() : null,
            request.HasRole ? request.Role.ToApplication() : null,
            request.PageToken?.ToApplication());

        GetAllAccounts.Result getAllAccountsServerResult = await _accountService.GetAllAccountsAsync(
            getAllAccountsServerRequest,
            context.CancellationToken);

        return getAllAccountsServerResult switch
        {
            GetAllAccounts.Result.Success success => new GetAllAccountsResponse
            {
                Accounts = { success.Accounts.Select(a => a.ToProto()) },
                PageToken = success.PageToken?.ToProto(),
            },

            GetAllAccounts.Result.InvalidPageSize _ => throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                $"Invalid page size")),

            _ => throw new UnreachableException(),
        };
    }

    public override async Task<GetAllStudentProfilesResponse> GetAllStudentProfiles(
        GetAllStudentProfilesRequest request,
        ServerCallContext context)
    {
        var getAllStudentProfilesServerRequest = new GetAllStudentProfiles.Request(
            request.PageSize,
            request.Ids.HasValue ? request.Ids.Ids_.ToArray() : null,
            request.PageToken?.ToApplication());

        GetAllStudentProfiles.Result getAllStudentProfilesServerResult =
            await _accountService.GetAllStudentProfilesAsync(
                getAllStudentProfilesServerRequest,
                context.CancellationToken);

        return getAllStudentProfilesServerResult switch
        {
            GetAllStudentProfiles.Result.Success success => new GetAllStudentProfilesResponse
            {
                StudentProfiles = { success.StudentProfiles.Select(sp => sp.ToProto()) },
                PageToken = success.PageToken?.ToProto(),
            },

            GetAllStudentProfiles.Result.InvalidPageSize _ => throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                $"Invalid page size")),

            _ => throw new UnreachableException(),
        };
    }

    public override async Task<GetFollowersResponse> GetFollowers(
        GetFollowersRequest request,
        ServerCallContext context)
    {
        var getFollowersServerRequest = new GetFollowers.Request(
            request.StudentId,
            request.PageSize,
            request.PageToken?.ToApplication());

        GetFollowers.Result getFollowersServerResult = await _accountService.GetFollowersAsync(
            getFollowersServerRequest,
            context.CancellationToken);

        return getFollowersServerResult switch
        {
            GetFollowers.Result.Success success => new GetFollowersResponse
            {
                StudentProfiles = { success.StudentProfiles.Select(sp => sp.ToProto()) },
                PageToken = success.PageToken?.ToProto(),
            },

            GetFollowers.Result.InvalidPageSize _ => throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                $"Invalid page size")),

            _ => throw new UnreachableException(),
        };
    }

    public override async Task<UpdateAccountResponse> UpdateAccount(
        UpdateAccountRequest request,
        ServerCallContext context)
    {
        var updateAccountServerRequest = new UpdateAccount.Request(
            request.AccountId,
            request.PasswordHash ?? default(Optional<string>),
            request.Email ?? default(Optional<string>));

        UpdateAccount.Result updateAccountServerResult = await _accountService.UpdateAccountAsync(
            updateAccountServerRequest,
            context.CancellationToken);

        return updateAccountServerResult switch
        {
            UpdateAccount.Result.Success success => new UpdateAccountResponse(),

            UpdateAccount.Result.AccountNotFound _ => throw new RpcException(new Status(
                StatusCode.NotFound,
                $"Account with ID {request.AccountId} not found")),

            UpdateAccount.Result.NoChanges _ => throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "No changes are offered")),

            UpdateAccount.Result.EmailAlreadyExists _ => throw new RpcException(new Status(
                StatusCode.AlreadyExists,
                $"Email already exists")),

            _ => throw new UnreachableException(),
        };
    }

    public override async Task<UpdateStudentProfileResponse> UpdateStudentProfile(
        UpdateStudentProfileRequest request,
        ServerCallContext context)
    {
        var updateStudentProfileServerRequest = new UpdateStudentProfile.Request(
            request.AccountId,
            request.Nickname ?? default(Optional<string>),
            request.ProfilePhotoUrl.HasValue ? request.ProfilePhotoUrl.ProfilePhotoUrl_ : default(Optional<string?>));

        UpdateStudentProfile.Result updateStudentProfileServerResult = await _accountService.UpdateStudentProfileAsync(
            updateStudentProfileServerRequest,
            context.CancellationToken);

        return updateStudentProfileServerResult switch
        {
            UpdateStudentProfile.Result.Success success => new UpdateStudentProfileResponse(),

            UpdateStudentProfile.Result.StudentNotFound _ => throw new RpcException(new Status(
                StatusCode.NotFound,
                $"Student with account ID {request.AccountId} not found")),

            UpdateStudentProfile.Result.NoChanges _ => throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "No changes are offered")),

            UpdateStudentProfile.Result.NicknameAlreadyExists _ => throw new RpcException(new Status(
                StatusCode.AlreadyExists,
                $"Nickname already exists")),

            _ => throw new UnreachableException(),
        };
    }
}
using Accounts.UserService.Contracts;
using Google.Protobuf.WellKnownTypes;
using UserService.Application.Models.Accounts;
using Account = UserService.Application.Models.Accounts.Account;
using GrpcAccount = Accounts.UserService.Contracts.Account;
using GrpcPageToken = Accounts.UserService.Contracts.PageToken;
using GrpcStudentProfile = Accounts.UserService.Contracts.StudentProfile;
using PageToken = UserService.Application.Models.Common.PageToken;
using StudentProfile = UserService.Application.Models.StudentProfiles.StudentProfile;

namespace Grpc.Extensions;

public static class MappingExtensions
{
    public static GrpcAccount ToProto(this Account account)
    {
        return new GrpcAccount
        {
            AccountId = account.AccountId,
            Role = account.Role.ToProto(),
            PasswordId = account.PasswordId,
            Email = account.Email,
            AccountCreatedAt = account.AccountCreatedAt.ToTimestamp(),
            AccountUpdatedAt = account.AccountUpdatedAt.ToTimestamp(),
        };
    }

    public static Role ToProto(this Roles role)
    {
        return role switch
        {
            Roles.Admin => Role.Admin,
            Roles.Creator => Role.Creator,
            Roles.Student => Role.Student,
            _ => Role.Unspecified,
        };
    }

    public static Roles ToApplication(this Role role)
    {
        return role switch
        {
            Role.Admin => Roles.Admin,
            Role.Creator => Roles.Creator,
            Role.Student => Roles.Student,
            Role.Unspecified or _ => throw new ArgumentOutOfRangeException(nameof(role), role, null),
        };
    }

    public static GrpcPageToken ToProto(this PageToken token)
    {
        return new GrpcPageToken
        {
            LastSeenId = token.LastSeenId,
        };
    }

    public static PageToken ToApplication(this GrpcPageToken token)
    {
        return new PageToken(token.LastSeenId);
    }

    public static GrpcStudentProfile ToProto(this StudentProfile studentProfile)
    {
        return new GrpcStudentProfile
        {
            AccountId = studentProfile.AccountId,
            Nickname = studentProfile.Nickname,
            ProfilePhotoUrl = studentProfile.ProfilePhotoUrl,
        };
    }
}
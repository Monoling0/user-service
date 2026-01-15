using UserService.Application.Models.Accounts;

namespace UserService.Application.Abstractions.Persistence.Requests;

public record GetAllAccountsRepositoryRequest(
    int PageSize,
    Roles? RoleCode,
    long? LastSeenId);
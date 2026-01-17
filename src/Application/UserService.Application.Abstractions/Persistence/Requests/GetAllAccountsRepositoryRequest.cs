using UserService.Application.Models.Accounts;

namespace UserService.Application.Abstractions.Persistence.Requests;

public record GetAllAccountsRepositoryRequest(
    int PageSize,
    long[]? Ids,
    Roles? Role,
    long? LastSeenId);
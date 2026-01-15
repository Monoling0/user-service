using UserService.Application.Models.Accounts;

namespace UserService.Application.Abstractions.Persistence.Requests;

public record CreateAccountRepositoryRequest(
    Roles Role,
    long PasswordId,
    string Email,
    DateTimeOffset AccountCreatedAt);
namespace UserService.Application.Abstractions.Persistence.Requests;

public record CreateAccountRepositoryRequest(
    long RoleId,
    long PasswordId,
    string Email,
    DateTimeOffset AccountCreatedAt);
namespace UserService.Application.Models.Accounts;

public record Account(
    long AccountId,
    Roles Role,
    long PasswordId,
    string Email,
    DateTimeOffset AccountCreatedAt,
    DateTimeOffset AccountUpdatedAt);
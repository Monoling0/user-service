namespace UserService.Application.Models.Accounts;

public record AccountPassword(
    long PasswordId,
    string PasswordHash);
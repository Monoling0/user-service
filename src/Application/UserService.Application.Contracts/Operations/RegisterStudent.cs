namespace UserService.Application.Contracts.Operations;

public class RegisterStudent
{
    public record Request(
        string PasswordHash,
        string Email,
        string Nickname,
        string? ProfilePhotoUrl);

    public abstract record Result
    {
        private Result() { }

        public sealed record Success(long AccountId) : Result;

        public sealed record NicknameAlreadyExists : Result;

        public sealed record EmailAlreadyExists : Result;
    }
}
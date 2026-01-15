namespace UserService.Application.Contracts.Operations;

public class AddCreator
{
    public record Request(
        string PasswordHash,
        string Email);

    public abstract record Result
    {
        private Result() { }

        public sealed record Success : Result;

        public sealed record EmailAlreadyExists : Result;
    }
}
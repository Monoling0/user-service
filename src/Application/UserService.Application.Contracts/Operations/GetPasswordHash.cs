namespace UserService.Application.Contracts.Operations;

public class GetPasswordHash
{
    public abstract record Result
    {
        private Result() { }

        public sealed record Success(string PasswordHash) : Result;

        public sealed record PasswordNotFound : Result;
    }
}
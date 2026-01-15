using Microsoft.CodeAnalysis;

namespace UserService.Application.Contracts.Operations;

public class UpdateAccount
{
    public record Request(
        long AccountId,
        Optional<string> PasswordHash,
        Optional<string> Email);

    public abstract record Result
    {
        private Result() { }

        public sealed record Success : Result;

        public sealed record AccountNotFound : Result;

        public sealed record NoChanges : Result;

        public sealed record EmailAlreadyExists : Result;
    }
}
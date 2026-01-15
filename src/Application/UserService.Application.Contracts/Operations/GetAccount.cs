using UserService.Application.Models.Accounts;

namespace UserService.Application.Contracts.Operations;

public static class GetAccount
{
    public abstract record Result
    {
        private Result() { }

        public sealed record Success(Account Account) : Result;

        public sealed record AccountNotFound : Result;
    }
}
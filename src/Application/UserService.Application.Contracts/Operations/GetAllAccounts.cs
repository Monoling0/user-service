using UserService.Application.Models.Accounts;
using UserService.Application.Models.Common;

namespace UserService.Application.Contracts.Operations;

public class GetAllAccounts
{
    public record Request(
        int PageSize,
        long[]? Ids,
        Roles? Role,
        PageToken? PageToken);

    public abstract record Result
    {
        private Result() { }

        public sealed record Success(
            IList<Account> Accounts,
            PageToken? PageToken) : Result;

        public sealed record InvalidPageSize : Result;
    }
}
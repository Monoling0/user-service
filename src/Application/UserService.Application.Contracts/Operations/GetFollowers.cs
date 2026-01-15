using UserService.Application.Models.Common;

namespace UserService.Application.Contracts.Operations;

public class GetFollowers
{
    public record Request(
        long StudentId,
        int PageSize,
        PageToken? PageToken);

    public abstract record Result
    {
        private Result() { }

        public sealed record Success(
            IList<long> StudentProfiles,
            PageToken? PageToken) : Result;

        public sealed record InvalidPageSize : Result;
    }
}
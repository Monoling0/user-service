using UserService.Application.Models.Common;
using UserService.Application.Models.StudentProfiles;

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
            IList<StudentProfile> StudentProfiles,
            PageToken? PageToken) : Result;

        public sealed record InvalidPageSize : Result;

        public sealed record AccountNotFound : Result;
    }
}
using UserService.Application.Models.Common;
using UserService.Application.Models.StudentProfiles;

namespace UserService.Application.Contracts.Operations;

public class GetAllStudentProfiles
{
    public record Request(
        int PageSize,
        long[]? Ids,
        PageToken? PageToken);

    public abstract record Result
    {
        private Result() { }

        public sealed record Success(
            IList<StudentProfile> StudentProfiles,
            PageToken? PageToken) : Result;

        public sealed record InvalidPageSize : Result;
    }
}
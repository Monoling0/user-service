using UserService.Application.Models.StudentProfiles;

namespace UserService.Application.Contracts.Operations;

public class GetStudentProfileData
{
    public abstract record Result
    {
        private Result() { }

        public sealed record Success(StudentProfile StudentProfile) : Result;

        public sealed record StudentProfileNotFound : Result;
    }
}
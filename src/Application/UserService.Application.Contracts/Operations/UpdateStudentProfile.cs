using Microsoft.CodeAnalysis;

namespace UserService.Application.Contracts.Operations;

public class UpdateStudentProfile
{
    public record Request(
        long AccountId,
        Optional<string> Nickname,
        Optional<string?> ProfilePhotoUrl);

    public abstract record Result
    {
        private Result() { }

        public sealed record Success : Result;

        public sealed record StudentNotFound : Result;

        public sealed record NoChanges : Result;

        public sealed record NicknameAlreadyExists : Result;
    }
}
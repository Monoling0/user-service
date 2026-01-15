namespace UserService.Application.Contracts.Operations;

public class CreateSubscription
{
    public record Request(
        long FollowerId,
        long FolloweeId);

    public abstract record Result
    {
        private Result() { }

        public sealed record Success : Result;

        public sealed record FollowerNotFound : Result;

        public sealed record FolloweeNotFound : Result;

        public sealed record AlreadySubscribed : Result;

        public sealed record IdenticalIds : Result;
    }
}
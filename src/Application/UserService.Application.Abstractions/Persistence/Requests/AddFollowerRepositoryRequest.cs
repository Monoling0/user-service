namespace UserService.Application.Abstractions.Persistence.Requests;

public record AddFollowerRepositoryRequest(
    long FollowerId,
    long FolloweeId,
    DateTimeOffset CreatedAt);
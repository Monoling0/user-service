namespace UserService.Application.Abstractions.Persistence.Requests;

public record GetAllFollowersRepositoryRequest(
    long FolloweeId,
    int PageSize,
    long? LastSeenId);
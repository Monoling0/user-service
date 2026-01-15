namespace UserService.Application.Abstractions.Persistence.Requests;

public record GetAllStudentProfilesRepositoryRequest(
    int PageSize,
    long? LastSeenId);
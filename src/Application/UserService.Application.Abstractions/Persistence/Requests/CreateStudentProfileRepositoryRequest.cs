namespace UserService.Application.Abstractions.Persistence.Requests;

public record CreateStudentProfileRepositoryRequest(
    long AccountId,
    string Nickname,
    string? ProfilePhotoUrl);
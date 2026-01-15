namespace UserService.Application.Models.StudentProfiles;

public record StudentProfile(
    long AccountId,
    string Nickname,
    string? ProfilePhotoUrl);
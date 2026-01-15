using Microsoft.CodeAnalysis;

namespace UserService.Application.Abstractions.Persistence.Requests;

public record UpdateStudentProfileRepositoryRequest(
    long AccountId,
    Optional<string> Nickname,
    Optional<string?> ProfilePhotoUrl);
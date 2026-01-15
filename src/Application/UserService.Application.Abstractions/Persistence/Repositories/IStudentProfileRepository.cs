using UserService.Application.Abstractions.Persistence.Requests;
using UserService.Application.Models.StudentProfiles;

namespace UserService.Application.Abstractions.Persistence.Repositories;

public interface IStudentProfileRepository
{
    Task CreateAsync(
        CreateStudentProfileRepositoryRequest request,
        CancellationToken cancellationToken);

    Task<StudentProfile?> GetStudentProfileAsync(
        long accountId,
        CancellationToken cancellationToken);

    IAsyncEnumerable<StudentProfile> GetAllStudentProfilesAsync(
        GetAllStudentProfilesRepositoryRequest request,
        CancellationToken cancellationToken);

    Task<bool> ExistsNicknameAsync(
        string nickname,
        CancellationToken cancellationToken);

    Task<bool> UpdateStudentProfile(
        UpdateStudentProfileRepositoryRequest request,
        CancellationToken cancellationToken);
}
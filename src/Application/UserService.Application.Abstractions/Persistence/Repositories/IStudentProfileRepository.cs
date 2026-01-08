using UserService.Application.Abstractions.Persistence.Requests;

namespace UserService.Application.Abstractions.Persistence.Repositories;

public interface IStudentProfileRepository
{
    Task CreateAsync(
        CreateStudentProfileRepositoryRequest request,
        CancellationToken cancellationToken = default);
}
using UserService.Application.Abstractions.Persistence.Repositories;
using UserService.Application.Abstractions.Persistence.Requests;

namespace Infrastructure.Persistence.Repositories.Postgres;

public class StudentProfileRepository : IStudentProfileRepository
{
    public Task CreateAsync(CreateStudentProfileRepositoryRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
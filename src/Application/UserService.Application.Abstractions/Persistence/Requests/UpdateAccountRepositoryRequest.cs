using Microsoft.CodeAnalysis;

namespace UserService.Application.Abstractions.Persistence.Requests;

public record UpdateAccountRepositoryRequest(
    long AccountId,
    Optional<string> Email);
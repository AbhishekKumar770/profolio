using Profolio.Domain.ValueObjects;

namespace Profolio.Application.Abstractions.Persistence;

public sealed record PublicPortfolioSnapshot(
    Guid TenantId,
    string Username,
    int VersionNumber,
    string SnapshotJson,
    int SnapshotSchemaVersion,
    DateTime PublishedAtUtc);

public interface IPublicPortfolioReader
{
    Task<PublicPortfolioSnapshot?> GetPublishedByUsernameAsync(
        Username username,
        CancellationToken cancellationToken = default);
}

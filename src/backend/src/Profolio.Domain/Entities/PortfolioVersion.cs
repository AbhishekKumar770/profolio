namespace Profolio.Domain.Entities;

using Profolio.Domain.Common;

public sealed class PortfolioVersion : BaseEntity, ITenantEntity
{
    /// <summary>
    /// Maximum allowed snapshot JSON length in characters (500,000 characters).
    /// Application-layer UTF-8 byte-size constraints and payload schema validation are enforced during publication.
    /// </summary>
    public const int MaxSnapshotJsonLength = 500_000;

    public Guid TenantId { get; private set; }

    public int VersionNumber { get; private set; }

    public string SnapshotJson { get; private set; } = string.Empty;

    public int SnapshotSchemaVersion { get; private set; }

    private PortfolioVersion()
    {
        // Required by EF Core for entity materialization.
    }

    public PortfolioVersion(
        Guid tenantId,
        int versionNumber,
        string snapshotJson,
        int snapshotSchemaVersion = 1)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        }

        if (versionNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(versionNumber), versionNumber, "Version number must be positive.");
        }

        if (snapshotSchemaVersion <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(snapshotSchemaVersion), snapshotSchemaVersion, "Snapshot schema version must be positive.");
        }

        ArgumentNullException.ThrowIfNull(snapshotJson);

        if (string.IsNullOrWhiteSpace(snapshotJson))
        {
            throw new ArgumentException("Snapshot JSON cannot be empty or whitespace.", nameof(snapshotJson));
        }

        if (snapshotJson.Length > MaxSnapshotJsonLength)
        {
            throw new ArgumentException($"Snapshot JSON exceeds maximum allowed length of {MaxSnapshotJsonLength} characters.", nameof(snapshotJson));
        }

        TenantId = tenantId;
        VersionNumber = versionNumber;
        SnapshotJson = snapshotJson;
        SnapshotSchemaVersion = snapshotSchemaVersion;
    }
}

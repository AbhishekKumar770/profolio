namespace Profolio.Application.Features.DeveloperProfile.Queries.GetDeveloperProfile;

using System;

public sealed record DeveloperProfileDto(
    Guid Id,
    string FullName,
    string Headline,
    string? Bio,
    string? Location,
    string? PublicContactEmail,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

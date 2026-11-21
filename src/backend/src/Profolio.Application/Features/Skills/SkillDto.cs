namespace Profolio.Application.Features.Skills;

public sealed record SkillDto(Guid Id, string Name, string? Category, int DisplayOrder, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);

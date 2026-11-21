namespace Profolio.Application.Features.Skills.Commands.UpdateSkill;

public sealed record UpdateSkillCommand(Guid SkillId, string Name, string? Category, int DisplayOrder);

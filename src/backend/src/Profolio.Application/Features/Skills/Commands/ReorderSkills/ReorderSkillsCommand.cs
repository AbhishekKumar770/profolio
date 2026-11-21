namespace Profolio.Application.Features.Skills.Commands.ReorderSkills;

public sealed record ReorderSkillsCommand(IReadOnlyList<Guid> OrderedSkillIds);

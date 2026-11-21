namespace Profolio.Application.Features.Skills;

using Profolio.Application.Common.Results;
using Profolio.Domain.Entities;

public static class SkillErrors
{
    public const string UniqueIndexName = "ix_skills_tenant_id_lower_name";

    public static Error NotFound() => Error.NotFound("Skill.NotFound", "Skill was not found for the active tenant.");
    public static Error DuplicateName() => Error.Conflict("Skill.Name.Duplicate", "A skill with this name already exists for the active tenant.");
    public static Error InvalidName(string description) => Error.Validation("Skill.Name.Invalid", description, nameof(Skill.Name));
    public static Error InvalidCategory(string description) => Error.Validation("Skill.Category.Invalid", description, nameof(Skill.Category));
    public static Error InvalidDisplayOrder() => Error.Validation("Skill.DisplayOrder.Invalid", "Display order cannot be negative.", nameof(Skill.DisplayOrder));
    public static Error InvalidReorder(string description) => Error.Validation("Skill.Reorder.Invalid", description, "OrderedSkillIds");
}

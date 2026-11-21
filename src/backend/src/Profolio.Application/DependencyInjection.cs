namespace Profolio.Application;

using System;
using Microsoft.Extensions.DependencyInjection;
using Profolio.Application.Common.Handlers;
using Profolio.Application.Features.DeveloperProfile.Commands.ClearPublicContactEmail;
using Profolio.Application.Features.DeveloperProfile.Commands.SetPublicContactEmail;
using Profolio.Application.Features.DeveloperProfile.Commands.UpdateDeveloperProfile;
using Profolio.Application.Features.DeveloperProfile.Queries.GetDeveloperProfile;
using Profolio.Application.Features.Skills.Commands.AddSkill;
using Profolio.Application.Features.Skills.Commands.DeleteSkill;
using Profolio.Application.Features.Skills.Commands.ReorderSkills;
using Profolio.Application.Features.Skills.Commands.UpdateSkill;
using Profolio.Application.Features.Skills;
using Profolio.Application.Features.Skills.Queries.GetSkills;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IQueryHandler<GetDeveloperProfileQuery, DeveloperProfileDto>, GetDeveloperProfileQueryHandler>();
        services.AddScoped<ICommandHandler<UpdateDeveloperProfileCommand>, UpdateDeveloperProfileCommandHandler>();
        services.AddScoped<ICommandHandler<SetPublicContactEmailCommand>, SetPublicContactEmailCommandHandler>();
        services.AddScoped<ICommandHandler<ClearPublicContactEmailCommand>, ClearPublicContactEmailCommandHandler>();
        services.AddScoped<IQueryHandler<GetSkillsQuery, IReadOnlyList<SkillDto>>, GetSkillsQueryHandler>();
        services.AddScoped<ICommandHandler<AddSkillCommand, Guid>, AddSkillCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateSkillCommand>, UpdateSkillCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteSkillCommand>, DeleteSkillCommandHandler>();
        services.AddScoped<ICommandHandler<ReorderSkillsCommand>, ReorderSkillsCommandHandler>();

        return services;
    }
}

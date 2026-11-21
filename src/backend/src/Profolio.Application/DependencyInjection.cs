namespace Profolio.Application;

using System;
using Microsoft.Extensions.DependencyInjection;
using Profolio.Application.Common.Handlers;
using Profolio.Application.Features.DeveloperProfile.Commands.ClearPublicContactEmail;
using Profolio.Application.Features.DeveloperProfile.Commands.SetPublicContactEmail;
using Profolio.Application.Features.DeveloperProfile.Commands.UpdateDeveloperProfile;
using Profolio.Application.Features.DeveloperProfile.Queries.GetDeveloperProfile;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IQueryHandler<GetDeveloperProfileQuery, DeveloperProfileDto>, GetDeveloperProfileQueryHandler>();
        services.AddScoped<ICommandHandler<UpdateDeveloperProfileCommand>, UpdateDeveloperProfileCommandHandler>();
        services.AddScoped<ICommandHandler<SetPublicContactEmailCommand>, SetPublicContactEmailCommandHandler>();
        services.AddScoped<ICommandHandler<ClearPublicContactEmailCommand>, ClearPublicContactEmailCommandHandler>();

        return services;
    }
}

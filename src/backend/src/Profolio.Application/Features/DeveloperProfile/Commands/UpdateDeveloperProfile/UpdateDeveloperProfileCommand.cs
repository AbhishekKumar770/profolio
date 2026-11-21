namespace Profolio.Application.Features.DeveloperProfile.Commands.UpdateDeveloperProfile;

public sealed record UpdateDeveloperProfileCommand(
    string FullName,
    string Headline,
    string? Bio,
    string? Location);

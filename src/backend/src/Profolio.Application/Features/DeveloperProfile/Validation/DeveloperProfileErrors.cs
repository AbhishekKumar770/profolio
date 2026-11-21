namespace Profolio.Application.Features.DeveloperProfile.Validation;

using Profolio.Application.Common.Results;
using Profolio.Domain.Entities;

public static class DeveloperProfileErrors
{
    public static Error NotFound() =>
        Error.NotFound(
            "DeveloperProfile.NotFound",
            "Developer profile was not found for the active tenant.");

    public static Error EmptyFullName() =>
        Error.Validation(
            "DeveloperProfile.FullName.Empty",
            "Full name is required.",
            nameof(DeveloperProfile.FullName));

    public static Error FullNameTooLong() =>
        Error.Validation(
            "DeveloperProfile.FullName.TooLong",
            $"Full name cannot exceed {DeveloperProfile.MaxFullNameLength} characters.",
            nameof(DeveloperProfile.FullName));

    public static Error EmptyHeadline() =>
        Error.Validation(
            "DeveloperProfile.Headline.Empty",
            "Headline is required.",
            nameof(DeveloperProfile.Headline));

    public static Error HeadlineTooLong() =>
        Error.Validation(
            "DeveloperProfile.Headline.TooLong",
            $"Headline cannot exceed {DeveloperProfile.MaxHeadlineLength} characters.",
            nameof(DeveloperProfile.Headline));

    public static Error LocationTooLong() =>
        Error.Validation(
            "DeveloperProfile.Location.TooLong",
            $"Location cannot exceed {DeveloperProfile.MaxLocationLength} characters.",
            nameof(DeveloperProfile.Location));

    public static Error InvalidEmail(string description) =>
        Error.Validation(
            "DeveloperProfile.Email.Invalid",
            description,
            "Email");
}

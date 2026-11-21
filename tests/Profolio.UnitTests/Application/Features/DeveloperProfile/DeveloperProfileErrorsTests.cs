namespace Profolio.UnitTests.Application.Features.DeveloperProfile;

using Profolio.Application.Common.Results;
using Profolio.Application.Features.DeveloperProfile.Validation;
using Profolio.Domain.Entities;
using Xunit;

public class DeveloperProfileErrorsTests
{
    [Fact]
    public void NotFound_ReturnsExpectedError()
    {
        var error = DeveloperProfileErrors.NotFound();

        Assert.Equal("DeveloperProfile.NotFound", error.Code);
        Assert.Equal(ErrorType.NotFound, error.Type);
        Assert.Null(error.Field);
        Assert.Contains("not found", error.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EmptyFullName_ReturnsExpectedError()
    {
        var error = DeveloperProfileErrors.EmptyFullName();

        Assert.Equal("DeveloperProfile.FullName.Empty", error.Code);
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal(nameof(DeveloperProfile.FullName), error.Field);
        Assert.Equal("Full name is required.", error.Description);
    }

    [Fact]
    public void FullNameTooLong_ReturnsExpectedError()
    {
        var error = DeveloperProfileErrors.FullNameTooLong();

        Assert.Equal("DeveloperProfile.FullName.TooLong", error.Code);
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal(nameof(DeveloperProfile.FullName), error.Field);
        Assert.Equal($"Full name cannot exceed {DeveloperProfile.MaxFullNameLength} characters.", error.Description);
    }

    [Fact]
    public void EmptyHeadline_ReturnsExpectedError()
    {
        var error = DeveloperProfileErrors.EmptyHeadline();

        Assert.Equal("DeveloperProfile.Headline.Empty", error.Code);
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal(nameof(DeveloperProfile.Headline), error.Field);
        Assert.Equal("Headline is required.", error.Description);
    }

    [Fact]
    public void HeadlineTooLong_ReturnsExpectedError()
    {
        var error = DeveloperProfileErrors.HeadlineTooLong();

        Assert.Equal("DeveloperProfile.Headline.TooLong", error.Code);
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal(nameof(DeveloperProfile.Headline), error.Field);
        Assert.Equal($"Headline cannot exceed {DeveloperProfile.MaxHeadlineLength} characters.", error.Description);
    }

    [Fact]
    public void LocationTooLong_ReturnsExpectedError()
    {
        var error = DeveloperProfileErrors.LocationTooLong();

        Assert.Equal("DeveloperProfile.Location.TooLong", error.Code);
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal(nameof(DeveloperProfile.Location), error.Field);
        Assert.Equal($"Location cannot exceed {DeveloperProfile.MaxLocationLength} characters.", error.Description);
    }

    [Fact]
    public void InvalidEmail_ReturnsExpectedError()
    {
        var error = DeveloperProfileErrors.InvalidEmail("Invalid format.");

        Assert.Equal("DeveloperProfile.Email.Invalid", error.Code);
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("Email", error.Field);
        Assert.Equal("Invalid format.", error.Description);
    }
}

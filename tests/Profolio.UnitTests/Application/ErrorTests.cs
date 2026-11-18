using System.Reflection;
using Profolio.Application.Common.Results;

namespace Profolio.UnitTests.Application;

public class ErrorTests
{
    [Fact]
    public void Constructor_WithValidArguments_InitializesProperties()
    {
        // Act
        var error = new Error("Profile.NotFound", "Developer profile was not found.", ErrorType.NotFound);

        // Assert
        Assert.Equal("Profile.NotFound", error.Code);
        Assert.Equal("Developer profile was not found.", error.Description);
        Assert.Equal(ErrorType.NotFound, error.Type);
        Assert.Null(error.Field);
    }

    [Fact]
    public void Constructor_WithSurroundingWhitespace_TrimsCodeDescriptionAndField()
    {
        // Act
        var error = new Error("  Profile.InvalidName  ", "  Full name is required.  ", ErrorType.Validation, "  FullName  ");

        // Assert
        Assert.Equal("Profile.InvalidName", error.Code);
        Assert.Equal("Full name is required.", error.Description);
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("FullName", error.Field);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    public void Constructor_WithNullEmptyOrWhitespaceField_NormalizesFieldToNull(string? field)
    {
        // Act
        var error = new Error("Profile.Invalid", "Invalid input.", ErrorType.Validation, field);

        // Assert
        Assert.Null(error.Field);
    }

    [Fact]
    public void Constructor_WithNullCode_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new Error(null!, "Description", ErrorType.Failure));
        Assert.Equal("code", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    public void Constructor_WithEmptyOrWhitespaceCode_ThrowsArgumentException(string invalidCode)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Error(invalidCode, "Description", ErrorType.Failure));
        Assert.Equal("code", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullDescription_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new Error("Profile.Error", null!, ErrorType.Failure));
        Assert.Equal("description", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    public void Constructor_WithEmptyOrWhitespaceDescription_ThrowsArgumentException(string invalidDescription)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Error("Profile.Error", invalidDescription, ErrorType.Failure));
        Assert.Equal("description", exception.ParamName);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(99)]
    [InlineData(int.MaxValue)]
    public void Constructor_WithUndefinedErrorType_ThrowsArgumentOutOfRangeException(int invalidEnumValue)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Error("Profile.Error", "Invalid type", (ErrorType)invalidEnumValue));
        Assert.Equal("type", exception.ParamName);
    }

    [Fact]
    public void FactoryMethods_ProduceExpectedErrorTypesAndFields()
    {
        // Act
        var validationWithField = Error.Validation("  Profile.FullNameRequired  ", "  Full name is required.  ", "  FullName  ");
        var validationWithoutField = Error.Validation("Profile.Invalid", "Profile input is invalid.");
        var notFound = Error.NotFound("  Profile.NotFound  ", "  Profile not found.  ");
        var conflict = Error.Conflict("  Project.SlugConflict  ", "  Slug already exists.  ");
        var unauthorized = Error.Unauthorized("  Auth.Unauthorized  ", "  Authentication required.  ");
        var forbidden = Error.Forbidden("  Tenant.Forbidden  ", "  Access denied.  ");
        var failure = Error.Failure("  Publishing.Failed  ", "  Unexpected error.  ");

        // Assert
        Assert.Equal(ErrorType.Validation, validationWithField.Type);
        Assert.Equal("Profile.FullNameRequired", validationWithField.Code);
        Assert.Equal("Full name is required.", validationWithField.Description);
        Assert.Equal("FullName", validationWithField.Field);

        Assert.Equal(ErrorType.Validation, validationWithoutField.Type);
        Assert.Equal("Profile.Invalid", validationWithoutField.Code);
        Assert.Equal("Profile input is invalid.", validationWithoutField.Description);
        Assert.Null(validationWithoutField.Field);

        Assert.Equal(ErrorType.NotFound, notFound.Type);
        Assert.Equal("Profile.NotFound", notFound.Code);
        Assert.Equal("Profile not found.", notFound.Description);
        Assert.Null(notFound.Field);

        Assert.Equal(ErrorType.Conflict, conflict.Type);
        Assert.Equal("Project.SlugConflict", conflict.Code);
        Assert.Equal("Slug already exists.", conflict.Description);
        Assert.Null(conflict.Field);

        Assert.Equal(ErrorType.Unauthorized, unauthorized.Type);
        Assert.Equal("Auth.Unauthorized", unauthorized.Code);
        Assert.Equal("Authentication required.", unauthorized.Description);
        Assert.Null(unauthorized.Field);

        Assert.Equal(ErrorType.Forbidden, forbidden.Type);
        Assert.Equal("Tenant.Forbidden", forbidden.Code);
        Assert.Equal("Access denied.", forbidden.Description);
        Assert.Null(forbidden.Field);

        Assert.Equal(ErrorType.Failure, failure.Type);
        Assert.Equal("Publishing.Failed", failure.Code);
        Assert.Equal("Unexpected error.", failure.Description);
        Assert.Null(failure.Field);
    }

    [Fact]
    public void ValueEquality_ComparesCodeDescriptionTypeAndField()
    {
        // Arrange
        var error1 = Error.Validation("Profile.Invalid", "Invalid name.", "FullName");
        var error2 = Error.Validation("  Profile.Invalid  ", "  Invalid name.  ", "  FullName  ");
        var differentField = Error.Validation("Profile.Invalid", "Invalid name.", "Headline");
        var differentType = Error.Conflict("Profile.Invalid", "Invalid name.");

        // Assert
        Assert.Equal(error1, error2);
        Assert.Equal(error1.GetHashCode(), error2.GetHashCode());
        Assert.NotEqual(error1, differentField);
        Assert.NotEqual(error1, differentType);
    }

    [Fact]
    public void Immutability_HasNoPublicPropertySetters()
    {
        // Arrange
        var properties = typeof(Error).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Assert
        foreach (var property in properties)
        {
            Assert.False(property.SetMethod?.IsPublic ?? false, $"Property {property.Name} has a public setter.");
        }
    }
}

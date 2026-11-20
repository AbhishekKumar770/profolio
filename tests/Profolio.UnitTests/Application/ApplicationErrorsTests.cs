namespace Profolio.UnitTests.Application;

using System;
using Profolio.Application.Common.Results;
using Profolio.Application.Common.Validation;
using Xunit;

public class ApplicationErrorsTests
{
    [Fact]
    public void ConcurrencyConflict_ReturnsExpectedErrorTypeAndCode()
    {
        // Act
        var error = ApplicationErrors.Concurrency.Conflict();

        // Assert
        Assert.Equal("Concurrency.Conflict", error.Code);
        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Contains("modified by another operation", error.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TenancyUnauthenticated_ReturnsExpectedErrorTypeAndCode()
    {
        // Act
        var error = ApplicationErrors.Tenancy.Unauthenticated();

        // Assert
        Assert.Equal("Auth.Unauthenticated", error.Code);
        Assert.Equal(ErrorType.Unauthorized, error.Type);
        Assert.Contains("User is not authenticated", error.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TenancyNotResolved_ReturnsExpectedErrorTypeAndCode()
    {
        // Act
        var error = ApplicationErrors.Tenancy.NotResolved();

        // Assert
        Assert.Equal("Tenant.NotResolved", error.Code);
        Assert.Equal(ErrorType.Forbidden, error.Type);
        Assert.Contains("No active tenant is resolved", error.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TenancyAccessDenied_ReturnsExpectedErrorTypeAndCode()
    {
        // Act
        var error = ApplicationErrors.Tenancy.AccessDenied();

        // Assert
        Assert.Equal("Tenant.AccessDenied", error.Code);
        Assert.Equal(ErrorType.Forbidden, error.Type);
        Assert.Contains("Access to this tenant's resource is prohibited", error.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GeneralInvalidInput_ReturnsExpectedErrorTypeAndField()
    {
        // Act
        var error = ApplicationErrors.General.InvalidInput("Payload cannot be empty.", "Payload");

        // Assert
        Assert.Equal("General.InvalidInput", error.Code);
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("Payload cannot be empty.", error.Description);
        Assert.Equal("Payload", error.Field);
    }
}

namespace Profolio.Application.Common.Validation;

using Profolio.Application.Common.Results;

public static class ApplicationErrors
{
    public static class Concurrency
    {
        public static Error Conflict(string description = "The record was modified by another operation. Please reload and try again.") =>
            Error.Conflict("Concurrency.Conflict", description);
    }

    public static class Tenancy
    {
        public static Error Unauthenticated(string description = "User is not authenticated.") =>
            Error.Unauthorized("Auth.Unauthenticated", description);

        public static Error NotResolved(string description = "No active tenant is resolved in the current context.") =>
            Error.Forbidden("Tenant.NotResolved", description);

        public static Error AccessDenied(string description = "Access to this tenant's resource is prohibited.") =>
            Error.Forbidden("Tenant.AccessDenied", description);
    }

    public static class General
    {
        public static Error InvalidInput(string description, string? field = null) =>
            Error.Validation("General.InvalidInput", description, field);
    }
}

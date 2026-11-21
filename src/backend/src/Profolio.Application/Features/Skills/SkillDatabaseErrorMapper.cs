namespace Profolio.Application.Features.Skills;

using Microsoft.EntityFrameworkCore;
using Npgsql;

internal static class SkillDatabaseErrorMapper
{
    internal static bool IsDuplicateSkillNameViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException postgresException &&
        postgresException.SqlState == PostgresErrorCodes.UniqueViolation &&
        string.Equals(postgresException.ConstraintName, SkillErrors.UniqueIndexName, StringComparison.Ordinal);
}

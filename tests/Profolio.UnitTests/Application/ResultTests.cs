using System.Reflection;
using Profolio.Application.Common.Results;

namespace Profolio.UnitTests.Application;

public class ResultTests
{
    [Fact]
    public void Success_NonGeneric_SetsSuccessStateWithEmptyErrors()
    {
        // Act
        var result = Result.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Error);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failure_NonGenericWithSingleError_SetsFailureStateAndExposesError()
    {
        // Arrange
        var error = Error.NotFound("Profile.NotFound", "Profile was not found.");

        // Act
        var result = Result.Failure(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        Assert.Single(result.Errors);
        Assert.Equal(error, result.Errors[0]);
    }

    [Fact]
    public void Failure_NonGenericWithMultipleErrors_PreservesAllErrorsAndSetsPrimaryError()
    {
        // Arrange
        var error1 = Error.Validation("Profile.FullNameRequired", "Full name is required.", "FullName");
        var error2 = Error.Validation("Profile.HeadlineTooLong", "Headline is too long.", "Headline");

        // Act
        var result = Result.Failure([error1, error2]);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error1, result.Error);
        Assert.Equal(2, result.Errors.Count);
        Assert.Equal(error1, result.Errors[0]);
        Assert.Equal(error2, result.Errors[1]);
    }

    [Fact]
    public void Success_GenericWithReferenceType_SetsValueAndEmptyErrors()
    {
        // Act
        var result = Result.Success("developer-profile");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal("developer-profile", result.Value);
        Assert.Null(result.Error);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Success_GenericWithValueType_SetsValueIncludingDefaultValueType()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var guidResult = Result.Success(id);
        var zeroResult = Result.Success(0);

        // Assert
        Assert.True(guidResult.IsSuccess);
        Assert.Equal(id, guidResult.Value);
        Assert.True(zeroResult.IsSuccess);
        Assert.Equal(0, zeroResult.Value);
    }

    [Fact]
    public void Success_GenericWithNullValue_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => Result.Success<string>(null!));
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Success_GenericWithNullableValueTypeNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => Result.Success<int?>(null));
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Success_GenericWithNullableValueTypeNonNull_SucceedsAndReturnsValue()
    {
        // Act
        var result = Result.Success<int?>(42);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
        Assert.Null(result.Error);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failure_GenericWithSingleError_ThrowsInvalidOperationExceptionWhenAccessingValue()
    {
        // Arrange
        var error = Error.Conflict("Project.SlugConflict", "Project slug already exists.");

        // Act
        var result = Result.Failure<Guid>(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        Assert.Single(result.Errors);
        Assert.Equal(error, result.Errors[0]);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void Failure_GenericWithMultipleErrors_ExposesErrorsAndRejectsValueAccess()
    {
        // Arrange
        var error1 = Error.Validation("Project.TitleRequired", "Title is required.", "Title");
        var error2 = Error.Validation("Project.SlugRequired", "Slug is required.", "Slug");

        // Act
        var result = Result.Failure<string>([error1, error2]);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error1, result.Error);
        Assert.Equal(2, result.Errors.Count);
        Assert.Equal(error1, result.Errors[0]);
        Assert.Equal(error2, result.Errors[1]);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void Failure_WithNullError_ThrowsArgumentNullException()
    {
        // Act & Assert
        var nonGenericEx = Assert.Throws<ArgumentNullException>(() => Result.Failure((Error)null!));
        Assert.Equal("error", nonGenericEx.ParamName);

        var genericEx = Assert.Throws<ArgumentNullException>(() => Result.Failure<string>((Error)null!));
        Assert.Equal("error", genericEx.ParamName);
    }

    [Fact]
    public void Failure_WithNullErrorCollection_ThrowsArgumentNullException()
    {
        // Act & Assert
        var nonGenericEx = Assert.Throws<ArgumentNullException>(() => Result.Failure((IEnumerable<Error>)null!));
        Assert.Equal("errors", nonGenericEx.ParamName);

        var genericEx = Assert.Throws<ArgumentNullException>(() => Result.Failure<string>((IEnumerable<Error>)null!));
        Assert.Equal("errors", genericEx.ParamName);
    }

    [Fact]
    public void Failure_WithEmptyErrorCollection_ThrowsArgumentException()
    {
        // Act & Assert
        var nonGenericEx = Assert.Throws<ArgumentException>(() => Result.Failure(Array.Empty<Error>()));
        Assert.Equal("errors", nonGenericEx.ParamName);

        var genericEx = Assert.Throws<ArgumentException>(() => Result.Failure<string>(Array.Empty<Error>()));
        Assert.Equal("errors", genericEx.ParamName);
    }

    [Fact]
    public void Failure_WithNullElementInErrorCollection_ThrowsArgumentException()
    {
        // Arrange
        var validError = Error.Validation("Skill.NameRequired", "Skill name is required.", "Name");
        Error[] errorsWithNull = [validError, null!];

        // Act & Assert
        var nonGenericEx = Assert.Throws<ArgumentException>(() => Result.Failure(errorsWithNull));
        Assert.Equal("errors", nonGenericEx.ParamName);

        var genericEx = Assert.Throws<ArgumentException>(() => Result.Failure<string>(errorsWithNull));
        Assert.Equal("errors", genericEx.ParamName);
    }

    [Fact]
    public void Errors_DefensivelyCopiesSourceListAndIsReadOnlyAtRuntime()
    {
        // Arrange
        var error1 = Error.Validation("Profile.Invalid", "First error.", "FullName");
        var error2 = Error.Validation("Profile.Invalid2", "Second error.", "Headline");
        var sourceList = new List<Error> { error1, error2 };

        // Act
        var nonGenericSingle = Result.Failure(error1);
        var nonGenericMultiple = Result.Failure(sourceList);
        var genericSingle = Result.Failure<int>(error1);
        var genericMultiple = Result.Failure<int>(sourceList);
        var nonGenericSuccess = Result.Success();
        var genericSuccess = Result.Success("profile");

        sourceList.Add(Error.Failure("Extra.Error", "Should not appear."));
        sourceList.Clear();

        // Assert - defensive copy preserved
        Assert.Equal(2, nonGenericMultiple.Errors.Count);
        Assert.Equal(error1, nonGenericMultiple.Errors[0]);
        Assert.Equal(error2, nonGenericMultiple.Errors[1]);
        Assert.Equal(2, genericMultiple.Errors.Count);
        Assert.Equal(error1, genericMultiple.Errors[0]);
        Assert.Equal(error2, genericMultiple.Errors[1]);

        // Assert - exposed collections are not raw arrays and cannot be mutated via IList<Error>
        AssertReadOnlyErrors(nonGenericSingle.Errors, error2, expectNonEmpty: true);
        AssertReadOnlyErrors(nonGenericMultiple.Errors, error2, expectNonEmpty: true);
        AssertReadOnlyErrors(genericSingle.Errors, error2, expectNonEmpty: true);
        AssertReadOnlyErrors(genericMultiple.Errors, error2, expectNonEmpty: true);
        AssertReadOnlyErrors(nonGenericSuccess.Errors, error1, expectNonEmpty: false);
        AssertReadOnlyErrors(genericSuccess.Errors, error1, expectNonEmpty: false);
    }

    private static void AssertReadOnlyErrors(IReadOnlyList<Error> errors, Error sampleError, bool expectNonEmpty)
    {
        Assert.IsNotType<Error[]>(errors);
        var list = Assert.IsAssignableFrom<IList<Error>>(errors);
        Assert.True(list.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => list.Add(sampleError));
        Assert.Throws<NotSupportedException>(() => list.Clear());

        if (expectNonEmpty)
        {
            Assert.Throws<NotSupportedException>(() => list[0] = sampleError);
        }
    }

    [Fact]
    public void GenericResult_IsAssignableToBaseResultAndPreservesNullableFlow()
    {
        // Arrange
        var error = Error.Forbidden("Tenant.Forbidden", "Access denied.");
        Result baseResult = Result.Failure<string>(error);

        // Act & Assert
        Assert.True(baseResult.IsFailure);
        Assert.False(baseResult.IsSuccess);
        if (baseResult.IsFailure)
        {
            // Compiles without null-forgiving operator thanks to [MemberNotNullWhen(true, nameof(Error))]
            Assert.Equal("Tenant.Forbidden", baseResult.Error.Code);
        }

        if (!baseResult.IsSuccess)
        {
            // Compiles without null-forgiving operator thanks to [MemberNotNullWhen(false, nameof(Error))]
            Assert.Equal("Tenant.Forbidden", baseResult.Error.Code);
        }
    }

    [Fact]
    public void ApiSurface_HasNoPublicConstructorsNoImplicitConversionsAndNoDuplicateFactoriesOnGenericResult()
    {
        // Assert - no public constructors
        Assert.Empty(typeof(Result).GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Empty(typeof(Result<string>).GetConstructors(BindingFlags.Public | BindingFlags.Instance));

        // Assert - no implicit conversion operators
        Assert.DoesNotContain(
            typeof(Result).GetMethods(BindingFlags.Public | BindingFlags.Static),
            m => m.Name == "op_Implicit");
        Assert.DoesNotContain(
            typeof(Result<string>).GetMethods(BindingFlags.Public | BindingFlags.Static),
            m => m.Name == "op_Implicit");

        // Assert - canonical factories live on Result only (no duplicate declared static factories on Result<T>)
        var declaredGenericStaticMethods = typeof(Result<string>)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        Assert.Empty(declaredGenericStaticMethods);
    }
}

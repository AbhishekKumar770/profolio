using Profolio.Domain.Entities;
using Profolio.Domain.ValueObjects;

namespace Profolio.UnitTests;

public class DeveloperProfileTests
{
    [Fact]
    public void Constructor_WithValidArguments_CreatesDeveloperProfile()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var fullName = "Ada Lovelace";
        var headline = "First Computer Programmer";

        // Act
        var profile = new DeveloperProfile(tenantId, fullName, headline);

        // Assert
        Assert.NotEqual(Guid.Empty, profile.Id);
        Assert.Equal(tenantId, profile.TenantId);
        Assert.Equal(fullName, profile.FullName);
        Assert.Equal(headline, profile.Headline);
        Assert.Null(profile.Bio);
        Assert.Null(profile.Location);
        Assert.Null(profile.PublicContactEmail);
        Assert.Null(profile.ProfilePhotoMediaId);
        Assert.True(profile.CreatedAtUtc <= DateTime.UtcNow);
        Assert.Null(profile.UpdatedAtUtc);
    }

    [Fact]
    public void Constructor_WithEmptyTenantId_ThrowsArgumentException()
    {
        // Arrange
        var emptyTenantId = Guid.Empty;
        var fullName = "Ada Lovelace";
        var headline = "First Computer Programmer";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new DeveloperProfile(emptyTenantId, fullName, headline));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithEmptyOrWhitespaceFullName_ThrowsArgumentException(string? invalidFullName)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var headline = "First Computer Programmer";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new DeveloperProfile(tenantId, invalidFullName!, headline));
        Assert.Equal("fullName", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithEmptyOrWhitespaceHeadline_ThrowsArgumentException(string? invalidHeadline)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var fullName = "Ada Lovelace";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new DeveloperProfile(tenantId, fullName, invalidHeadline!));
        Assert.Equal("headline", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithUntrimmedStrings_TrimsFullNameAndHeadline()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var untrimmedFullName = "   Ada Lovelace   ";
        var untrimmedHeadline = "   First Computer Programmer   ";

        // Act
        var profile = new DeveloperProfile(tenantId, untrimmedFullName, untrimmedHeadline);

        // Assert
        Assert.Equal("Ada Lovelace", profile.FullName);
        Assert.Equal("First Computer Programmer", profile.Headline);
    }

    [Fact]
    public void Constructor_WithFullNameAtMaximumLength_Succeeds()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var maxFullName = new string('F', DeveloperProfile.MaxFullNameLength);

        // Act
        var profile = new DeveloperProfile(tenantId, maxFullName, "Programmer");

        // Assert
        Assert.Equal(maxFullName, profile.FullName);
        Assert.Equal(DeveloperProfile.MaxFullNameLength, profile.FullName.Length);
    }

    [Fact]
    public void Constructor_WithFullNameAtMaximumLengthSurroundedByWhitespace_TrimsAndSucceeds()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var maxFullName = new string('F', DeveloperProfile.MaxFullNameLength);
        var paddedFullName = $"   {maxFullName}   ";

        // Act
        var profile = new DeveloperProfile(tenantId, paddedFullName, "Programmer");

        // Assert
        Assert.Equal(maxFullName, profile.FullName);
        Assert.Equal(DeveloperProfile.MaxFullNameLength, profile.FullName.Length);
    }

    [Fact]
    public void Constructor_WithFullNameExceedingMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var excessiveFullName = new string('F', DeveloperProfile.MaxFullNameLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new DeveloperProfile(tenantId, excessiveFullName, "Programmer"));
        Assert.Equal("fullName", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithHeadlineAtMaximumLength_Succeeds()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var maxHeadline = new string('H', DeveloperProfile.MaxHeadlineLength);

        // Act
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", maxHeadline);

        // Assert
        Assert.Equal(maxHeadline, profile.Headline);
        Assert.Equal(DeveloperProfile.MaxHeadlineLength, profile.Headline.Length);
    }

    [Fact]
    public void Constructor_WithHeadlineAtMaximumLengthSurroundedByWhitespace_TrimsAndSucceeds()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var maxHeadline = new string('H', DeveloperProfile.MaxHeadlineLength);
        var paddedHeadline = $"   {maxHeadline}   ";

        // Act
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", paddedHeadline);

        // Assert
        Assert.Equal(maxHeadline, profile.Headline);
        Assert.Equal(DeveloperProfile.MaxHeadlineLength, profile.Headline.Length);
    }

    [Fact]
    public void Constructor_WithHeadlineExceedingMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var excessiveHeadline = new string('H', DeveloperProfile.MaxHeadlineLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new DeveloperProfile(tenantId, "Ada Lovelace", excessiveHeadline));
        Assert.Equal("headline", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateBasicInformation_WithEmptyOrWhitespaceBioAndLocation_NormalizesToNull(string? emptyValue)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");

        // Act
        profile.UpdateBasicInformation("Ada Lovelace", "Programmer", emptyValue, emptyValue);

        // Assert
        Assert.Null(profile.Bio);
        Assert.Null(profile.Location);
    }

    [Fact]
    public void UpdateBasicInformation_WithUntrimmedBio_TrimsLeadingAndTrailingWhitespace()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        var untrimmedBio = "   Visionary mathematician and writer.   ";

        // Act
        profile.UpdateBasicInformation("Ada Lovelace", "Programmer", untrimmedBio, null);

        // Assert
        Assert.Equal("Visionary mathematician and writer.", profile.Bio);
    }

    [Fact]
    public void UpdateBasicInformation_WithVeryLongBio_AcceptsAndStoresBioWithoutLengthRestriction()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        var longBio = new string('B', 10000);

        // Act
        profile.UpdateBasicInformation("Ada Lovelace", "Programmer", longBio, null);

        // Assert
        Assert.Equal(longBio, profile.Bio);
        Assert.Equal(10000, profile.Bio?.Length);
    }

    [Fact]
    public void UpdateBasicInformation_WithLocationAtMaximumLength_Succeeds()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        var maxLocation = new string('L', DeveloperProfile.MaxLocationLength);

        // Act
        profile.UpdateBasicInformation("Ada Lovelace", "Programmer", "Bio", maxLocation);

        // Assert
        Assert.Equal(maxLocation, profile.Location);
        Assert.Equal(DeveloperProfile.MaxLocationLength, profile.Location?.Length);
    }

    [Fact]
    public void UpdateBasicInformation_WithLocationAtMaximumLengthSurroundedByWhitespace_TrimsAndSucceeds()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        var maxLocation = new string('L', DeveloperProfile.MaxLocationLength);
        var paddedLocation = $"   {maxLocation}   ";

        // Act
        profile.UpdateBasicInformation("Ada Lovelace", "Programmer", "Bio", paddedLocation);

        // Assert
        Assert.Equal(maxLocation, profile.Location);
        Assert.Equal(DeveloperProfile.MaxLocationLength, profile.Location?.Length);
    }

    [Fact]
    public void UpdateBasicInformation_WithLocationExceedingMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        var excessiveLocation = new string('L', DeveloperProfile.MaxLocationLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => profile.UpdateBasicInformation("Ada Lovelace", "Programmer", "Bio", excessiveLocation));
        Assert.Equal("location", exception.ParamName);
    }

    [Fact]
    public void UpdateBasicInformation_PreservesTenantId()
    {
        // Arrange
        var originalTenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(originalTenantId, "Ada Lovelace", "Programmer");

        // Act
        profile.UpdateBasicInformation("Ada King", "Countess of Lovelace", "Mathematician", "London");

        // Assert
        Assert.Equal(originalTenantId, profile.TenantId);
    }

    [Fact]
    public void UpdateBasicInformation_PreservesId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        var originalId = profile.Id;

        // Act
        profile.UpdateBasicInformation("Ada King", "Countess of Lovelace", "Mathematician", "London");

        // Assert
        Assert.Equal(originalId, profile.Id);
    }

    [Fact]
    public void UpdateBasicInformation_PopulatesUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        Assert.Null(profile.UpdatedAtUtc);

        // Act
        profile.UpdateBasicInformation("Ada King", "Countess of Lovelace", "Mathematician", "London");

        // Assert
        Assert.NotNull(profile.UpdatedAtUtc);
        Assert.True(profile.UpdatedAtUtc >= profile.CreatedAtUtc);
        Assert.True(profile.UpdatedAtUtc <= DateTime.UtcNow);
    }

    [Fact]
    public void UpdateBasicInformation_WithUnchangedValues_DoesNotModifyEntityOrUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        profile.UpdateBasicInformation("Ada King", "Countess of Lovelace", "Mathematician", "London");
        var originalUpdatedAt = profile.UpdatedAtUtc;

        // Act
        profile.UpdateBasicInformation("Ada King", "Countess of Lovelace", "Mathematician", "London");

        // Assert
        Assert.Equal(originalUpdatedAt, profile.UpdatedAtUtc);
        Assert.Equal("Ada King", profile.FullName);
        Assert.Equal("Countess of Lovelace", profile.Headline);
        Assert.Equal("Mathematician", profile.Bio);
        Assert.Equal("London", profile.Location);
    }

    [Fact]
    public void UpdateBasicInformation_WithPaddedValuesNormalizingToExistingValues_DoesNotModifyEntityOrUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        profile.UpdateBasicInformation("Ada King", "Countess of Lovelace", "Mathematician", "London");
        var originalUpdatedAt = profile.UpdatedAtUtc;

        // Act - values padded with whitespace that normalize identically
        profile.UpdateBasicInformation("  Ada King  ", "  Countess of Lovelace  ", "  Mathematician  ", "  London  ");

        // Assert
        Assert.Equal(originalUpdatedAt, profile.UpdatedAtUtc);
    }

    // =========================================================================
    // ATOMICITY TESTS
    // =========================================================================

    [Fact]
    public void UpdateBasicInformation_WhenFullNameInvalid_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Original Name", "Original Headline");
        profile.UpdateBasicInformation("Original Name", "Original Headline", "Original Bio", "Original Location");
        var originalUpdatedAt = profile.UpdatedAtUtc;
        var originalId = profile.Id;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => profile.UpdateBasicInformation(
            "   ",
            "New Headline",
            "New Bio",
            "New Location"));

        Assert.Equal("fullName", exception.ParamName);
        Assert.Equal("Original Name", profile.FullName);
        Assert.Equal("Original Headline", profile.Headline);
        Assert.Equal("Original Bio", profile.Bio);
        Assert.Equal("Original Location", profile.Location);
        Assert.Equal(originalId, profile.Id);
        Assert.Equal(tenantId, profile.TenantId);
        Assert.Equal(originalUpdatedAt, profile.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateBasicInformation_WhenFullNameExceedsMaxLength_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Original Name", "Original Headline");
        profile.UpdateBasicInformation("Original Name", "Original Headline", "Original Bio", "Original Location");
        var originalUpdatedAt = profile.UpdatedAtUtc;
        var excessiveName = new string('F', DeveloperProfile.MaxFullNameLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => profile.UpdateBasicInformation(
            excessiveName,
            "New Headline",
            "New Bio",
            "New Location"));

        Assert.Equal("fullName", exception.ParamName);
        Assert.Equal("Original Name", profile.FullName);
        Assert.Equal("Original Headline", profile.Headline);
        Assert.Equal("Original Bio", profile.Bio);
        Assert.Equal("Original Location", profile.Location);
        Assert.Equal(originalUpdatedAt, profile.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateBasicInformation_WhenHeadlineInvalid_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Original Name", "Original Headline");
        profile.UpdateBasicInformation("Original Name", "Original Headline", "Original Bio", "Original Location");
        var originalUpdatedAt = profile.UpdatedAtUtc;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => profile.UpdateBasicInformation(
            "Valid Changed Name",
            "   ",
            "New Bio",
            "New Location"));

        Assert.Equal("headline", exception.ParamName);
        Assert.Equal("Original Name", profile.FullName);
        Assert.Equal("Original Headline", profile.Headline);
        Assert.Equal("Original Bio", profile.Bio);
        Assert.Equal("Original Location", profile.Location);
        Assert.Equal(originalUpdatedAt, profile.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateBasicInformation_WhenHeadlineExceedsMaxLength_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Original Name", "Original Headline");
        profile.UpdateBasicInformation("Original Name", "Original Headline", "Original Bio", "Original Location");
        var originalUpdatedAt = profile.UpdatedAtUtc;
        var excessiveHeadline = new string('H', DeveloperProfile.MaxHeadlineLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => profile.UpdateBasicInformation(
            "Valid Changed Name",
            excessiveHeadline,
            "New Bio",
            "New Location"));

        Assert.Equal("headline", exception.ParamName);
        Assert.Equal("Original Name", profile.FullName);
        Assert.Equal("Original Headline", profile.Headline);
        Assert.Equal("Original Bio", profile.Bio);
        Assert.Equal("Original Location", profile.Location);
        Assert.Equal(originalUpdatedAt, profile.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateBasicInformation_WhenLocationExceedsMaxLength_ThrowsAndDoesNotMutateAnyState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Original Name", "Original Headline");
        profile.UpdateBasicInformation("Original Name", "Original Headline", "Original Bio", "Original Location");
        var originalUpdatedAt = profile.UpdatedAtUtc;
        var excessiveLocation = new string('L', DeveloperProfile.MaxLocationLength + 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => profile.UpdateBasicInformation(
            "Valid Changed Name",
            "Valid Changed Headline",
            "New Bio",
            excessiveLocation));

        Assert.Equal("location", exception.ParamName);
        Assert.Equal("Original Name", profile.FullName);
        Assert.Equal("Original Headline", profile.Headline);
        Assert.Equal("Original Bio", profile.Bio);
        Assert.Equal("Original Location", profile.Location);
        Assert.Equal(originalUpdatedAt, profile.UpdatedAtUtc);
    }

    // =========================================================================
    // PROFILE PHOTO TESTS
    // =========================================================================

    [Fact]
    public void SetProfilePhoto_WithValidMediaId_SetsPropertyAndUpdatesTimestamp()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        var mediaId = Guid.NewGuid();

        // Act
        profile.SetProfilePhoto(mediaId);

        // Assert
        Assert.Equal(mediaId, profile.ProfilePhotoMediaId);
        Assert.NotNull(profile.UpdatedAtUtc);
        Assert.True(profile.UpdatedAtUtc >= profile.CreatedAtUtc);
    }

    [Fact]
    public void SetProfilePhoto_WithEmptyMediaId_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => profile.SetProfilePhoto(Guid.Empty));
        Assert.Equal("mediaId", exception.ParamName);
        Assert.Null(profile.ProfilePhotoMediaId);
        Assert.Null(profile.UpdatedAtUtc);
    }

    [Fact]
    public void SetProfilePhoto_WithSameMediaId_DoesNotModifyUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        var mediaId = Guid.NewGuid();
        profile.SetProfilePhoto(mediaId);
        var originalUpdatedAt = profile.UpdatedAtUtc;

        // Act
        profile.SetProfilePhoto(mediaId);

        // Assert
        Assert.Equal(mediaId, profile.ProfilePhotoMediaId);
        Assert.Equal(originalUpdatedAt, profile.UpdatedAtUtc);
    }

    [Fact]
    public void SetProfilePhoto_ReplacingExistingPhoto_SetsNewPropertyAndUpdatesTimestamp()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        var initialMediaId = Guid.NewGuid();
        var newMediaId = Guid.NewGuid();
        profile.SetProfilePhoto(initialMediaId);

        // Act
        profile.SetProfilePhoto(newMediaId);

        // Assert
        Assert.Equal(newMediaId, profile.ProfilePhotoMediaId);
        Assert.NotNull(profile.UpdatedAtUtc);
    }

    [Fact]
    public void SetProfilePhoto_PreservesIdAndTenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        var originalId = profile.Id;

        // Act
        profile.SetProfilePhoto(Guid.NewGuid());

        // Assert
        Assert.Equal(originalId, profile.Id);
        Assert.Equal(tenantId, profile.TenantId);
    }

    [Fact]
    public void RemoveProfilePhoto_WhenPhotoExists_ClearsPropertyAndUpdatesTimestamp()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        profile.SetProfilePhoto(Guid.NewGuid());
        Assert.NotNull(profile.ProfilePhotoMediaId);

        // Act
        profile.RemoveProfilePhoto();

        // Assert
        Assert.Null(profile.ProfilePhotoMediaId);
        Assert.NotNull(profile.UpdatedAtUtc);
    }

    [Fact]
    public void RemoveProfilePhoto_WhenAlreadyNull_DoesNotModifyEntityOrUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        Assert.Null(profile.ProfilePhotoMediaId);
        Assert.Null(profile.UpdatedAtUtc);

        // Act
        profile.RemoveProfilePhoto();

        // Assert
        Assert.Null(profile.ProfilePhotoMediaId);
        Assert.Null(profile.UpdatedAtUtc);
    }

    [Fact]
    public void RemoveProfilePhoto_PreservesIdAndTenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        profile.SetProfilePhoto(Guid.NewGuid());
        var originalId = profile.Id;

        // Act
        profile.RemoveProfilePhoto();

        // Assert
        Assert.Equal(originalId, profile.Id);
        Assert.Equal(tenantId, profile.TenantId);
    }

    // =========================================================================
    // PUBLIC CONTACT EMAIL TESTS (PRESERVED)
    // =========================================================================

    [Fact]
    public void SetPublicContactEmail_WithValidEmail_SetsPropertyAndUpdatesTimestamp()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        var email = new EmailAddress("ada.lovelace@example.com");

        // Act
        profile.SetPublicContactEmail(email);

        // Assert
        Assert.NotNull(profile.PublicContactEmail);
        Assert.Equal(email, profile.PublicContactEmail);
        Assert.NotNull(profile.UpdatedAtUtc);
        Assert.True(profile.UpdatedAtUtc >= profile.CreatedAtUtc);
    }

    [Fact]
    public void SetPublicContactEmail_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => profile.SetPublicContactEmail(null!));
        Assert.Equal("contactEmail", exception.ParamName);
    }

    [Fact]
    public void SetPublicContactEmail_PreservesIdAndTenantId()
    {
        // Arrange
        var originalTenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(originalTenantId, "Ada Lovelace", "Programmer");
        var originalId = profile.Id;
        var email = new EmailAddress("ada.lovelace@example.com");

        // Act
        profile.SetPublicContactEmail(email);

        // Assert
        Assert.Equal(originalId, profile.Id);
        Assert.Equal(originalTenantId, profile.TenantId);
    }

    [Fact]
    public void SetPublicContactEmail_WithSameEmailInstance_DoesNotModifyUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        var email = new EmailAddress("ada.lovelace@example.com");
        profile.SetPublicContactEmail(email);
        var originalUpdatedAt = profile.UpdatedAtUtc;

        // Act
        profile.SetPublicContactEmail(email);

        // Assert
        Assert.Equal(originalUpdatedAt, profile.UpdatedAtUtc);
    }

    [Fact]
    public void SetPublicContactEmail_WithDifferentInstanceHavingSameNormalizedValue_DoesNotModifyUpdatedAtUtc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        var email1 = new EmailAddress("ada.lovelace@example.com");
        var email2 = new EmailAddress("ada.lovelace@EXAMPLE.com");
        profile.SetPublicContactEmail(email1);
        var originalUpdatedAt = profile.UpdatedAtUtc;

        // Act
        profile.SetPublicContactEmail(email2);

        // Assert
        Assert.Equal(originalUpdatedAt, profile.UpdatedAtUtc);
    }

    [Fact]
    public void ClearPublicContactEmail_WhenEmailExists_ClearsEmailAndUpdatesTimestamp()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        profile.SetPublicContactEmail(new EmailAddress("ada.lovelace@example.com"));
        Assert.NotNull(profile.PublicContactEmail);

        // Act
        profile.ClearPublicContactEmail();

        // Assert
        Assert.Null(profile.PublicContactEmail);
        Assert.NotNull(profile.UpdatedAtUtc);
    }

    [Fact]
    public void ClearPublicContactEmail_WhenAlreadyNull_DoesNotModifyEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var profile = new DeveloperProfile(tenantId, "Ada Lovelace", "Programmer");
        Assert.Null(profile.PublicContactEmail);
        Assert.Null(profile.UpdatedAtUtc);

        // Act
        profile.ClearPublicContactEmail();

        // Assert
        Assert.Null(profile.PublicContactEmail);
        Assert.Null(profile.UpdatedAtUtc);
    }
}

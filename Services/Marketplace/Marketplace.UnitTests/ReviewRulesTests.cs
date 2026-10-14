using FluentAssertions;
using Marketplace.Application.Abstractions;
using Xunit;

namespace Marketplace.UnitTests;

public class ReviewRulesTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Validate_WithValidRatingAndComment_ShouldSucceed(int rating)
    {
        // Act
        var result = ReviewRules.Validate(rating, "Super service !");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(10)]
    public void Validate_WithRatingOutOfRange_ShouldFail(int rating)
    {
        // Act
        var result = ReviewRules.Validate(rating, "Commentaire");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Rating must be between 1 and 5");
    }

    [Fact]
    public void Validate_WithCommentExceedingMaxCommentLength_ShouldFail()
    {
        // Arrange
        var longComment = new string('a', 1001);

        // Act
        var result = ReviewRules.Validate(5, longComment);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Comment must not exceed 1000 characters");
    }

    [Fact]
    public void Validate_WithNullComment_ShouldSucceed()
    {
        // Act
        var result = ReviewRules.Validate(4, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void NormalizeComment_WithNull_ShouldReturnEmptyString()
    {
        // Act
        var normalized = ReviewRules.NormalizeComment(null);

        // Assert
        normalized.Should().Be(string.Empty);
    }

    [Fact]
    public void NormalizeComment_WithSurroundingSpaces_ShouldTrim()
    {
        // Act
        var normalized = ReviewRules.NormalizeComment("   Très bon travail   ");

        // Assert
        normalized.Should().Be("Très bon travail");
    }
}

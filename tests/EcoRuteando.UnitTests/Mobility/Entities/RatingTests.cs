using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Shared.Exceptions;
using Xunit;

namespace EcoRuteando.UnitTests.Mobility.Entities;

public sealed class RatingTests
{
    private readonly Guid _validUserId = Guid.NewGuid();
    private readonly Guid _validRouteId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsDomainException()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new Rating(Guid.Empty, _validRouteId, 4, "Excelente ruta"));

        Assert.Equal("El usuario es obligatorio.", ex.Message);
    }

    [Fact]
    public void Constructor_WithEmptyRouteId_ThrowsDomainException()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new Rating(_validUserId, Guid.Empty, 4, "Excelente ruta"));

        Assert.Equal("La ruta es obligatoria.", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(10)]
    public void Constructor_WithInvalidRatingValue_ThrowsDomainException(short invalidRating)
    {
        var ex = Assert.Throws<DomainException>(() =>
            new Rating(_validUserId, _validRouteId, invalidRating, "Comentario"));

        Assert.Equal("La calificación debe estar entre 1 y 5 estrellas.", ex.Message);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Constructor_WithValidData_InitializesCorrectly(short ratingValue)
    {
        var validUsageId = Guid.NewGuid();
        var rating = new Rating(_validUserId, _validRouteId, ratingValue, "  Muy buen camino  ", usageId: validUsageId);

        Assert.NotEqual(Guid.Empty, rating.Id);
        Assert.Equal(_validUserId, rating.UserId);
        Assert.Equal(_validRouteId, rating.RouteId);
        Assert.Equal(validUsageId, rating.UsageId);
        Assert.Equal(ratingValue, rating.RatingValue);
        Assert.Equal("Muy buen camino", rating.Comment);
        Assert.Equal(0, rating.HelpfulCount);
        Assert.True(rating.IsPublished);
    }

    [Fact]
    public void Constructor_WithoutUsageId_SetsUsageIdToNull()
    {
        var rating = new Rating(_validUserId, _validRouteId, 4, "Excelente ruta", usageId: null);

        Assert.Null(rating.UsageId);
    }

    [Fact]
    public void Constructor_WithWhitespaceComment_SetsCommentToNull()
    {
        var rating = new Rating(_validUserId, _validRouteId, 4, "   ");

        Assert.Null(rating.Comment);
        Assert.Null(rating.UsageId);
    }

    [Fact]
    public void Update_WithInvalidRatingValue_ThrowsDomainException()
    {
        var rating = new Rating(_validUserId, _validRouteId, 4, "Original");

        var ex = Assert.Throws<DomainException>(() => rating.Update(0, "Nuevo"));

        Assert.Equal("La calificación debe estar entre 1 y 5 estrellas.", ex.Message);
    }

    [Fact]
    public void Update_WithValidData_UpdatesRatingValueAndComment()
    {
        var rating = new Rating(_validUserId, _validRouteId, 3, "Original");

        rating.Update(5, "  Actualizado  ");

        Assert.Equal(5, rating.RatingValue);
        Assert.Equal("Actualizado", rating.Comment);
    }
}

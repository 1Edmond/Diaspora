using Shared.Domain.Commons;
using System;

namespace Marketplace.Application.Abstractions;

/// <summary>
/// Règles de validation des avis, partagées par les handlers de création et de mise à jour.
/// Les bornes reflètent le schéma : <c>reviews.Comment</c> est limité à 1000 caractères
/// (voir ReviewConfiguration) et la note est comprise entre 1 et 5 (voir Review.Create).
/// </summary>
public static class ReviewRules
{
    public const int MinRating = 1;
    public const int MaxRating = 5;
    public const int MaxCommentLength = 1000;

    /// <summary>Normalise un commentaire : jamais null, sans espaces superflus.</summary>
    public static string NormalizeComment(string? comment)
        => (comment ?? string.Empty).Trim();

    /// <summary>
    /// Valide la note et le commentaire. Évite que Review.Create / Review.Update ne lèvent
    /// une ArgumentException (qui remonterait en HTTP 500) sur une entrée client invalide.
    /// </summary>
    public static Result Validate(int rating, string? comment)
    {
        if (rating < MinRating || rating > MaxRating)
            return Result.Failure($"Rating must be between {MinRating} and {MaxRating}.");

        if (NormalizeComment(comment).Length > MaxCommentLength)
            return Result.Failure($"Comment must not exceed {MaxCommentLength} characters.");

        return Result.Success();
    }
}

using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Marketplace.Application.Commands;

/// <summary>
/// Création d'un avis. Le caractère « vérifié » n'est volontairement PAS un paramètre :
/// il est déterminé côté serveur (voir CreateReviewCommandHandler) à partir des demandes
/// de service réellement complétées entre l'auteur et le prestataire.
/// </summary>
public record CreateReviewCommand(
    Guid ListingId,
    Guid ReviewerId,
    int Rating,
    string Comment
) : ICommand<Result<Guid>>;

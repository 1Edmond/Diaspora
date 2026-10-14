using Shared.Application.Interfaces;
using Wallet.Application.Interfaces;

namespace Wallet.Api.Security;

/// <summary>
/// Contrôle de propriété des wallets. Le jeton ne porte que l'identifiant utilisateur
/// (ClaimTypes.NameIdentifier) ; le wallet référence ce même identifiant dans Wallet.UserId.
/// Toute opération sur un wallet identifié par un ExternalProfileId fourni par le client
/// DOIT passer par ces méthodes avant d'être exécutée.
/// </summary>
public static class WalletOwnership
{
    /// <summary>Vrai uniquement si le wallet du profil donné appartient à l'utilisateur authentifié.</summary>
    public static async Task<bool> IsOwnerAsync(
        IWalletRepository walletRepository,
        ICurrentUserService currentUser,
        Guid externalProfileId,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.UserId, out var userId))
            return false;

        var wallet = await walletRepository.GetByExternalProfileIdAsync(externalProfileId, cancellationToken);
        return wallet is not null && wallet.UserId == userId;
    }

    /// <summary>Lecture : propriétaire du wallet ou administrateur.</summary>
    public static async Task<bool> CanReadAsync(
        IWalletRepository walletRepository,
        ICurrentUserService currentUser,
        Guid externalProfileId,
        CancellationToken cancellationToken)
        => currentUser.IsInRole("Admin")
           || await IsOwnerAsync(walletRepository, currentUser, externalProfileId, cancellationToken);
}

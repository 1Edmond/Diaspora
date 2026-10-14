namespace Shared.Domain.Enums;

/// <summary>
/// Niveaux de vérification d'un profil utilisateur au sein de la plateforme.
///
/// L0_Registered : Compte inscrit, email/téléphone vérifié.
/// L1_IdentityDeclared : Nom, prénom, nationalité et adresse permanente déclarés.
/// L2_IdentityVerified : Pièce d'identité et selfie validés par KYC (badge « Vérifié »).
/// L3_EnhancedVerified : Vérification renforcée (justificatif de domicile, compte bancaire / historique).
/// </summary>
public enum VerificationLevel : byte
{
    L0_Registered = 0,
    L1_IdentityDeclared = 1,
    L2_IdentityVerified = 2,
    L3_EnhancedVerified = 3
}

using Marketplace.Domain.Enums;
using Marketplace.Domain.Events;
using Marketplace.Domain.ValueObjects;
using Shared.Domain.Commons;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Domain.Entities;

public class ServiceRequest : AggregateRoot
{
    public Guid ListingId { get; private set; }
    public Guid RequesterId { get; private set; }      // ExternalProfile Guid
    public Guid ProviderId { get; private set; }       // ExternalProfile Guid
    public string Message { get; private set; } = string.Empty;
    public ServiceRequestStatus Status { get; private set; }

    // Paiement (si PaymentMode = WalletPayment ou Both)
    public decimal? AgreedPrice { get; private set; }
    public string? AgreedCurrency { get; private set; }
    public Guid? WalletTransactionId { get; private set; }   // Référence vers Wallet Service
    public PaymentStatus PaymentStatus { get; private set; }

    // Colis & Suivi physique
    public decimal? WeightKg { get; private set; }
    public string? PackageDescription { get; private set; }
    public string? PackageContentCategory { get; private set; }
    public decimal? DeclaredValue { get; private set; }
    public string? HandoverOtpHash { get; private set; }
    public string? DeliveryOtpHash { get; private set; }
    public DateTime? HandoverConfirmedAt { get; private set; }
    public string? HandoverProofUrl { get; private set; }
    public DateTime? DeliveryConfirmedAt { get; private set; }
    public string? DeliveryProofUrl { get; private set; }

    // Dates
    public DateTime? AcceptedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    // Messagerie — référence vers le fil de discussion créé côté Chat Service
    public Guid? ChatThreadId { get; private set; }

    // Litiges (Dispute)
    public bool IsInDispute { get; private set; }
    public Guid? ActiveDisputeId { get; private set; }

    public ServiceListing Listing { get; private set; } = null!;

    private ServiceRequest() { }

    public static ServiceRequest Create(
        Guid listingId,
        Guid requesterId,
        Guid providerId,
        string message,
        decimal? agreedPrice = null,
        string? agreedCurrency = null,
        decimal? weightKg = null,
        string? packageDescription = null,
        string? packageContentCategory = null,
        decimal? declaredValue = null)
    {
        var request = new ServiceRequest
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            RequesterId = requesterId,
            ProviderId = providerId,
            Message = message,
            Status = ServiceRequestStatus.Pending,
            AgreedPrice = agreedPrice,
            AgreedCurrency = agreedCurrency,
            PaymentStatus = agreedPrice.HasValue ? PaymentStatus.Pending : PaymentStatus.NotApplicable,
            WeightKg = weightKg,
            PackageDescription = packageDescription,
            PackageContentCategory = packageContentCategory,
            DeclaredValue = declaredValue
        };

        request.AddDomainEvent(new ServiceRequestCreatedEvent(request.Id, listingId, requesterId, providerId));
        return request;
    }

    public void Accept()
    {
        Status = ServiceRequestStatus.Accepted;
        AcceptedAt = DateTime.UtcNow;
        AddDomainEvent(new ServiceRequestAcceptedEvent(Id, ListingId, RequesterId, ProviderId));
    }

    public void Decline(string reason)
    {
        Status = ServiceRequestStatus.Declined;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
    }

    public void Complete()
    {
        Status = ServiceRequestStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        AddDomainEvent(new ServiceRequestCompletedEvent(Id, ListingId, RequesterId, ProviderId, AgreedPrice, AgreedCurrency));
    }

    public void Cancel(string reason)
    {
        Status = ServiceRequestStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
    }

    public void GenerateOtps(out string rawHandoverOtp, out string rawDeliveryOtp)
    {
        var handoverSecret = OtpSecret.Generate(out rawHandoverOtp);
        var deliverySecret = OtpSecret.Generate(out rawDeliveryOtp);

        HandoverOtpHash = handoverSecret.ToStorageString();
        DeliveryOtpHash = deliverySecret.ToStorageString();
    }

    public Result ConfirmHandover(string otp, string? proofPhotoUrl)
    {
        if (IsInDispute)
            return Result.Failure("Un litige est actuellement en cours sur cette demande.");

        if (Status != ServiceRequestStatus.Accepted)
            return Result.Failure("La remise du colis ne peut être confirmée que pour une demande acceptée.");

        if (string.IsNullOrWhiteSpace(HandoverOtpHash))
            return Result.Failure("Aucun code OTP de remise n'a été généré pour cette demande.");

        var secret = OtpSecret.FromStorage(HandoverOtpHash);
        if (secret.IsLockedOut)
            return Result.Failure("Nombre maximum de tentatives atteint pour le code de remise.");
        if (secret.IsExpired)
            return Result.Failure("Le code de remise a expiré.");

        var isValid = secret.Verify(otp);
        HandoverOtpHash = secret.ToStorageString();

        if (!isValid)
            return Result.Failure($"Code de remise invalide. Tentative {secret.FailedAttempts}/{OtpSecret.MaxFailedAttempts}.");

        Status = ServiceRequestStatus.PackageHandedOver;
        HandoverConfirmedAt = DateTime.UtcNow;
        HandoverProofUrl = proofPhotoUrl;

        return Result.Success();
    }

    public Result MarkInTransit()
    {
        if (Status != ServiceRequestStatus.PackageHandedOver)
            return Result.Failure("Le colis doit avoir été remis au voyageur pour passer en transit.");

        Status = ServiceRequestStatus.InTransit;
        return Result.Success();
    }

    public Result MarkArrivedAtDestination()
    {
        if (Status != ServiceRequestStatus.InTransit)
            return Result.Failure("Le colis doit être en transit pour marquer l'arrivée à destination.");

        Status = ServiceRequestStatus.ArrivedAtDestination;
        return Result.Success();
    }

    public Result ConfirmDelivery(string otp, string? proofPhotoUrl)
    {
        if (IsInDispute)
            return Result.Failure("Un litige est actuellement en cours sur cette demande. Impossible de confirmer la livraison ou de libérer les fonds.");

        if (Status != ServiceRequestStatus.ArrivedAtDestination && Status != ServiceRequestStatus.InTransit && Status != ServiceRequestStatus.PackageHandedOver)
            return Result.Failure("La livraison ne peut être confirmée que pour un colis en cours d'acheminement.");

        if (string.IsNullOrWhiteSpace(DeliveryOtpHash))
            return Result.Failure("Aucun code OTP de livraison n'a été généré pour cette demande.");

        var secret = OtpSecret.FromStorage(DeliveryOtpHash);
        if (secret.IsLockedOut)
            return Result.Failure("Nombre maximum de tentatives atteint pour le code de livraison.");
        if (secret.IsExpired)
            return Result.Failure("Le code de livraison a expiré.");

        var isValid = secret.Verify(otp);
        DeliveryOtpHash = secret.ToStorageString();

        if (!isValid)
            return Result.Failure($"Code de livraison invalide. Tentative {secret.FailedAttempts}/{OtpSecret.MaxFailedAttempts}.");

        Status = ServiceRequestStatus.Delivered;
        DeliveryConfirmedAt = DateTime.UtcNow;
        DeliveryProofUrl = proofPhotoUrl;
        CompletedAt = DateTime.UtcNow;

        AddDomainEvent(new ServiceRequestCompletedEvent(
            Id, ListingId, RequesterId, ProviderId, AgreedPrice, AgreedCurrency));

        return Result.Success();
    }

    public void MarkPaymentProcessing()
    {
        PaymentStatus = PaymentStatus.Processing;
    }

    public void MarkPaymentHeld(Guid transactionId)
    {
        WalletTransactionId = transactionId;
        PaymentStatus = PaymentStatus.Held;

        if (WeightKg.HasValue && string.IsNullOrWhiteSpace(HandoverOtpHash))
        {
            GenerateOtps(out _, out _);
        }
    }

    public void MarkPaymentFailed()
    {
        PaymentStatus = PaymentStatus.Failed;
    }

    public void MarkPaymentReleased(Guid transactionId)
    {
        WalletTransactionId = transactionId;
        PaymentStatus = PaymentStatus.Released;
    }

    public void MarkPaymentRefunded(Guid transactionId)
    {
        WalletTransactionId = transactionId;
        PaymentStatus = PaymentStatus.Refunded;
    }

    public void MarkPaymentCompleted(Guid transactionId) => MarkPaymentReleased(transactionId);

    public void LinkChatThread(Guid chatThreadId)
    {
        ChatThreadId = chatThreadId;
    }

    public Result OpenDispute(Guid disputeId)
    {
        if (IsInDispute)
            return Result.Failure("Un litige est déjà ouvert pour cette demande.");
        if (Status == ServiceRequestStatus.Cancelled || Status == ServiceRequestStatus.Declined)
            return Result.Failure("Impossible d'ouvrir un litige sur une demande annulée ou refusée.");

        IsInDispute = true;
        ActiveDisputeId = disputeId;
        return Result.Success();
    }

    public void ResolveDispute()
    {
        IsInDispute = false;
        ActiveDisputeId = null;
    }
}

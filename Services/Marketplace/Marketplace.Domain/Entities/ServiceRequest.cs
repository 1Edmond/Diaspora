using Marketplace.Domain.Enums;
using Marketplace.Domain.Events;
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

    // Dates
    public DateTime? AcceptedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    // Messagerie — référence vers le fil de discussion créé côté Chat Service
    public Guid? ChatThreadId { get; private set; }

    public ServiceListing Listing { get; private set; } = null!;

    private ServiceRequest() { }

    public static ServiceRequest Create(
        Guid listingId,
        Guid requesterId,
        Guid providerId,
        string message,
        decimal? agreedPrice = null,
        string? agreedCurrency = null)
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
            PaymentStatus = agreedPrice.HasValue ? PaymentStatus.Pending : PaymentStatus.NotApplicable
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

    public void MarkPaymentCompleted(Guid transactionId)
    {
        WalletTransactionId = transactionId;
        PaymentStatus = PaymentStatus.Completed;
    }

    public void LinkChatThread(Guid chatThreadId)
    {
        ChatThreadId = chatThreadId;
    }
}

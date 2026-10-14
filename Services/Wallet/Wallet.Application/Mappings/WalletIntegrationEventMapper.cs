using Shared.Domain.Interfaces;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Wallet;
using Shared.Messaging.Events.Freelance;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Domain.Events;

namespace Wallet.Application.Mappings;

public sealed class WalletIntegrationEventMapper : IIntegrationEventMapper
{
    public IIntegrationEvent? Map(IDomainEvent domainEvent) =>
        domainEvent switch
        {
            WalletCreatedDomainEvent e => new WalletCreatedIntegrationEvent(
                e.WalletId, e.ExternalProfileId, e.UserId, e.Currency),

            DepositCompletedDomainEvent e => new DepositCompletedIntegrationEvent(
                e.WalletId, e.TransactionId, e.Amount, e.Currency, e.NewBalance),

            TransferCompletedDomainEvent e => new TransferCompletedIntegrationEvent(
                e.SenderWalletId, e.ReceiverWalletId, e.TransactionId,
                e.Amount, e.Fee, e.Currency),

            ProcedurePaymentCompletedDomainEvent e => new ProcedurePaymentCompletedIntegrationEvent(
                e.WalletId, e.TransactionId, e.ProcedureId, e.Amount, e.Currency),


            FreelanceEscrowHoldCompletedDomainEvent e => new FreelanceEscrowHoldCompletedIntegrationEvent(
                e.EmployerWalletId, e.TransactionId, e.JobApplicationId, e.Amount, e.Currency),

            FreelanceEscrowReleasedDomainEvent e => new FreelanceEscrowReleasedIntegrationEvent(
                e.EmployerWalletId, e.WorkerWalletId, e.TransactionId, e.JobApplicationId, e.Amount, e.Currency),

            FreelanceEscrowRefundedDomainEvent e => new FreelanceEscrowRefundedIntegrationEvent(
                e.EmployerWalletId, e.TransactionId, e.JobApplicationId, e.Amount, e.Currency),

            FreelancePaymentCompletedDomainEvent e => new FreelancePaymentCompletedIntegrationEvent(
                e.EmployerWalletId, e.WorkerWalletId, e.TransactionId, e.JobApplicationId, e.Amount, e.Currency),

            _ => null
        };
}

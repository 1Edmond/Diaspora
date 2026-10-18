using Marketplace.Domain.Enums;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Commands;

public record CreateServiceRequestCommand(
    Guid ListingId,
    Guid RequesterId,
    string Message,
    decimal? AgreedPrice,
    string? AgreedCurrency,
    decimal? WeightKg = null,
    string? PackageDescription = null,
    string? PackageContentCategory = null,
    decimal? DeclaredValue = null,
    PaymentSettlementType SettlementType = PaymentSettlementType.WalletEscrow,
    DirectCashTiming? CashTiming = null
) : ICommand<Result<Guid>>;
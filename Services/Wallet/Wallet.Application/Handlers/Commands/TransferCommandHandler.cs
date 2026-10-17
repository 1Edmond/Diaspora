using Shared.Domain.Commons;
using Shared.Domain.Enums;
using Shared.Domain.Interfaces;
using Shared.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Application.Commands;
using Wallet.Application.Interfaces;
using Wallet.Application.Responses;
using Wallet.Domain.Events;

namespace Wallet.Application.Handlers.Commands;

public sealed class TransferCommandHandler(
    IWalletRepository walletRepository,
    IWalletParameterRepository parameterRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<TransferResponse>> Handle(
        TransferCommand command,
        CancellationToken cancellationToken)
    {
        var sender = await walletRepository.GetByExternalProfileIdAsync(
            command.SenderExternalProfileId, cancellationToken);
        if (sender is null)
            return Result.Failure<TransferResponse>("Sender wallet not found.");

        var receiver = await walletRepository.GetByExternalProfileIdAsync(
            command.ReceiverExternalProfileId, cancellationToken);
        if (receiver is null)
            return Result.Failure<TransferResponse>("Receiver wallet not found.");

        if (sender.Currency != receiver.Currency)
            return Result.Failure<TransferResponse>(
                $"Currency mismatch: sender ({sender.Currency}) and receiver ({receiver.Currency}).");

        // ── Charger les paramètres de frais ───────────────────────────────────
        var feeTypeStr = await parameterRepository.GetStringAsync(
            Domain.Entities.WalletParameter.TransferFeeType, "Percentage", cancellationToken);
        var feeValue = await parameterRepository.GetDecimalAsync(
            Domain.Entities.WalletParameter.TransferFeeValue, 0m, cancellationToken);
        var feeMin = await parameterRepository.GetDecimalAsync(
            Domain.Entities.WalletParameter.TransferFeeMin, 0m, cancellationToken);
        var feeMax = await parameterRepository.GetDecimalAsync(
            Domain.Entities.WalletParameter.TransferFeeMax, 0m, cancellationToken);

        var feeType = Enum.TryParse<FeeType>(feeTypeStr, out var ft) ? ft : FeeType.Percentage;

        var fee = FeeCalculation.Calculate(
            command.Amount, sender.Currency, feeType, feeValue,
            feeMin > 0 ? feeMin : null,
            feeMax > 0 ? feeMax : null);

        // ── Débiter l'émetteur ────────────────────────────────────────────────
        var debitResult = sender.DebitForTransfer(
            command.Amount, fee, receiver.Id, command.Description);

        if (debitResult.IsFailure)
            return Result.Failure<TransferResponse>(debitResult.Error);

        // ── Créditer le destinataire ──────────────────────────────────────────
        var creditResult = receiver.CreditFromTransfer(
            command.Amount, sender.Id, command.Description);

        if (creditResult.IsFailure)
            return Result.Failure<TransferResponse>(creditResult.Error);

        sender.AddDomainEvent(new TransferCompletedDomainEvent(
            sender.Id, receiver.Id, debitResult.Value.transferTx.Id,
            command.Amount, fee.FeeAmount, sender.Currency));

        await walletRepository.UpdateAsync(sender, cancellationToken);
        await walletRepository.UpdateAsync(receiver, cancellationToken);

        if (fee.FeeAmount > 0)
        {
            var platformWallet = await walletRepository.GetPlatformRevenueWalletAsync(
                sender.Currency, cancellationToken);
            if (platformWallet is not null)
            {
                var feeCreditResult = platformWallet.CreditPlatformTransferFee(
                    fee.FeeAmount,
                    debitResult.Value.transferTx.Id,
                    $"Transfer fee for {command.Amount} {sender.Currency}");
                if (feeCreditResult.IsSuccess)
                {
                    await walletRepository.UpdateAsync(platformWallet, cancellationToken);
                }
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new TransferResponse(
            debitResult.Value.transferTx.Id,
            command.Amount,
            fee.FeeAmount,
            sender.Balance));
    }
}

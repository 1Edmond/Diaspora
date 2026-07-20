using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Application.Commands;

namespace Wallet.Application.Validators.Commands;

public sealed class TransferCommandValidator : AbstractValidator<TransferCommand>
{
    public TransferCommandValidator()
    {
        RuleFor(x => x.SenderExternalProfileId).NotEmpty();
        RuleFor(x => x.ReceiverExternalProfileId)
            .NotEmpty()
            .NotEqual(x => x.SenderExternalProfileId)
                .WithMessage("Cannot transfer to the same profile.");
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Transfer amount must be greater than zero.");
        RuleFor(x => x.Description)
            .NotEmpty().MaximumLength(500);
    }
}

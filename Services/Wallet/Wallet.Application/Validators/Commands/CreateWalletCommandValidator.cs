using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Application.Commands;

namespace Wallet.Application.Validators.Commands;

public sealed class CreateWalletCommandValidator : AbstractValidator<CreateWalletCommand>
{
    public CreateWalletCommandValidator()
    {
        RuleFor(x => x.ExternalProfileId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3, 10).WithMessage("Currency code must be between 3 and 10 characters.");
    }
}
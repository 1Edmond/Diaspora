using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Application.Commands;

namespace Wallet.Application.Validators.Commands;

public sealed class UpdateWalletParameterCommandValidator
    : AbstractValidator<UpdateWalletParameterCommand>
{
    public UpdateWalletParameterCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Value).NotEmpty().MaximumLength(500);
    }
}
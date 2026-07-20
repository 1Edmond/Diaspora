using Chat.Application.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Validators.Commands;

public sealed class CreateBroadcastCommandValidator
    : AbstractValidator<CreateBroadcastCommand>
{
    public CreateBroadcastCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CreatedByChatProfileId).NotEmpty();
    }
}
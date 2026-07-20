using Chat.Application.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Validators.Commands;

public sealed class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description != null);
        RuleFor(x => x.CreatedByChatProfileId).NotEmpty();
    }
}
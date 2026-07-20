using Chat.Application.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Validators.Commands;

public sealed class CreateDirectMessageCommandValidator
    : AbstractValidator<CreateDirectMessageCommand>
{
    public CreateDirectMessageCommandValidator()
    {
        RuleFor(x => x.InitiatorChatProfileId).NotEmpty();
        RuleFor(x => x.RecipientChatProfileId)
            .NotEmpty()
            .NotEqual(x => x.InitiatorChatProfileId)
                .WithMessage("Cannot create a DM with yourself.");
    }
}
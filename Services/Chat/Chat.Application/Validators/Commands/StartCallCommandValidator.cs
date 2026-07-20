using Chat.Application.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Validators.Commands;

public sealed class StartCallCommandValidator : AbstractValidator<StartCallCommand>
{
    public StartCallCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.InitiatorChatProfileId).NotEmpty();
    }
}
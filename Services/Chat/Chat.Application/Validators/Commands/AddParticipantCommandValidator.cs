using Chat.Application.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Validators.Commands;

public sealed class AddParticipantCommandValidator : AbstractValidator<AddParticipantCommand>
{
    public AddParticipantCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.RequestingChatProfileId).NotEmpty();
        RuleFor(x => x.ChatProfileId).NotEmpty();
    }
}
using Chat.Application.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Validators.Commands;

public sealed class CreateMentionGroupCommandValidator
    : AbstractValidator<CreateMentionGroupCommand>
{
    public CreateMentionGroupCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.RequestingChatProfileId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50)
            .Matches(@"^[a-zA-Z0-9_-]+$")
            .WithMessage("Name can only contain letters, numbers, underscores and hyphens.");
    }
}
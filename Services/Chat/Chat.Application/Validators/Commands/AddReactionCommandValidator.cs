using Chat.Application.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Validators.Commands;

public sealed class AddReactionCommandValidator : AbstractValidator<AddReactionCommand>
{
    public AddReactionCommandValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty();
        RuleFor(x => x.ChatProfileId).NotEmpty();
        RuleFor(x => x.Emoji).NotEmpty().MaximumLength(10);
    }
}

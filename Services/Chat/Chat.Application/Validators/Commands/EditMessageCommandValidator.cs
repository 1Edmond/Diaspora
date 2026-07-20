using Chat.Application.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Validators.Commands;

public sealed class EditMessageCommandValidator : AbstractValidator<EditMessageCommand>
{
    public EditMessageCommandValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty();
        RuleFor(x => x.RequestingChatProfileId).NotEmpty();
        RuleFor(x => x.NewContent).NotEmpty().MaximumLength(4000);
    }
}
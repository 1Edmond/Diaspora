using Chat.Application.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Validators.Commands;

public sealed class CreateChatProfileCommandValidator
    : AbstractValidator<CreateChatProfileCommand>
{
    public CreateChatProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BotToken)
            .NotEmpty().When(x => x.IsBot)
            .WithMessage("BotToken is required for bots.");
    }
}
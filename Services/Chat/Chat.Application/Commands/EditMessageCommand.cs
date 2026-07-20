using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Commands;

public sealed record EditMessageCommand(
    Guid MessageId,
    Guid RequestingChatProfileId,
    string NewContent
) : ICommand<Result>;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Commands;

public sealed record DeleteMessageCommand(
    Guid MessageId,
    Guid RequestingChatProfileId,
    bool IsAdmin = false
) : ICommand<Result>;
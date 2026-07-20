using Chat.Domain.Enums;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Commands;

public sealed record EndCallCommand(
    Guid CallId,
    Guid RequestingChatProfileId,
    CallStatus FinalStatus = CallStatus.Ended
) : ICommand<Result>;
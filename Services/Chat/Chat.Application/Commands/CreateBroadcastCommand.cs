using Chat.Application.Responses;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Commands;

public sealed record CreateBroadcastCommand(
    string Title,
    string? Description,
    Guid CreatedByChatProfileId
) : ICommand<Result<CreateBroadcastResponse>>;
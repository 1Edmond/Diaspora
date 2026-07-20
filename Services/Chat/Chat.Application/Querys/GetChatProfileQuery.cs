using Chat.Application.Dtos;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Querys;

public sealed record GetChatProfileQuery(Guid ChatProfileId)
    : IQuery<Result<ChatProfileDto>>;

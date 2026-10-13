using Chat.Application.Dtos;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Chat.Application.Querys;

/// <summary>
/// Résout le ChatProfileId à partir d'un ExternalProfileId (Profile Service), pour permettre
/// à d'autres services (Marketplace, Freelance...) d'ouvrir une conversation entre deux
/// utilisateurs qu'ils ne connaissent que par leur ExternalProfileId.
/// </summary>
public sealed record GetChatProfileByExternalProfileIdQuery(Guid ExternalProfileId)
    : IQuery<Result<ChatProfileDto>>;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Commands;

/// <summary>
/// Enregistre l'identifiant du fil de discussion (Chat Service) associé à une demande de service.
/// Le fil est en général créé par le Chat Service à la réception de ServiceRequestAcceptedIntegrationEvent ;
/// cette commande permet aussi une création manuelle depuis l'app mobile/API si besoin.
/// </summary>
public record LinkServiceRequestChatThreadCommand(
    Guid ServiceRequestId,
    Guid RequestedBy,
    Guid ChatThreadId
) : ICommand<Result>;
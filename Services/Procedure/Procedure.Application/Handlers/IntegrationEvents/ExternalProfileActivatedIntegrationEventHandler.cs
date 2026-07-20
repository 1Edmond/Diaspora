using Procedure.Application.Interfaces;
using Procedure.Domain.Entities;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events.Profile;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Handlers.IntegrationEvents;

public class ExternalProfileActivatedIntegrationEventHandler(IProcedureRepository procedureRepository, IUserProcedureRepository userProcedureRepository, IUnitOfWork unitOfWork)
{
    private readonly IProcedureRepository procedureRepository = procedureRepository;
    private readonly IUserProcedureRepository userProcedureRepository = userProcedureRepository;
    private readonly IUnitOfWork unitOfWork = unitOfWork;

    public async Task Handle(ExternalProfileActivatedIntegrationEvent message, CancellationToken cancellationToken)
    {
        var externalTypeProfileProcedures = await procedureRepository.FindAsync(
            p => p.ProfileTypeId == message.ProfileTypeId
            );

        var userProcedures = externalTypeProfileProcedures.Select(
            x => UserProcedure.Create(message.ProfileId, x.Id).Value
            );

        await userProcedureRepository.AddRangeAsync(userProcedures.ToList(), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

    }
}

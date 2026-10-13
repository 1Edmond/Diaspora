using Npgsql.Replication;
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

    public async Task Handle(ProfileActivatedIntegrationEvent message, CancellationToken cancellationToken)
    {
        
        var externalTypeProfileProcedures = await procedureRepository.FindAsync(
            p => p.ProfileTypeId == message.ProfileTypeId);

        var existUserProcedures = await userProcedureRepository.FindAsync(p => p.ProfileId == message.ProfileId);

        var existingProcedureIds = existUserProcedures.Select(p => p.ProcedureId).ToHashSet();
        
        var userProcedures = externalTypeProfileProcedures
        .Where(x => !existingProcedureIds.Contains(x.Id))
        .Select(x => UserProcedure.Create(message.ProfileId, x.Id).Value);

        if (userProcedures.Any()) 
        {
            await userProcedureRepository.AddRangeAsync(userProcedures.ToList(), cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

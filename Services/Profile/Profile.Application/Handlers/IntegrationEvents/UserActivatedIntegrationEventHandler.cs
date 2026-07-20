using Profile.Application.Commands;
using Profile.Application.Interfaces;
using Profile.Domain.Entities;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Handlers.IntegrationEvents;

public class UserActivatedIntegrationEventHandler(IInternalProfileTypeRepository internalProfileTypeRepository)
{

    public async Task<CreateInternalProfileCommand?> HandleAsync(UserActivatedIntegrationEvent message, CancellationToken cancellationToken)
    {
        var internalProfileType = await internalProfileTypeRepository.GetByPhoneIndicatifAsync(message.PhoneNumber.CountryCode, cancellationToken);

        if (internalProfileType == null)
            return null;

        return new CreateInternalProfileCommand(
            message.UserId,
            internalProfileType.Id,
            message.PhoneNumber,
            message.FirstName,
            message.LastName,
            message.DateOfBirth
            );
    }


}

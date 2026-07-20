using Profile.Application.Commands;
using Profile.Application.Interfaces;
using Profile.Domain.Entities;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Handlers.IntegrationEvents;

public class UserActivatedIntegrationEventHandler(IProfileTypeRepository internalProfileTypeRepository)
{

    public async Task<CreateProfileCommand?> HandleAsync(UserActivatedIntegrationEvent message, CancellationToken cancellationToken)
    {
        var profileType = await internalProfileTypeRepository.GetByPhoneIndicatifAsync(message.PhoneNumber.CountryCode, cancellationToken);

        if (profileType == null)
            return null;

        return new CreateProfileCommand(
            message.UserId,
            profileType.Id,
            message.PhoneNumber,
            message.FirstName,
            message.LastName,
            message.DateOfBirth,
            Domain.Enums.ProfileKind.Boursier
            );
    }


}

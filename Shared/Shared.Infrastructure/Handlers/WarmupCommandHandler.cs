using Microsoft.EntityFrameworkCore;
using Shared.Application.Commands;
using Shared.Application.Interfaces;
using Shared.Domain.Interfaces;
using Shared.Infrastructure.Inbox;
using Shared.Infrastructure.Outbox;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Infrastructure.Handlers;

public class WarmupCommandHandler
{
    public static async Task Handle(
        WarmupCommand command,
        IRepository<InboxMessage> repository,
        IHashingService hashingService,
        IJwtTokenService jwtService,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        await repository.GetAllAsync();
    }
}

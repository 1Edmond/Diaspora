using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Commands;

public record RemoveFavoriteCommand(Guid ListingId, Guid UserId) : ICommand<Result>;
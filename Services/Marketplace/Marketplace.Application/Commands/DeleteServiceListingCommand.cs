using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Commands;

public record DeleteServiceListingCommand(Guid Id, Guid RequestedBy) : ICommand<Result>;

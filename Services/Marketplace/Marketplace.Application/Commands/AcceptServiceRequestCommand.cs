using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Commands;

public record AcceptServiceRequestCommand(Guid Id, Guid ProviderId) : ICommand<Result>;


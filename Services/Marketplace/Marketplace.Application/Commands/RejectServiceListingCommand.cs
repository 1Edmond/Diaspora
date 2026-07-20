using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Commands;

public record RejectServiceListingCommand(Guid Id, string Reason) : ICommand<Result>;


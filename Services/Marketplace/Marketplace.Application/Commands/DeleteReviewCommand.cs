using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Commands;

public record DeleteReviewCommand(Guid Id, Guid RequestedBy, bool IsAdmin = false) : ICommand<Result>;

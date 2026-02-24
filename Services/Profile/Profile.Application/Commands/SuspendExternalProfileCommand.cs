using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Commands;

public sealed record SuspendExternalProfileCommand(
    Guid ExternalProfileId
) : ICommand<Result>;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Responses.Commands;

public sealed record CreateExternalProfileResponse(Guid ProfileId);
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Responses.Commands;

public sealed record CreateProfileResponse(Guid ProfileId);
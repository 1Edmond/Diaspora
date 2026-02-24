using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Responses;

public sealed record StartUserProcedureResponse(Guid UserProcedureId);
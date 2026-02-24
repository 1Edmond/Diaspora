using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Requests;

public sealed record StartUserProcedureRequest(Guid ProfileId, Guid ProcedureId);
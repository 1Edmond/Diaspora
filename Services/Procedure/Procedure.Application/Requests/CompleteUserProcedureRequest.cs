using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Requests;

public sealed record CompleteUserProcedureRequest(Guid ProfileId ,string? Notes);
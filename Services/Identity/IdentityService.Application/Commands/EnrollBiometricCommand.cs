using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Commands;

public record EnrollBiometricCommand(Guid UserId, string DeviceId, string PublicKey);
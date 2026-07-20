using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Commands;

public record RequestBiometricChallengeCommand(string Email, string DeviceId);
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Models;

public record BiometricChallenge(Guid UserId, string DeviceId, string Nonce, DateTime ExpiresAt);
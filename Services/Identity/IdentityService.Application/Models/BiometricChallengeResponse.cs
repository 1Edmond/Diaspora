using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Models;

public record BiometricChallengeResponse(string Nonce, DateTime ExpiresAt);
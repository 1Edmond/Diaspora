using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Commands;

public record BiometricLoginCommand(
    string Email,
    string DeviceId,
    string Nonce,
    string Signature,
    string IpAddress = "");
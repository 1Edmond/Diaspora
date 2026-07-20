using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Abstractions;

public interface IEd25519SignatureVerifier
{
    bool Verify(string publicKeyBase64, string message, string signatureBase64);
}


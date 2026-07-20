using IdentityService.Application.Abstractions;
using System;
using System.Collections.Generic;
using NSec.Cryptography;
using System.Text;

namespace IdentityService.Infrastructure.Services;

public class Ed25519SignatureVerifier : IEd25519SignatureVerifier
{
    public bool Verify(string publicKeyBase64, string message, string signatureBase64)
    {
        try
        {
            var publicKeyBytes = Convert.FromBase64String(publicKeyBase64);
            var signatureBytes = Convert.FromBase64String(signatureBase64);
            var messageBytes = System.Text.Encoding.UTF8.GetBytes(message);

            var algorithm = SignatureAlgorithm.Ed25519;
            var publicKey = PublicKey.Import(algorithm, publicKeyBytes, KeyBlobFormat.RawPublicKey);

            return algorithm.Verify(publicKey, messageBytes, signatureBytes);
        }
        catch
        {
            return false;
        }
    }
}
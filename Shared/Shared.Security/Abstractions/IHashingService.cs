using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Security.Abstractions;

public interface IHashingService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    string HashData(string data);
}
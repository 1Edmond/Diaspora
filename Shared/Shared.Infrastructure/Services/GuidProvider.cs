using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Infrastructure.Services;

public class GuidProvider : IGuidProvider
{
    public Guid NewGuid() => Guid.NewGuid();
}
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Infrastructure.Extensions;

public static class DbContextExtensions
{
    public static void ClearChangeTracker(this DbContext context)
    {
        context.ChangeTracker.Clear();
    }

    public static async Task<bool> CanConnectAsync(
        this DbContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.Database.CanConnectAsync(cancellationToken);
        }
        catch
        {
            return false;
        }
    }
}

using Freelance.Application.Abstractions;
using Freelance.Application.Dtos;
using Freelance.Application.Queries;
using Freelance.Application.Extensions;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Handlers.Queries;

public class GetMyJobPreferencesQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<WorkerJobPreferenceDto>> Handle(GetMyJobPreferencesQuery query)
    {
        var preference = await _context.WorkerJobPreferences
            .FirstOrDefaultAsync(p => p.WorkerId == query.WorkerId);

        if (preference is null)
            return Result.Failure<WorkerJobPreferenceDto>("Preferences not found.");

        return Result.Success(preference.MapToDto());
    }
}








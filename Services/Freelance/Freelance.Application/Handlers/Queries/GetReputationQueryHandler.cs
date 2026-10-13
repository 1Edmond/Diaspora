using Freelance.Application.Abstractions;
using Freelance.Application.Dtos;
using Freelance.Application.Extensions;
using Freelance.Application.Queries;
using Freelance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Handlers.Queries;

public class GetReputationQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<ReputationDto>> Handle(GetReputationQuery query)
    {
        var reputation = await _context.FreelanceReputations
            .FirstOrDefaultAsync(r => r.SubjectId == query.SubjectId && r.Role == query.Role);

        if (reputation is null)
            return Result.Success(new ReputationDto(
                query.SubjectId, query.Role, 0, 0, 0, 0, 0, 0));

        return Result.Success(reputation.MapToDto());
    }
}







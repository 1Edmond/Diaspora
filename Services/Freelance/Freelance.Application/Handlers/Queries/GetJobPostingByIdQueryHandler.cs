using Freelance.Application.Abstractions;
using Freelance.Application.Dtos;
using Freelance.Application.Queries;
using Freelance.Application.Extensions;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Handlers.Queries;

public class GetJobPostingByIdQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<JobPostingDto>> Handle(GetJobPostingByIdQuery query)
    {
        var posting = await _context.JobPostings
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == query.Id);

        if (posting is null)
            return Result.Failure<JobPostingDto>("Job posting not found.");

        return Result.Success(posting.MapToDto());
    }
}








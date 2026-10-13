using Freelance.Application.Abstractions;
using Freelance.Application.Dtos;
using Freelance.Application.Queries;
using Freelance.Application.Extensions;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Handlers.Queries;

public class GetJobApplicationByIdQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<JobApplicationDto>> Handle(GetJobApplicationByIdQuery query)
    {
        var application = await _context.JobApplications
            .Include(a => a.Posting)
            .FirstOrDefaultAsync(a => a.Id == query.Id);

        if (application is null)
            return Result.Failure<JobApplicationDto>("Application not found.");

        if (application.WorkerId != query.RequestedBy && application.Posting.EmployerId != query.RequestedBy)
            return Result.Failure<JobApplicationDto>("Access denied.");

        return Result.Success(application.MapToDto());
    }
}








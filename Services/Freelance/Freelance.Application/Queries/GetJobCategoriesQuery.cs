using Freelance.Application.Dtos;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Queries;

public record GetJobCategoriesQuery(bool ActiveOnly = true, Guid? ProfileTypeId = null) : IQuery<Result<List<JobCategoryDto>>>;
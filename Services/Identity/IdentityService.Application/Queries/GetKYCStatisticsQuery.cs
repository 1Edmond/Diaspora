using IdentityService.Application.Dtos;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Queries;

public record GetKYCStatisticsQuery : IQuery<Result<KYCStatisticsDto>>;


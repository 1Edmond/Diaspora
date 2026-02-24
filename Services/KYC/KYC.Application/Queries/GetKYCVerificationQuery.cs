using KYC.Application.Dtos;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Queries;

public record GetKYCVerificationQuery(Guid VerificationId) : IQuery<Result<KYCVerificationDto>>;


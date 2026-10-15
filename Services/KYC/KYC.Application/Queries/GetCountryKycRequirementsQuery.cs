using System;

namespace KYC.Application.Queries;

public sealed record GetCountryKycRequirementsQuery(Guid? ProfileTypeId = null);

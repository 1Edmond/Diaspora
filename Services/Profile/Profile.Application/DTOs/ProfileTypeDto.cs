using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.DTOs;

public sealed record ProfileTypeDto(
    Guid Id,
    string CountryName,
    string CountryCode,
    string PhoneIndicatif,
    string Currency,
    bool IsActive
);

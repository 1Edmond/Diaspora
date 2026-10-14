using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Requests;

public sealed record CreateProfileTypeRequest(
    string CountryName,
    string CountryCode,
    string PhoneIndicatif,
    string Currency,
    string? Flag = null
);

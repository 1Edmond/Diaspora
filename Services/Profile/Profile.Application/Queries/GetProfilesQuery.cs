using Shared.Application.Models;
using Profile.Application.DTOs;
using Profile.Domain.Enums;
using System;

namespace Profile.Application.Queries;

public record GetProfilesQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    ProfileStatus? Status = null,
    Guid? ProfileTypeId = null
);

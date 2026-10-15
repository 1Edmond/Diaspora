using Shared.Domain.Enums;

namespace Profile.Application.DTOs;

public sealed record SetProfileVerificationLevelRequest(VerificationLevel Level);

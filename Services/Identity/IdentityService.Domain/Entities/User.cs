using Shared.Domain.Commons;
using Shared.Domain.Enums;
using Shared.Domain.Primitives;
using Shared.Domain.ValueObjects;
using IdentityService.Domain.Events;
using Shared.Domain.Attributes;
using Shared.Domain.Interfaces;

namespace IdentityService.Domain.Entities;

public class User : AggregateRoot, ISoftDeletable, IAuditable
{
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserStatus Status { get; private set; }


    public bool EmailVerified { get; private set; }
    public string? EmailVerificationCode { get; private set; }
    public DateTime? EmailVerificationCodeExpiry { get; private set; }


    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public string? DeviceId {get; private set; }
    public string? DevicePublicKeyId { get; private set; }


    [SensitiveData]
    public EncryptedString? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public DateOnly DateOfBirth { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    public KYCLevel KYCLevel { get; private set; }
    public DateTime? KYCSubmittedAt { get; private set; }
    public DateTime? KYCApprovedAt { get; private set; }
    public string DataLanguage { get; private set; } = "fr";
    public string InterfaceLanguage { get; private set; } = "fr";
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    private User() { }

    public static Result<User> Create(
        string email,
        string passwordHash,
        DateOnly dateOfBirth,
        string? firstName = null,
        string? lastName = null,
        string? phoneNumber = null
        )
    {
        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
            return Result.Failure<User>(emailResult.Error);

        PhoneNumber? phone = null;
        if (!string.IsNullOrEmpty(phoneNumber))
        {
            var phoneResult = PhoneNumber.Create(phoneNumber);
            if (phoneResult.IsFailure)
                return Result.Failure<User>(phoneResult.Error);
            phone = phoneResult.Value;
        }

        if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-15)))
            return Result.Failure<User>("Bad date of birth");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = emailResult.Value,
            PasswordHash = passwordHash,
            Status = UserStatus.Inactive,
            EmailVerified = false,
            DateOfBirth = dateOfBirth,
            EmailVerificationCode = GenerateVerificationCode(),
            EmailVerificationCodeExpiry = DateTime.UtcNow.AddHours(24),
            FirstName = EncryptedString.FromClear(firstName!),
            LastName = lastName,
            PhoneNumber = phone,
            DataLanguage = "fr",
            InterfaceLanguage = "fr",
            FailedLoginAttempts = 0,
            CreatedAt = DateTime.UtcNow
        };

        user.AddDomainEvent(new UserCreatedEvent(
            user.Id,
            user.Email.Value,
            user.EmailVerificationCode));

        return Result.Success(user);
    }

  



    public Result UpdateKYCLevel(KYCLevel level)
    {
        if (IsDeleted)
            return Result.Failure("Cannot update KYC level for deleted user");

        KYCLevel = level;
        
        if (level != KYCLevel.None)
        {
            KYCApprovedAt = DateTime.UtcNow;
        }
        else
        {
            KYCApprovedAt = null;
        }

        Status = UserStatus.Active;
        AddDomainEvent(new UserKYCLevelUpdatedEvent(Id, Email.Value, level));

        return Result.Success();
    }

    public void RecordKYCSubmission()
    {
        KYCSubmittedAt = DateTime.UtcNow;
        AddDomainEvent(new UserKYCSubmittedEvent(Id, Email.Value));
    }
    public void Delete(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        foreach (var token in _refreshTokens.Where(t => t.IsActive))
        {
            token.Revoke();
        }
        AddDomainEvent(new UserDeletedEvent(Id.ToString(), Email.Value, deletedBy));
    }


    public Result VerifyEmail(string code)
    {
        if (EmailVerified)
            return Result.Failure("Email already verified");

        if (EmailVerificationCode != code)
        {
            return Result.Failure("Invalid verification code");
        }

        if (EmailVerificationCodeExpiry < DateTime.UtcNow)
        {
            return Result.Failure("Verification code expired");
        }

        EmailVerified = true;
        EmailVerificationCode = null;
        EmailVerificationCodeExpiry = null;

        if (Status == UserStatus.Inactive)
            Status = UserStatus.Active;

        AddDomainEvent(new EmailVerifiedEvent(Id, Email.Value));

        return Result.Success();
    }

    public Result<string> RegenerateVerificationCode()
    {
        if (EmailVerified)
            return Result.Failure<string>("Email already verified");

        EmailVerificationCode = GenerateVerificationCode();
        EmailVerificationCodeExpiry = DateTime.UtcNow.AddHours(24);

        return Result.Success(EmailVerificationCode);
    }

    public Result RecordSuccessfulLogin()
    {
        if (!EmailVerified)
            return Result.Failure("Email not verified");

        if (Status != UserStatus.Active)
            return Result.Failure($"User account is {Status}");

        if (IsLockedOut())
            return Result.Failure($"Account locked until {LockedUntil}");

        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockedUntil = null;

        AddDomainEvent(new UserLoggedInEvent(Id, Email.Value, DateTime.UtcNow));

        return Result.Success();
    }

    public Result RecordFailedLogin()
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= 5)
        {
            LockedUntil = DateTime.UtcNow.AddMinutes(15);
            AddDomainEvent(new UserLockedOutEvent(Id, Email.Value, LockedUntil.Value));
            return Result.Failure("Account locked due to too many failed attempts");
        }

        return Result.Success();
    }

    public bool IsLockedOut()
    {
        return LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;
    }

    public Result SetBiometriqueData(string deviceId, string publicKey)
    {
        if (string.IsNullOrEmpty(deviceId) || string.IsNullOrEmpty(publicKey))
            return Result.Failure("Information needed to set this feature");
        
        DeviceId = deviceId;
        DevicePublicKeyId = publicKey;
        
        return Result.Success();
    }

    public Result ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;

        foreach (var token in _refreshTokens.Where(t => t.IsActive))
        {
            token.Revoke();
        }

        AddDomainEvent(new PasswordChangedEvent(Id, Email.Value));

        return Result.Success();
    }

    public Result Suspend(string reason)
    {
        if (Status == UserStatus.Suspended)
            return Result.Failure("User already suspended");

        Status = UserStatus.Suspended;

        foreach (var token in _refreshTokens.Where(t => t.IsActive))
        {
            token.Revoke();
        }

        AddDomainEvent(new UserSuspendedEvent(Id, Email.Value, reason));

        return Result.Success();
    }

    public Result Activate()
    {
        if (Status == UserStatus.Active)
            return Result.Failure("User is already active");

        Status = UserStatus.Active;

        AddDomainEvent(new UserActivatedEvent(
            Id,
            Email.Value,
            PhoneNumber,
            FirstName?.ClearValue ?? string.Empty,
            LastName ?? string.Empty,
            DateOfBirth));

        return Result.Success();
    }

    public Result SetLanguagePreferences(string dataLanguage, string interfaceLanguage)
    {
        if (string.IsNullOrWhiteSpace(dataLanguage) || dataLanguage.Length > 10)
            return Result.Failure("Invalid data language code.");

        if (string.IsNullOrWhiteSpace(interfaceLanguage) || interfaceLanguage.Length > 10)
            return Result.Failure("Invalid interface language code.");

        DataLanguage = dataLanguage.Trim().ToLowerInvariant();
        InterfaceLanguage = interfaceLanguage.Trim().ToLowerInvariant();

        return Result.Success();
    }

    public void Update(string? firstName, string? lastName)
    {
        if (!string.IsNullOrWhiteSpace(firstName))
            FirstName = EncryptedString.FromClear(firstName);

        if (!string.IsNullOrWhiteSpace(lastName))
            LastName = lastName;

        //if (phoneNumber != null)
        //    PhoneNumber = phoneNumber;

        AddDomainEvent(new UserUpdatedEvent(Id, Email.Value));
    }

    public RefreshToken AddRefreshToken(string token, string ipAddress, DateTime expires)
    {
        var refreshToken = RefreshToken.Create(token, ipAddress, expires);
        _refreshTokens.Add(refreshToken);
        return refreshToken;
    }

    public Result RevokeRefreshToken(string token, string ipAddress, string? replacedByToken = null)
    {
        var refreshToken = _refreshTokens.FirstOrDefault(t => t.Token == token);

        if (refreshToken == null || !refreshToken.IsActive)
            return Result.Failure("Invalid refresh token");

        refreshToken.Revoke(ipAddress, replacedByToken);

        return Result.Success();
    }

    public void RevokeAllActiveTokens(string ipAddress, string reason = "Revoked")
    {
        foreach (var token in _refreshTokens.Where(t => t.IsActive))
        {
            token.Revoke(ipAddress, reason);
        }
    }

    public void RemoveOldRefreshTokens(int ttlDays)
    {
        _refreshTokens.RemoveAll(t =>
            !t.IsActive &&
            t.CreatedAt.AddDays(ttlDays) <= DateTime.UtcNow);
    }

    private static string GenerateVerificationCode()
    {
        return System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
    }


    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;

        AddDomainEvent(new UserRestoredEvent(Id.ToString(), Email.Value));
    }
}

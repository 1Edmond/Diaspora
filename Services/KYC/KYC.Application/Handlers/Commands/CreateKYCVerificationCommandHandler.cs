using KYC.Application.Abstractions;
using KYC.Application.Commands;
using KYC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using Shared.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Handlers.Commands;

public class CreateKYCVerificationCommandHandler(IKYCDbContext _context)
{
    public async Task<Result<Guid>> Handle(CreateKYCVerificationCommand command)
    {
        var existing = await _context.KYCVerifications
            .FirstOrDefaultAsync(k => k.UserId == command.UserId && !k.IsDeleted);

        if (existing != null && existing.Status != KYCStatus.Rejected)
            return Result.Failure<Guid>("User already has an active KYC verification");

        var addressResult = Address.Create(
            command.Street,
            command.City,
            command.State,
            command.PostalCode,
            command.Country);

        if (addressResult.IsFailure)
            return Result.Failure<Guid>(addressResult.Error);

        var verificationResult = KYCVerification.Create(
            command.UserId,
            command.FirstName,
            command.LastName,
            command.DateOfBirth,
            command.Nationality,
            addressResult.Value,
            command.UserId.ToString());

        if (verificationResult.IsFailure)
            return Result.Failure<Guid>(verificationResult.Error);

        _context.KYCVerifications.Add(verificationResult.Value);
        await _context.SaveChangesAsync();

        return Result.Success(verificationResult.Value.Id);
    }
}
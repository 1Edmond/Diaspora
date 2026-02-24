using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Commands;

public record CreateKYCVerificationCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string Nationality,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country
) : ICommand<Result<Guid>>;

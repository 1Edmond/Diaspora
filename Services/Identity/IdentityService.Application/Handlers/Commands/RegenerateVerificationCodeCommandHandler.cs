using IdentityService.Application.Commands;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Handlers.Commands;

public class RegenerateVerificationCodeCommandHandler
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegenerateVerificationCodeCommandHandler> _logger;

    public RegenerateVerificationCodeCommandHandler(
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        ILogger<RegenerateVerificationCodeCommandHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(RegenerateVerificationCodeCommand command)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.Email.Value == command.Email);
        if (user == null)
        {
            _logger.LogWarning("Verification code regeneration attempted for non-existent email: {Email}", command.Email);
            return Result.Success();
        }

        var result = user.RegenerateVerificationCode();
        if (result.IsFailure)
            return Result.Failure(result.Error);

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // TODO: Envoyer l'email avec le nouveau code
        // await _emailService.SendVerificationEmailAsync(user.Email.Value, result.Value);

        _logger.LogInformation("Verification code regenerated for user {UserId}", user.Id);
        return Result.Success();
    }
}
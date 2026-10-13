using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Commands;

public record DeleteJobTemplateCommand(Guid Id, Guid RequestedBy) : ICommand<Result>;
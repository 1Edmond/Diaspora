using Shared.Application.Abstractions;
using Shared.Domain.Commons;

namespace Procedure.Application.Commands;

public sealed record DeleteLocationCommand(Guid Id) : ICommand<Result>;

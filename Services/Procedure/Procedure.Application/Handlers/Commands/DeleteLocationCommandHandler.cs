using Procedure.Application.Commands;
using Procedure.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;

namespace Procedure.Application.Handlers.Commands;

public sealed class DeleteLocationCommandHandler(
    ILocationRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        DeleteLocationCommand command,
        CancellationToken cancellationToken)
    {
        var location = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (location is null)
            return Result.Failure("Location not found.");

        await repository.DeleteAsync(location, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

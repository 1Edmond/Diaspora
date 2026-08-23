using Chat.Application.Commands;
using Chat.Application.Interfaces;
using Chat.Domain.Enums;
using Shared.Domain.Interfaces;

namespace Chat.Application.Handlers.Commands;

public sealed class UpdatePresenceCommandHandler(
    IChatProfileRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task Handle(UpdatePresenceCommand command, CancellationToken cancellationToken)
    {
        var profile = await repository.GetByIdAsync(command.ChatProfileId, cancellationToken);
        if (profile is null)
            return;

        profile.SetStatus(command.IsOnline ? ChatProfileStatus.Online : ChatProfileStatus.Offline);

        await repository.UpdateAsync(profile, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

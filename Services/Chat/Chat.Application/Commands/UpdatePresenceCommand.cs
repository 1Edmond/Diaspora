namespace Chat.Application.Commands;

public sealed record UpdatePresenceCommand(Guid ChatProfileId, bool IsOnline);

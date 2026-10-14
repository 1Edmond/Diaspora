using Shared.Application.Abstractions;
using Shared.Domain.Commons;

namespace Profile.Application.Commands;

public record RepublishProfileDirectoryCommand() : ICommand<Result<RepublishProfileDirectoryResponse>>;

public record RepublishProfileDirectoryResponse(int TotalProfilesProcessed, int TotalEventsPublished);

using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Commands;

public record AssignPermissionsCommand(List<string> PermissionKeys) : ICommand<Result>;

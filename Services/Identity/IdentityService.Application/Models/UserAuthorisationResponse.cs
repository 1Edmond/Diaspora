using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Models;

public class UserAuthorisationResponse
{
    public List<string> Roles { get; set; }
    public List<string> Permissions { get; set; }
}

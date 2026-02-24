using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Domain.Constants;

public static class DefaultRole
{
    public static List<string> GetDefaultRoles() => new List<string>
    {
        Admin,
        Moderator,
        User,
    };

    public static string Admin => "Admin";
    public static string Moderator => "Moderator";
    public static string User => "User";

    public static Guid GetRoleId(string roleName)
    {
        return roleName switch
        {
            "Admin" => Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "User" => Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Moderator" => Guid.Parse("33333333-3333-3333-3333-333333333333"),
            _ => throw new ArgumentException($"Unknown role name: {roleName}")
        };
    }

    public static string GetRoleName(Guid roleId)
    {
        return roleId switch
        {
            var id when id == Guid.Parse("11111111-1111-1111-1111-111111111111") => "Admin",
            var id when id == Guid.Parse("22222222-2222-2222-2222-222222222222") => "User",
            var id when id == Guid.Parse("33333333-3333-3333-3333-333333333333") => "Moderator",
            _ => throw new ArgumentException($"Unknown role ID: {roleId}")
        };
    }
}

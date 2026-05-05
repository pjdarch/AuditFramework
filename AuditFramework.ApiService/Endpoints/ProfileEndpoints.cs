using System.Security.Claims;
using AuditFramework.ApiService.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuditFramework.ApiService.Endpoints;

public static class ProfileEndpoints
{
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profile").RequireAuthorization();

        group.MapGet(
            "",
            async (ClaimsPrincipal principal, UserManager<ApplicationUser> users) =>
            {
                var user = await users.GetUserAsync(principal);
                if (user is null)
                    return Results.Unauthorized();
                return Results.Ok(new ProfileDto(user.Email, user.FirstName, user.LastName));
            }
        );

        group.MapPut(
            "",
            async (
                [FromBody] UpdateProfileRequest req,
                ClaimsPrincipal principal,
                UserManager<ApplicationUser> users
            ) =>
            {
                var user = await users.GetUserAsync(principal);
                if (user is null)
                    return Results.Unauthorized();
                user.FirstName = req.FirstName;
                user.LastName = req.LastName;
                var result = await users.UpdateAsync(user);
                return result.Succeeded
                    ? Results.Ok(new ProfileDto(user.Email, user.FirstName, user.LastName))
                    : Results.ValidationProblem(
                        result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description })
                    );
            }
        );

        return app;
    }
}

public record ProfileDto(string? Email, string? FirstName, string? LastName);

public record UpdateProfileRequest(string? FirstName, string? LastName);

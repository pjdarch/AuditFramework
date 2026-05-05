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
                return Results.Ok(
                    new ProfileDto(
                        user.Email,
                        user.FirstName,
                        user.LastName,
                        user.PhoneNumber,
                        user.DateOfBirth,
                        user.AddressLine1,
                        user.AddressLine2,
                        user.City,
                        user.StateOrProvince,
                        user.PostalCode,
                        user.Country
                    )
                );
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
                user.PhoneNumber = req.PhoneNumber;
                user.DateOfBirth = req.DateOfBirth;
                user.AddressLine1 = req.AddressLine1;
                user.AddressLine2 = req.AddressLine2;
                user.City = req.City;
                user.StateOrProvince = req.StateOrProvince;
                user.PostalCode = req.PostalCode;
                user.Country = req.Country;
                var result = await users.UpdateAsync(user);
                return result.Succeeded
                    ? Results.Ok(
                        new ProfileDto(
                            user.Email,
                            user.FirstName,
                            user.LastName,
                            user.PhoneNumber,
                            user.DateOfBirth,
                            user.AddressLine1,
                            user.AddressLine2,
                            user.City,
                            user.StateOrProvince,
                            user.PostalCode,
                            user.Country
                        )
                    )
                    : Results.ValidationProblem(
                        result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description })
                    );
            }
        );

        return app;
    }
}

public record ProfileDto(
    string? Email,
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? StateOrProvince,
    string? PostalCode,
    string? Country
);

public record UpdateProfileRequest(
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? StateOrProvince,
    string? PostalCode,
    string? Country
);

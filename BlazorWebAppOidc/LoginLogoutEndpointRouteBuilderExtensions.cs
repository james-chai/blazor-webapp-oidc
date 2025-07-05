using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Routing;

internal static class LoginLogoutEndpointRouteBuilderExtensions
{
    internal static IEndpointConventionBuilder MapLoginAndLogout(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("");

        group.MapGet("/login", (string? returnUrl) => TypedResults.Challenge(GetAuthProperties(returnUrl)))
            .AllowAnonymous();

        // Sign out of the Cookie and OIDC handlers. If you do not sign out with the OIDC handler,
        // the user will automatically be signed back in the next time they visit a page that requires authentication
        // without being able to choose another account.
        group.MapPost("/logout", async (
            [FromForm] string? returnUrl,
            HttpContext context,
            IConfiguration config) =>
        {
            // Local application sign-out
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var auth0Config = config.GetSection("Authentication:Schemes:Auth0");
            var domain = auth0Config["Domain"];

            var request = context.Request;
            returnUrl = $"{request.Scheme}://{request.Host}";

            // Auth0 federated logout URL
            var logoutUrl = $"{auth0Config["Domain"]}v2/logout?" +
                $"client_id={auth0Config["ClientId"]}&" +
                $"returnTo={Uri.EscapeDataString(returnUrl)}";

            // Redirect to Auth0 for global sign-out
            return Results.Redirect(logoutUrl);
        });

        return group;
    }

    private static AuthenticationProperties GetAuthProperties(string? returnUrl, bool isLogout = false)
    {
        // TODO: Use HttpContext.Request.PathBase instead.
        const string pathBase = "/";

        // Prevent open redirects.
        if (isLogout || string.IsNullOrEmpty(returnUrl))   // Redirect to the Home page after logout
        {
            returnUrl = pathBase;
        }
        else if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
        {
            returnUrl = new Uri(returnUrl, UriKind.Absolute).PathAndQuery;
        }
        else if (returnUrl[0] != '/')
        {
            returnUrl = $"{pathBase}{returnUrl}";
        }

        return new AuthenticationProperties { RedirectUri = returnUrl };
    }
}

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

        group.MapGet("/login", (HttpContext httpContext, string? returnUrl) =>
            TypedResults.Challenge(GetAuthProperties(httpContext, returnUrl)))
            .AllowAnonymous();

        // Sign out of the Cookie and OIDC handlers. If you do not sign out with the OIDC handler,
        // the user will automatically be signed back in the next time they visit a page that requires authentication
        // without being able to choose another account.
        group.MapPost("/logout", (HttpContext httpContext, [FromForm] string? returnUrl) =>
            TypedResults.SignOut(
                GetAuthProperties(httpContext, returnUrl, true),
                [CookieAuthenticationDefaults.AuthenticationScheme, "MicrosoftOidc"]));

        return group;
    }

    private static AuthenticationProperties GetAuthProperties(HttpContext httpContext, string? returnUrl, bool isLogout = false)
    {
        var pathBase = httpContext.Request.PathBase.HasValue
            ? httpContext.Request.PathBase.Value
            : "/";

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

using BlazorWebAppOidc;
using BlazorWebAppOidc.Client.Weather;
using BlazorWebAppOidc.Components;
using BlazorWebAppOidc.Weather;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

const string MS_OIDC_SCHEME = "MicrosoftOidc";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(MS_OIDC_SCHEME)
    .AddOpenIdConnect(MS_OIDC_SCHEME, options => ConfigureOpenIdConnect(options, builder.Configuration))
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

// ConfigureCookieOidc attaches a cookie OnValidatePrincipal callback to get
// a new access token when the current one expires, and reissue a cookie with the
// new access token saved inside. If the refresh fails, the user will be signed
// out. OIDC connect options are set for saving tokens and the offline access
// scope.
builder.Services.ConfigureCookieOidc(CookieAuthenticationDefaults.AuthenticationScheme, MS_OIDC_SCHEME);

builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped<AuthenticationStateProvider, PersistingAuthenticationStateProvider>();

builder.Services.AddScoped<IWeatherForecaster, ServerWeatherForecaster>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<TokenHandler>();

builder.Services.AddHttpClient("ExternalApi",
      client => client.BaseAddress = new Uri(builder.Configuration["ExternalApiUri"] ??
          throw new Exception("Missing base address!")))
      .AddHttpMessageHandler<TokenHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapGet("/api/weather-forecasts", ([FromServices] IWeatherForecaster WeatherForecaster) =>
{
    return WeatherForecaster.GetWeatherForecastAsync();
}).RequireAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorWebAppOidc.Client._Imports).Assembly);

app.MapGroup("/authentication").MapLoginAndLogout();

app.Run();


static void ConfigureOpenIdConnect(OpenIdConnectOptions options, IConfiguration configuration)
{
    var oidcConfig = configuration.GetSection("Authentication:Schemes:MicrosoftOidc");

    // ........................................................................
    // The OIDC handler must use a sign-in scheme capable of persisting 
    // user credentials across requests.
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // ........................................................................

    // The following example Authority is configured for Microsoft Entra ID
    // and a single-tenant application registration. Set the {TENANT ID} 
    // placeholder to the Tenant ID. The "common" Authority 
    // https://login.microsoftonline.com/common/v2.0/ should be used 
    // for multi-tenant apps. You can also use the "common" Authority for 
    // single-tenant apps, but it requires a custom IssuerValidator as shown 
    // in the comments below. 
    options.Authority = oidcConfig["Authority"];
    // ........................................................................

    // Set the Client ID for the app. Set the {CLIENT ID} placeholder to
    // the Client ID.
    options.ClientId = oidcConfig["ClientId"];
    options.ClientSecret = oidcConfig["ClientSecret"];
    // ........................................................................

    // Setting ResponseType to "code" configures the OIDC handler to use 
    // authorization code flow. Implicit grants and hybrid flows are unnecessary
    // in this mode. In a Microsoft Entra ID app registration, you don't need to 
    // select either box for the authorization endpoint to return access tokens 
    // or ID tokens. The OIDC handler automatically requests the appropriate 
    // tokens using the code returned from the authorization endpoint.

    options.ResponseType = OpenIdConnectResponseType.Code;

    // Set MapInboundClaims to "false" to obtain the original claim types from 
    // the token. Many OIDC servers use "name" and "role"/"roles" rather than 
    // the SOAP/WS-Fed defaults in ClaimTypes. Adjust these values if your 
    // identity provider uses different claim types.

    options.MapInboundClaims = false;
    options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
    options.TokenValidationParameters.RoleClaimType = "roles";
    // ........................................................................

    // ........................................................................
    // Many OIDC providers work with the default issuer validator, but the
    // configuration must account for the issuer parameterized with "{TENANT ID}" 
    // returned by the "common" endpoint's /.well-known/openid-configuration
    // For more information, see
    // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/issues/1731

    //var microsoftIssuerValidator = AadIssuerValidator.GetAadIssuerValidator(oidcOptions.Authority);
    //oidcOptions.TokenValidationParameters.IssuerValidator = microsoftIssuerValidator.Validate;
    // ........................................................................

    // ........................................................................
    // OIDC connect options set later via ConfigureCookieOidc
    //
    // (1) The "offline_access" scope is required for the refresh token.
    //
    // (2) SaveTokens is set to true, which saves the access and refresh tokens
    // in the cookie, so the app can authenticate requests for weather data and
    // use the refresh token to obtain a new access token on access token
    // expiration.
    // ........................................................................
    options.SaveTokens = true;

    // Scopes
    options.Scope.Clear();
    foreach (var scope in oidcConfig?.GetSection("Scopes").Get<string[]>() ?? [])
    {
        options.Scope.Add(scope);
    }
    // ........................................................................

    // ........................................................................
    // The following paths must match the redirect and post logout redirect 
    // paths configured when registering the application with the OIDC provider. 
    // The default values are "/signin-oidc" and "/signout-callback-oidc".
    options.CallbackPath = new PathString(oidcConfig["CallbackPath"]);
    options.SignedOutCallbackPath = new PathString(oidcConfig["SignedOutCallbackPath"]);
    // ........................................................................

    // ........................................................................
    // The RemoteSignOutPath is the "Front-channel logout URL" for remote single 
    // sign-out. The default value is "/signout-oidc".
    options.RemoteSignOutPath = new PathString(oidcConfig["RemoteSignOutPath"]);
    // ........................................................................

    // Token validation
    //options.TokenValidationParameters = new()
    //{
    //    NameClaimType = "name",
    //    RoleClaimType = "roles",
    //    ValidateIssuer = true
    //};

    // Optional: Add event handlers if needed
    //options.Events = new OpenIdConnectEvents
    //{
    //    OnAuthenticationFailed = context =>
    //    {
    //        context.Response.Redirect("/error");
    //        context.HandleResponse();
    //        return Task.CompletedTask;
    //    }
    //};
}

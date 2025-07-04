# Blazor Web App with OpenID Connect (OIDC)

This app is a customized version of a Microsoft sample project, enhanced with additional features and configurations.

* A Blazor Web App with global Auto interactivity.
* `PersistingAuthenticationStateProvider` and `PersistentAuthenticationStateProvider` services are added to the server and client Blazor apps respectively to capture authentication state and flow it between the server and client.
* OIDC authentication with Microsoft Entra without using Entra-specific packages. This sample can be used as a starting point for any OIDC authentication flow.
* Automatic non-interactive token refresh with the help of a custom `CookieOidcRefresher`.
* Secure web API call for weather data to a separate web API project. The access token is obtained from the server-side `HttpContext` and attached to outgoing requests with a custom `DelegatingHandler` service.

## Article for this app

The article for this sample app is [Secure an ASP.NET Core Blazor Web App with OpenID Connect (OIDC)](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-oidc?pivots=non-bff-pattern).

## Configure Me-ID

Configure the OIDC provider in the `Program.cs` file.

## Run the sample

### Visual Studio

1. Open the `BlazorWebAppOidc` solution file in Visual Studio.
1. Use the **Start Projects** launch profile to start the web API app and Blazor apps.

### .NET CLI

In a command shell:

* Navigate to the `MinimalApiJwt` project folder and use the `dotnet run` command to run the project.
* Navigate to the `BlazorWebAppOidc` project folder and use the `dotnet watch` command to run the project.

Register an application

Follow these steps to create the app registration in Microsoft Entra:

1. Sign in to the [Microsoft Entra admin center](https://entra.microsoft.com/#home) as at least an Application Developer.
2. If you have access to multiple tenants, use the Settings icon  in the top menu to switch to the tenant in which you want to register the application.
3. Browse to Entra ID > App registrations and select New registration.
4. Enter a meaningful Name for your, for example identity-client-app. App users can see this name, and it can be changed at any time. You can have multiple app registrations with the same name.
5. Under Supported account types, specify who can use the application. We recommend you select Accounts in this organizational directory only for most applications. Refer to the table for more information on each option.


### User serect example

```
{
  "Authentication:Schemes:MicrosoftOidc:ClientId": "01358d66-c678-*******************",
  "Authentication:Schemes:MicrosoftOidc:ClientSecret": "KM68Q~*****************************",
  "Authentication:Schemes:MicrosoftOidc:Authority": "https://login.microsoftonline.com/************************************/v2.0/"
}
```
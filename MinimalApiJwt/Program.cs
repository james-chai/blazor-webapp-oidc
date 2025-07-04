using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using MinimalApiJwt.Infrastructure;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

var oidcConfig = builder.Configuration.GetSection("OidcConfig");
var swaggerConfig = builder.Configuration.GetSection("Swagger");

builder.Services.AddCors(options =>
{
    options.AddPolicy("SwaggerPolicy", policy =>
    {
        policy.WithOrigins("https://localhost:7277")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Authentication
builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", options =>
    {
        // {TENANT ID} is the directory (tenant) ID.
        //
        // Authority format {AUTHORITY} matches the issurer (`iss`) of the JWT returned by the identity provider.
        //
        // Authority format {AUTHORITY} for ME-ID tenant type: https://sts.windows.net/{TENANT ID}/
        // Authority format {AUTHORITY} for B2C tenant type: https://login.microsoftonline.com/{TENANT ID}/v2.0/
        //
        options.Authority = oidcConfig["Authority"];
        //
        // The following should match just the path of the Application ID URI configured when adding the "Weather.Get" scope
        // under "Expose an API" in the Azure or Entra portal. {CLIENT ID} is the application (client) ID of this 
        // app's registration in the Azure portal.
        // 
        // Audience format {AUDIENCE} for ME-ID tenant type: api://{CLIENT ID}
        // Audience format {AUDIENCE} for B2C tenant type: https://{DIRECTORY NAME}.onmicrosoft.com/{CLIENT ID}
        //
        options.Audience = oidcConfig["Audience"];
    });

builder.Services.AddAuthorization();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = builder.Configuration["Swagger:Title"],
        Version = builder.Configuration["Swagger:Version"]
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Paste a valid access token (obtained separately)",
        In = ParameterLocation.Header
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    c.DocInclusionPredicate((name, api) => true);
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapEndpoints();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
}

app.Run();

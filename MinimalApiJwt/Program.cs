using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", jwtOptions =>
    {
        // {TENANT ID} is the directory (tenant) ID.
        //
        // Authority format {AUTHORITY} matches the issurer (`iss`) of the JWT returned by the identity provider.
        //
        // Authority format {AUTHORITY} for ME-ID tenant type: https://sts.windows.net/{TENANT ID}/
        // Authority format {AUTHORITY} for B2C tenant type: https://login.microsoftonline.com/{TENANT ID}/v2.0/
        //
        jwtOptions.Authority = "https://sts.windows.net/288313b1-6ad2-4ee1-9557-6bf3dbed703d/";
        //
        // The following should match just the path of the Application ID URI configured when adding the "Weather.Get" scope
        // under "Expose an API" in the Azure or Entra portal. {CLIENT ID} is the application (client) ID of this 
        // app's registration in the Azure portal.
        // 
        // Audience format {AUDIENCE} for ME-ID tenant type: api://{CLIENT ID}
        // Audience format {AUDIENCE} for B2C tenant type: https://{DIRECTORY NAME}.onmicrosoft.com/{CLIENT ID}
        //
        jwtOptions.Audience = "api://6f4685ed-3434-4678-b6d4-0f7bac089b6a";
    });
builder.Services.AddAuthorization();

// Add OpenApi 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Minimal API JWT", Version = "v1" });
});

var app = builder.Build();

// Configure Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
}

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weather-forecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
}).RequireAuthorization();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

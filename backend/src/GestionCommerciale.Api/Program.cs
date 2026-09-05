using GestionCommerciale.Api.Middleware;
using GestionCommerciale.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Les erreurs de validation des DTOs (ex: [Required], [EmailAddress]) renvoient
        // automatiquement un 400 avec le détail des champs invalides.
        options.SuppressModelStateInvalidFilter = false;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Gestion Commerciale API",
        Version = "v1",
        Description = "API de gestion des clients, produits et commandes."
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

var allowedOrigin = builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:4200";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
        policy.WithOrigins(allowedOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Gestion Commerciale API v1");
});

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");
app.UseAuthorization();
app.MapControllers();

app.Run();

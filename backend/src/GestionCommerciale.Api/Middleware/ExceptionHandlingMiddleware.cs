using System.Net;
using GestionCommerciale.Domain.Exceptions;

namespace GestionCommerciale.Api.Middleware;

/// <summary>
/// Centralise la gestion des erreurs : les BusinessException et NotFoundException
/// levées par la couche Application sont traduites en réponses HTTP cohérentes,
/// pour éviter les try/catch répétés dans chaque contrôleur.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BusinessException ex)
        {
            await WriteResponse(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await WriteResponse(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur non gérée");
            await WriteResponse(context, HttpStatusCode.InternalServerError, "Une erreur interne est survenue.");
        }
    }

    private static async Task WriteResponse(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsJsonAsync(new { message });
    }
}

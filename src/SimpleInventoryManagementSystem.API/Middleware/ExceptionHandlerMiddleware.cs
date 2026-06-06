using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SimpleInventoryManagementSystem.Domain.Exceptions;

namespace SimpleInventoryManagementSystem.API.Middleware;

public sealed class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (ValidationException ex) { await HandleValidationException(context, ex); }
        catch (InsufficientStockException ex) { await HandleDomainException(context, ex, 409, "Insufficient Stock"); }
        catch (ProductNotFoundException ex) { await HandleDomainException(context, ex, 404, "Not Found"); }
        catch (CustomerNotFoundException ex) { await HandleDomainException(context, ex, 404, "Not Found"); }
        catch (DomainException ex) { await HandleDomainException(context, ex, 400, "Bad Request"); }
        catch (Exception ex) { await HandleUnexpectedException(context, ex); }
    }

    private static async Task HandleValidationException(HttpContext context, ValidationException ex)
    {
        var errors = ex.Errors
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(f => f.ErrorMessage).ToArray());

        var problem = new ValidationProblemDetails(errors) { Status = 400 };

        await WriteProblemResponse(context, problem);
    }

    private static async Task HandleDomainException(HttpContext context, Exception ex, int statusCode, string title)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = ex.Message
        };

        await WriteProblemResponse(context, problem);
    }

    private async Task HandleUnexpectedException(HttpContext context, Exception ex)
    {
        logger.LogError(ex, "Unhandled exception occurred");

        var problem = new ProblemDetails
        {
            Status = 500,
            Title = "Internal Server Error"
        };

        await WriteProblemResponse(context, problem);
    }

    private static async Task WriteProblemResponse(HttpContext context, ProblemDetails problem)
    {
        context.Response.StatusCode = problem.Status ?? 500;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problem, problem.GetType(), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

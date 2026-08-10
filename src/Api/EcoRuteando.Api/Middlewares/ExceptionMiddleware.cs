
using EcoRuteando.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EcoRuteando.Api.Middlewares;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException exception)
        {
            await WriteProblemDetails(
                context,
                StatusCodes.Status400BadRequest,
                "Validation Error",
                exception.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage)));
        }
        catch (NotFoundException exception)
        {
            await WriteProblemDetails(
                context,
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                exception.Message);
        }
        catch (ConflictException exception)
        {
            await WriteProblemDetails(
                context,
                StatusCodes.Status409Conflict,
                "Conflict",
                exception.Message);
        }
        catch (UnauthorizedException exception)
        {
            await WriteProblemDetails(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                exception.Message);
        }
        catch (ForbiddenException exception)
        {
            await WriteProblemDetails(
                context,
                StatusCodes.Status403Forbidden,
                "Forbidden",
                exception.Message);
        }
        catch (Exception exception)
        {
            await WriteProblemDetails(
                context,
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                exception.Message);
        }
    }

    private static async Task WriteProblemDetails(
        HttpContext context,
        int statusCode,
        string title,
        object detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title
        };

        if (detail is string message)
        {
            problem.Detail = message;
        }

        if (detail is Dictionary<string, IEnumerable<string>> errors)
        {
            problem.Extensions["errors"] = errors;
        }

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problem));
    }
}
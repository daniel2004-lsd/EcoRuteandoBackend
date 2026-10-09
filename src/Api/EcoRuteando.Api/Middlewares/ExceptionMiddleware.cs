
using EcoRuteando.Shared.Exceptions;
using EcoRuteando.Modules.Security.Application.Abstractions.BackgroundJobs;
using EcoRuteando.Modules.Security.Application.Abstractions.Logging;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EcoRuteando.Api.Middlewares;

public sealed class ExceptionMiddleware
{
    private const string Source = "EcoRuteando.Api.Middlewares.ExceptionMiddleware";

    private readonly RequestDelegate _next;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        IBackgroundTaskQueue backgroundTaskQueue,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _backgroundTaskQueue = backgroundTaskQueue;
        _logger = logger;
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
            object detail = exception.Message;

            if (exception.AttemptsRemaining.HasValue
                || exception.RetryAfterSeconds.HasValue)
            {
                detail = new Dictionary<string, object?>
                {
                    ["message"] = exception.Message,
                    ["attemptsRemaining"] = exception.AttemptsRemaining,
                    ["retryAfterSeconds"] = exception.RetryAfterSeconds
                };
            }

            await WriteProblemDetails(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                detail);
        }
        catch (ForbiddenException exception)
        {
            await WriteProblemDetails(
                context,
                StatusCodes.Status403Forbidden,
                "Forbidden",
                exception.Message);
        }
        catch (DomainException exception)
        {
            await WriteProblemDetails(
                context,
                StatusCodes.Status400BadRequest,
                "Domain Error",
                exception.Message);
        }
        catch (Exception exception)
        {
            await LogErrorInBackgroundAsync(exception);

            await WriteProblemDetails(
                context,
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "Ocurrió un error interno. Intente de nuevo más tarde.");
        }
    }

    private async Task LogErrorInBackgroundAsync(Exception exception)
    {
        try
        {
            await _backgroundTaskQueue.EnqueueAsync<IErrorLogService>(logService =>
                logService.LogErrorAsync(
                    exception.Message,
                    exception,
                    source: Source));
        }
        catch
        {
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

        if (detail is IDictionary<string, object?> extras)
        {
            foreach (var pair in extras)
            {
                if (pair.Value is not null)
                {
                    problem.Extensions[pair.Key] = pair.Value;

                    if (pair.Key == "message")
                    {
                        problem.Detail = pair.Value?.ToString();
                    }
                }
            }
        }

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problem));
    }
}
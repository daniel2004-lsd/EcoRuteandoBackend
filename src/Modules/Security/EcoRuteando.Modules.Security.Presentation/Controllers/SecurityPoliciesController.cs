using EcoRuteando.Modules.Security.Application.SecurityPolicies.Commands.UpdateSecurityPolicy;
using EcoRuteando.Modules.Security.Application.SecurityPolicies.Queries.GetSecurityPolicy;
using EcoRuteando.Modules.Security.Presentation.Contracts.SecurityPolicy;
using EcoRuteando.Shared.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Security.Presentation.Controllers;

[ApiController]
[Route("api/security-policies")]
[Authorize]
public sealed class SecurityPoliciesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SecurityPoliciesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission("securitypolicies.read")]
    public async Task<IActionResult> GetSecurityPolicy(
        CancellationToken cancellationToken)
    {
        var policy = await _mediator.Send(
            new GetSecurityPolicyQuery(),
            cancellationToken);

        return Ok(policy);
    }

    [HttpPut]
    [HasPermission("securitypolicies.update")]
    public async Task<IActionResult> UpdateSecurityPolicy(
        UpdateSecurityPolicyRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSecurityPolicyCommand(
            request.MinPasswordLength,
            request.RequireUppercase,
            request.RequireNumbers,
            request.RequireSpecialChars,
            request.PasswordExpirationDays,
            request.MaxFailedAttempts,
            request.LockoutTimeMinutes,
            request.MaxActiveSessions);

        var policy = await _mediator.Send(command, cancellationToken);

        return Ok(policy);
    }
}

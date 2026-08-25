using EcoRuteando.Modules.Security.Application.Abstractions.Logging;
using EcoRuteando.Modules.Security.Application.SecurityPolicies.Queries.GetSecurityPolicy;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.SecurityPolicies.Commands.UpdateSecurityPolicy;

public sealed class UpdateSecurityPolicyCommandHandler
    : IRequestHandler<UpdateSecurityPolicyCommand, SecurityPolicyResponse>
{
    private readonly ISecurityPolicyRepository _securityPolicyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;

    public UpdateSecurityPolicyCommandHandler(
        ISecurityPolicyRepository securityPolicyRepository,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLogService)
    {
        _securityPolicyRepository = securityPolicyRepository;
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }

    public async Task<SecurityPolicyResponse> Handle(
        UpdateSecurityPolicyCommand request,
        CancellationToken cancellationToken)
    {
        var policy = await _securityPolicyRepository.GetAsync(cancellationToken);

        if (policy is null)
        {
            policy = new Domain.Entities.SecurityPolicy(
                request.MinPasswordLength,
                request.RequireUppercase,
                request.RequireNumbers,
                request.RequireSpecialChars,
                request.PasswordExpirationDays,
                request.MaxFailedAttempts,
                request.LockoutTimeMinutes,
                request.MaxActiveSessions);

            await _securityPolicyRepository.AddAsync(policy, cancellationToken);
        }
        else
        {
            policy.Update(
                request.MinPasswordLength,
                request.RequireUppercase,
                request.RequireNumbers,
                request.RequireSpecialChars,
                request.PasswordExpirationDays,
                request.MaxFailedAttempts,
                request.LockoutTimeMinutes,
                request.MaxActiveSessions);

            _securityPolicyRepository.Update(policy);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            null,
            "security_policy.updated",
            entityName: "security_policies",
            entityId: policy.Id.ToString(),
            cancellationToken: cancellationToken);

        return new SecurityPolicyResponse(
            policy.MinPasswordLength,
            policy.RequireUppercase,
            policy.RequireNumbers,
            policy.RequireSpecialChars,
            policy.PasswordExpirationDays,
            policy.MaxFailedAttempts,
            policy.LockoutTimeMinutes,
            policy.MaxActiveSessions,
            policy.UpdatedAt);
    }
}

using MediatR;

namespace EcoRuteando.Modules.Security.Application.SecurityPolicies.Queries.GetSecurityPolicy;

public sealed record GetSecurityPolicyQuery : IRequest<SecurityPolicyResponse>;

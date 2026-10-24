using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EcoRuteando.Modules.Security.Infrastructure.Security;

public sealed class TrustedDeviceService : ITrustedDeviceService
{
    private readonly JwtOptions _jwtOptions;
    private readonly ITrustedDeviceRepository? _trustedDeviceRepository;
    private readonly ISecurityUnitOfWork? _unitOfWork;

    public TrustedDeviceService(IOptions<JwtOptions> options)
        : this(options, null, null)
    {
    }

    public TrustedDeviceService(
        IOptions<JwtOptions> options,
        ITrustedDeviceRepository? trustedDeviceRepository,
        ISecurityUnitOfWork? unitOfWork)
    {
        _jwtOptions = options.Value;
        _trustedDeviceRepository = trustedDeviceRepository;
        _unitOfWork = unitOfWork;
    }

    public string GenerateTrustedDeviceToken(Guid userId, int days = 30)
    {
        return GenerateTrustedDeviceTokenAsync(userId, days).GetAwaiter().GetResult();
    }

    public async Task<string> GenerateTrustedDeviceTokenAsync(
        Guid userId,
        int days = 30,
        CancellationToken cancellationToken = default)
    {
        var deviceId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(days);

        if (_trustedDeviceRepository is not null)
        {
            var device = new TrustedDevice(
                id: deviceId,
                userId: userId,
                expiresAt: expiresAt);

            await _trustedDeviceRepository.AddAsync(device, cancellationToken);
        }

        if (_unitOfWork is not null)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("token_type", "trusted_device"),
            new Claim(JwtRegisteredClaimNames.Jti, deviceId.ToString())
        };

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));

        var signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidateTrustedDeviceToken(string? token, Guid userId)
    {
        return ValidateTrustedDeviceTokenAsync(token, userId).GetAwaiter().GetResult();
    }

    public async Task<bool> ValidateTrustedDeviceTokenAsync(
        string? token,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
            ValidateIssuer = !string.IsNullOrWhiteSpace(_jwtOptions.Issuer),
            ValidIssuer = _jwtOptions.Issuer,
            ValidateAudience = !string.IsNullOrWhiteSpace(_jwtOptions.Audience),
            ValidAudience = _jwtOptions.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            var subClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                        ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var typeClaim = principal.FindFirst("token_type")?.Value;
            var jtiClaim = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (typeClaim != "trusted_device" ||
                !Guid.TryParse(subClaim, out var tokenUserId) ||
                tokenUserId != userId ||
                !Guid.TryParse(jtiClaim, out var deviceId))
            {
                return false;
            }

            if (_trustedDeviceRepository is not null)
            {
                var device = await _trustedDeviceRepository.GetByIdAsync(deviceId, cancellationToken);
                if (device is null || device.UserId != userId || !device.IsActive)
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task RevokeAllForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (_trustedDeviceRepository is not null)
        {
            await _trustedDeviceRepository.RevokeAllByUserIdAsync(userId, cancellationToken);
        }

        if (_unitOfWork is not null)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

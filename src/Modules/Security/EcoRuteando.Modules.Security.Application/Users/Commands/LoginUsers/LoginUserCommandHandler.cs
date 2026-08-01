using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace EcoRuteando.Modules.Security.Application.Users.Commands.LoginUsers
{
    public sealed class LoginUserCommandHandler
     : IRequestHandler<LoginUserCommand, LoginResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtProvider _jwtProvider;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        public LoginUserCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IRefreshTokenService refreshTokenService,
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork,
            IJwtProvider jwtProvider)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtProvider = jwtProvider;
            _refreshTokenService = refreshTokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<LoginResponse> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(
                request.Email,
                cancellationToken);

            if (user is null)
            {
                throw new UnauthorizedException("Correo o contraseña incorrectos.");
            }

            var isPasswordValid = _passwordHasher.Verify(
                request.Password,
                user.PasswordHash);

            if (!isPasswordValid)
            {
                throw new UnauthorizedException("Correo o contraseña incorrectos.");
            }


            if (user.PrimaryRole is null)
            {
                throw new DomainException("El usuario no tiene un rol asignado.");
            }

            var accessToken = _jwtProvider.GenerateToken(
                user.Id,
                user.Email,
                user.PrimaryRole.Name);

            var refreshToken = _refreshTokenService.GenerateRefreshToken();

            var refreshTokenHash = _refreshTokenService.ComputeHash(refreshToken);

            var refreshTokenEntity = new EcoRuteando.Modules.Security.Domain.Entities.RefreshToken(
                user.Id,
                refreshTokenHash,
                DateTime.UtcNow.AddDays(7),
                null);

            await _refreshTokenRepository.AddAsync(
                refreshTokenEntity,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new LoginResponse(
                accessToken,
                refreshToken);
        }

    }
}

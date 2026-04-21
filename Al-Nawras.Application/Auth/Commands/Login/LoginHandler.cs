using Al_Nawras.Application.Auth.DTOs;
using Al_Nawras.Application.Auth.Interfaces;
using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Auth.Commands.Login
{
    public class LoginHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;

        public LoginHandler(
            IUserRepository userRepository,
            IAuthService authService,
            ITokenService tokenService,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _authService = authService;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<AuthResponse>> Handle(
            LoginCommand command,
            string ipAddress,
            CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);

            if (user is null || !user.IsActive)
                return Result<AuthResponse>.Failure("Invalid email or password.");

            if (!_authService.VerifyPassword(command.Password, user.PasswordHash))
                return Result<AuthResponse>.Failure("Invalid email or password.");

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddMinutes(60);

            var token = new RefreshToken(user.Id, refreshToken, DateTime.UtcNow.AddDays(7), ipAddress);

            // Add refresh token via EF navigation — no repo needed for this
            // (handled via AppDbContext directly in Infrastructure)
            user.RecordLogin();
            _userRepository.Update(user);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new AuthResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                ExpiresAt: expiresAt,
                UserId: user.Id,
                Email: user.Email,
                FirstName: user.FirstName,
                LastName: user.LastName,
                Role: user.Role.Name
            );

            return Result<AuthResponse>.Success(response);
        }
    }
}

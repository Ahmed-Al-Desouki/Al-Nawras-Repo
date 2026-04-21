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

namespace Al_Nawras.Application.Auth.Commands.Register
{
    public class RegisterHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterHandler(
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
            RegisterCommand command,
            string ipAddress,
            CancellationToken cancellationToken = default)
        {
            // Check duplicate email
            if (await _userRepository.ExistsByEmailAsync(command.Email, cancellationToken))
                return Result<AuthResponse>.Failure("An account with this email already exists.");

            // Validate password strength
            var passwordError = ValidatePassword(command.Password);
            if (passwordError is not null)
                return Result<AuthResponse>.Failure(passwordError);

            var passwordHash = _authService.HashPassword(command.Password);

            var user = new User(
                command.Email,
                passwordHash,
                command.FirstName,
                command.LastName,
                command.RoleId
            );

            await _userRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload with Role navigation populated
            var savedUser = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);

            var accessToken = _tokenService.GenerateAccessToken(savedUser);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var response = new AuthResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                ExpiresAt: DateTime.UtcNow.AddMinutes(60),
                UserId: savedUser.Id,
                Email: savedUser.Email,
                FirstName: savedUser.FirstName,
                LastName: savedUser.LastName,
                Role: savedUser.Role?.Name ?? ""
            );

            return Result<AuthResponse>.Success(response);
        }

        private static string? ValidatePassword(string password)
        {
            if (password.Length < 8)
                return "Password must be at least 8 characters.";
            if (!password.Any(char.IsUpper))
                return "Password must contain at least one uppercase letter.";
            if (!password.Any(char.IsDigit))
                return "Password must contain at least one number.";
            return null;
        }
    }
}

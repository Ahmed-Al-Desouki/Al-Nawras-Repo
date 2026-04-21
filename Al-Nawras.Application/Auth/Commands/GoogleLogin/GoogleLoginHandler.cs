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

namespace Al_Nawras.Application.Auth.Commands.GoogleLogin
{
    public class GoogleLoginHandler
    {
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;

        public GoogleLoginHandler(
            IGoogleAuthService googleAuthService,
            IUserRepository userRepository,
            ITokenService tokenService,
            IUnitOfWork unitOfWork)
        {
            _googleAuthService = googleAuthService;
            _userRepository = userRepository;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<AuthResponse>> Handle(
            GoogleLoginCommand command,
            CancellationToken cancellationToken = default)
        {
            // Verify the token with Google's servers
            var googleUser = await _googleAuthService.VerifyGoogleTokenAsync(command.IdToken);

            if (googleUser is null)
                return Result<AuthResponse>.Failure("Invalid or expired Google token.");

            // Find existing user by email
            var user = await _userRepository.GetByEmailAsync(googleUser.Email, cancellationToken);

            if (user is null)
            {
                // First time Google login — auto-register with Sales role
                user = new User(
                    googleUser.Email,
                    googleUser.FirstName,
                    googleUser.LastName,
                    googleUser.GoogleId,
                    googleUser.ProfilePictureUrl,
                    roleId: 2   // Sales by default — Admin can change later
                );

                await _userRepository.AddAsync(user, cancellationToken);
            }
            else
            {
                // Existing user — update Google profile info in case it changed
                user.UpdateGoogleProfile(googleUser.GoogleId, googleUser.ProfilePictureUrl);
                _userRepository.Update(user);
            }

            user.RecordLogin();
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload with Role navigation
            var freshUser = await _userRepository.GetByEmailAsync(googleUser.Email, cancellationToken);

            var accessToken = _tokenService.GenerateAccessToken(freshUser);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var response = new AuthResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                ExpiresAt: DateTime.UtcNow.AddMinutes(60),
                UserId: freshUser.Id,
                Email: freshUser.Email,
                FirstName: freshUser.FirstName,
                LastName: freshUser.LastName,
                Role: freshUser.Role?.Name ?? ""
            );

            return Result<AuthResponse>.Success(response);
        }
    }
}

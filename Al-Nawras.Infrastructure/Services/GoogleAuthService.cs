using Al_Nawras.Application.Auth.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Infrastructure.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;

        public GoogleAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<GoogleUserInfo?> VerifyGoogleTokenAsync(string idToken)
        {
            try
            {
                var clientId = _configuration["GoogleAuth:ClientId"];

                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                };

                // This hits Google's servers to validate the token signature
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                // Split name — Google gives a single displayName sometimes
                var nameParts = (payload.Name ?? "").Split(' ', 2);
                var firstName = nameParts.Length > 0 ? nameParts[0] : payload.Email;
                var lastName = nameParts.Length > 1 ? nameParts[1] : "";

                return new GoogleUserInfo(
                    GoogleId: payload.Subject,
                    Email: payload.Email,
                    FirstName: firstName,
                    LastName: lastName,
                    ProfilePictureUrl: payload.Picture
                );
            }
            catch (InvalidJwtException)
            {
                // Token invalid, expired, or wrong audience
                return null;
            }
        }
    }
}

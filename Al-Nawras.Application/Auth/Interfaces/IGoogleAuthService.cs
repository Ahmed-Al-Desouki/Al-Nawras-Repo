using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Auth.Interfaces
{
    public interface IGoogleAuthService
    {
        Task<GoogleUserInfo?> VerifyGoogleTokenAsync(string idToken);
    }

    public record GoogleUserInfo(
        string GoogleId,
        string Email,
        string FirstName,
        string LastName,
        string? ProfilePictureUrl
    );
}

using Al_Nawras.Application.Auth.Commands.GoogleLogin;
using Al_Nawras.Application.Auth.Commands.Login;
using Al_Nawras.Application.Auth.Commands.Register;
using Microsoft.AspNetCore.Mvc;

namespace Al_Nawras.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly LoginHandler _loginHandler;
        private readonly RegisterHandler _registerHandler;
        private readonly GoogleLoginHandler _googleLoginHandler;

        public AuthController(
            LoginHandler loginHandler,
            RegisterHandler registerHandler,
            GoogleLoginHandler googleLoginHandler)
        {
            _loginHandler = loginHandler;
            _registerHandler = registerHandler;
            _googleLoginHandler = googleLoginHandler;
        }

        /// <summary>Login with email and password</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginCommand command,
            CancellationToken cancellationToken)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _loginHandler.Handle(command, ip, cancellationToken);

            if (!result.IsSuccess)
                return Unauthorized(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>Register a new internal user with email and password</summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterCommand command,
            CancellationToken cancellationToken)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _registerHandler.Handle(command, ip, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Login or auto-register via Google.
        /// Frontend sends the Google ID token received after the user
        /// completes Google Sign-In on the client side.
        /// </summary>
        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin(
            [FromBody] GoogleLoginCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _googleLoginHandler.Handle(command, cancellationToken);

            if (!result.IsSuccess)
                return Unauthorized(new { error = result.Error });

            return Ok(result.Value);
        }
    }
}

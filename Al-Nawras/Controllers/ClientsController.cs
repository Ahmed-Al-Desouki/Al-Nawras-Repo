using Al_Nawras.Application.Clients.Commands.CreateClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Al_Nawras.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "SalesOrAdmin")]
    public class ClientsController : ControllerBase
    {
        private readonly CreateClientHandler _createHandler;

        public ClientsController(CreateClientHandler createHandler)
        {
            _createHandler = createHandler;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateClientCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _createHandler.Handle(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return CreatedAtAction(nameof(Create), new { id = result.Value }, new { id = result.Value });
        }
    }
}

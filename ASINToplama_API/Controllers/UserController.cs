using ASINToplama_BusinessLayer.Abstract;
using ASINToplama_EntityLayer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASINToplama_API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [AllowAnonymous] // şimdilik açık
    public class UserController : ControllerBase
    {
        private readonly IUserService _users;

        public UserController(IUserService users) => _users = users;

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken ct)
        {
            var dto = await _users.GetByIdAsync(id, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        [HttpGet("by-email")]
        public async Task<ActionResult<UserDto>> GetByEmail([FromQuery] string email, CancellationToken ct)
        {
            var dto = await _users.GetByEmailAsync(email, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<UserDto>>> Search(
            [FromQuery] string? keyword,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            var result = await _users.SearchAsync(keyword, pageNumber, pageSize, ct);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> Create([FromBody] UserCreateRequest req, CancellationToken ct)
        {
            var created = await _users.CreateAsync(req, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UserUpdateRequest req, CancellationToken ct)
        {
            var updated = await _users.UpdateAsync(id, req, ct);
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var ok = await _users.DeleteAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }
    }
}

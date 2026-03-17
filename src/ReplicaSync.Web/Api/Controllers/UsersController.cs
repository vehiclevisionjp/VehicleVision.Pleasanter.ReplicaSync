using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReplicaSync.Core.Interfaces;
using ReplicaSync.Core.Models;
using ReplicaSync.Web.Api.Models;
using ReplicaSync.Web.Security;

namespace ReplicaSync.Web.Api.Controllers;

/// <summary>
/// API controller for managing application users.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]
public sealed class UsersController : ControllerBase
{
    private readonly IAppUserRepository _userRepo;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/> class.
    /// </summary>
    public UsersController(IAppUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    /// <summary>Gets all users.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await _userRepo.GetAllAsync(cancellationToken);
        return Ok(users.Select(ToResponse));
    }

    /// <summary>Gets a user by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(user));
    }

    /// <summary>Creates a new user.</summary>
    [HttpPost]
    public async Task<ActionResult<UserResponse>> CreateAsync(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await _userRepo.GetByUsernameAsync(request.Username, cancellationToken);
        if (existing is not null)
        {
            return Conflict(new { message = "指定されたユーザー名は既に使用されています。" });
        }

        var user = new AppUser
        {
            Username = request.Username,
            PasswordHash = PasswordHasher.HashPassword(request.Password),
            Role = request.Role,
            MustChangePassword = request.MustChangePassword,
        };

        var created = await _userRepo.CreateAsync(user, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = created.Id }, ToResponse(created));
    }

    /// <summary>Updates an existing user.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserResponse>> UpdateAsync(
        int id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        // Check if the new username conflicts with another user
        var existing = await _userRepo.GetByUsernameAsync(request.Username, cancellationToken);
        if (existing is not null && existing.Id != id)
        {
            return Conflict(new { message = "指定されたユーザー名は既に使用されています。" });
        }

        user.Username = request.Username;
        user.Role = request.Role;
        user.MustChangePassword = request.MustChangePassword;

        if (!string.IsNullOrEmpty(request.Password))
        {
            user.PasswordHash = PasswordHasher.HashPassword(request.Password);
        }

        var updated = await _userRepo.UpdateAsync(user, cancellationToken);
        return Ok(ToResponse(updated));
    }

    /// <summary>Deletes a user by ID.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        await _userRepo.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private static UserResponse ToResponse(AppUser user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Role = user.Role,
        MustChangePassword = user.MustChangePassword,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt,
    };
}

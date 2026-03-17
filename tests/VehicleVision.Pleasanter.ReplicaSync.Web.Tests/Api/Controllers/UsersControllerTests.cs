using Microsoft.AspNetCore.Mvc;
using VehicleVision.Pleasanter.ReplicaSync.Core.Enums;
using VehicleVision.Pleasanter.ReplicaSync.Core.Interfaces;
using VehicleVision.Pleasanter.ReplicaSync.Core.Models;
using VehicleVision.Pleasanter.ReplicaSync.Web.Api.Controllers;
using VehicleVision.Pleasanter.ReplicaSync.Web.Api.Models;

namespace VehicleVision.Pleasanter.ReplicaSync.Web.Tests.Api.Controllers;

public class UsersControllerTests
{
    private readonly StubAppUserRepository _repo = new();

    private UsersController CreateController() => new(_repo);

    [Fact]
    public async Task GetAllAsyncShouldReturnAllUsers()
    {
        // Arrange
        _repo.Seed(new AppUser { Id = 1, Username = "admin", Role = AppRole.Administrator });
        _repo.Seed(new AppUser { Id = 2, Username = "user1", Role = AppRole.User });
        var controller = CreateController();

        // Act
        var result = await controller.GetAllAsync(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var users = Assert.IsAssignableFrom<IEnumerable<UserResponse>>(okResult.Value);
        Assert.Equal(2, users.Count());
    }

    [Fact]
    public async Task GetByIdAsyncShouldReturnUserWhenExists()
    {
        // Arrange
        _repo.Seed(new AppUser { Id = 1, Username = "admin", Role = AppRole.Administrator });
        var controller = CreateController();

        // Act
        var result = await controller.GetByIdAsync(1, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var user = Assert.IsType<UserResponse>(okResult.Value);
        Assert.Equal("admin", user.Username);
    }

    [Fact]
    public async Task GetByIdAsyncShouldReturnNotFoundWhenMissing()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.GetByIdAsync(999, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateAsyncShouldReturnCreatedUser()
    {
        // Arrange
        var controller = CreateController();
        var request = new CreateUserRequest
        {
            Username = "newuser",
            Password = "password123",
            Role = AppRole.User,
            MustChangePassword = true,
        };

        // Act
        var result = await controller.CreateAsync(request, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var user = Assert.IsType<UserResponse>(createdResult.Value);
        Assert.Equal("newuser", user.Username);
        Assert.Equal(AppRole.User, user.Role);
    }

    [Fact]
    public async Task CreateAsyncShouldReturnConflictWhenUsernameExists()
    {
        // Arrange
        _repo.Seed(new AppUser { Id = 1, Username = "existing" });
        var controller = CreateController();
        var request = new CreateUserRequest
        {
            Username = "existing",
            Password = "password123",
        };

        // Act
        var result = await controller.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateAsyncShouldReturnUpdatedUser()
    {
        // Arrange
        _repo.Seed(new AppUser { Id = 1, Username = "admin", Role = AppRole.Administrator });
        var controller = CreateController();
        var request = new UpdateUserRequest
        {
            Username = "admin-updated",
            Role = AppRole.Administrator,
            MustChangePassword = false,
        };

        // Act
        var result = await controller.UpdateAsync(1, request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var user = Assert.IsType<UserResponse>(okResult.Value);
        Assert.Equal("admin-updated", user.Username);
    }

    [Fact]
    public async Task UpdateAsyncShouldReturnNotFoundWhenMissing()
    {
        // Arrange
        var controller = CreateController();
        var request = new UpdateUserRequest { Username = "test" };

        // Act
        var result = await controller.UpdateAsync(999, request, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task UpdateAsyncShouldReturnConflictWhenUsernameConflicts()
    {
        // Arrange
        _repo.Seed(new AppUser { Id = 1, Username = "user1" });
        _repo.Seed(new AppUser { Id = 2, Username = "user2" });
        var controller = CreateController();
        var request = new UpdateUserRequest { Username = "user2" };

        // Act
        var result = await controller.UpdateAsync(1, request, CancellationToken.None);

        // Assert
        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task DeleteAsyncShouldReturnNoContentWhenExists()
    {
        // Arrange
        _repo.Seed(new AppUser { Id = 1, Username = "admin" });
        var controller = CreateController();

        // Act
        var result = await controller.DeleteAsync(1, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteAsyncShouldReturnNotFoundWhenMissing()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.DeleteAsync(999, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    /// <summary>In-memory stub for IAppUserRepository.</summary>
    private sealed class StubAppUserRepository : IAppUserRepository
    {
        private readonly List<AppUser> _users = [];
        private int _nextId = 1;

        public void Seed(AppUser user)
        {
            if (user.Id == 0) user.Id = _nextId++;
            else if (user.Id >= _nextId) _nextId = user.Id + 1;
            _users.Add(user);
        }

        public Task<IReadOnlyList<AppUser>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AppUser>>(_users.ToList());

        public Task<AppUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.Find(u => u.Id == id));

        public Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.Find(u =>
                string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)));

        public Task<AppUser> CreateAsync(AppUser user, CancellationToken cancellationToken = default)
        {
            user.Id = _nextId++;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            _users.Add(user);
            return Task.FromResult(user);
        }

        public Task<AppUser> UpdateAsync(AppUser user, CancellationToken cancellationToken = default)
        {
            user.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult(user);
        }

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            _users.RemoveAll(u => u.Id == id);
            return Task.CompletedTask;
        }

        public Task<bool> AnyUsersExistAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_users.Count > 0);
    }
}

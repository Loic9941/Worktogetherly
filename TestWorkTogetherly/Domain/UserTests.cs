using FluentAssertions;
using WorkTogetherly.Domain.Entities;

namespace TestWorkTogetherly.Domain;

public class UserTests
{
    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WhenValid_SetsFields()
    {
        var user = User.Create("Alice", "Dupont", "alice@test.com");

        user.FirstName.Should().Be("Alice");
        user.LastName.Should().Be("Dupont");
        user.Email.Should().Be("alice@test.com");
        user.UserName.Should().Be("alice@test.com");
        user.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_WhenFirstNameNull_Throws()
    {
        var act = () => User.Create(null!, "Dupont", "alice@test.com");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WhenLastNameNull_Throws()
    {
        var act = () => User.Create("Alice", null!, "alice@test.com");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WhenEmailNull_Throws()
    {
        var act = () => User.Create("Alice", "Dupont", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── UpdateProfile ─────────────────────────────────────────────────────────

    [Fact]
    public void UpdateProfile_WhenValid_MutatesFields()
    {
        var user = User.Create("Alice", "Dupont", "alice@test.com");

        user.UpdateProfile("Bob", "Martin");

        user.FirstName.Should().Be("Bob");
        user.LastName.Should().Be("Martin");
    }

    // ── ReplacePhoto ──────────────────────────────────────────────────────────

    [Fact]
    public void ReplacePhoto_WhenNoExistingPhoto_SetsPathAndReturnsNull()
    {
        var user = User.Create("Alice", "Dupont", "alice@test.com");

        var previous = user.ReplacePhoto("uploads/users/photo.jpg");

        previous.Should().BeNull();
        user.PhotoPath.Should().Be("uploads/users/photo.jpg");
    }

    [Fact]
    public void ReplacePhoto_WhenExistingPhoto_SetsNewPathAndReturnsPrevious()
    {
        var user = User.Create("Alice", "Dupont", "alice@test.com");
        user.ReplacePhoto("old/path.jpg");

        var previous = user.ReplacePhoto("new/path.jpg");

        previous.Should().Be("old/path.jpg");
        user.PhotoPath.Should().Be("new/path.jpg");
    }

    // ── RemovePhoto ───────────────────────────────────────────────────────────

    [Fact]
    public void RemovePhoto_WhenPhotoExists_ClearsPathAndReturnsPrevious()
    {
        var user = User.Create("Alice", "Dupont", "alice@test.com");
        user.ReplacePhoto("uploads/users/photo.jpg");

        var previous = user.RemovePhoto();

        previous.Should().Be("uploads/users/photo.jpg");
        user.PhotoPath.Should().BeNull();
    }

    [Fact]
    public void RemovePhoto_WhenNoPhoto_ReturnsNull()
    {
        var user = User.Create("Alice", "Dupont", "alice@test.com");

        var previous = user.RemovePhoto();

        previous.Should().BeNull();
        user.PhotoPath.Should().BeNull();
    }
}

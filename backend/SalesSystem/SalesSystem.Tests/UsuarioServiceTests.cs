using SalesSystem.BLL.Services;
using SalesSystem.DAL.Repositories;
using SalesSystem.DTO;
using SalesSystem.Entity;
using SalesSystem.Utility;

namespace SalesSystem.Tests;

public class UsuarioServiceTests : IDisposable
{
    private readonly SqliteDatabase _db = new();

    public UsuarioServiceTests()
    {
        _db.Seed(ctx =>
        {
            ctx.Rols.Add(new Rol { IdRol = 1, Name = "Admin", DateRegistry = DateTime.Now });
            ctx.Usuario.AddRange(
                new Usuario { IdUser = 1, FullName = "Legacy", Email = "legacy@x.com", IdRol = 1, Password = "plain-pass", Status = true, DateRegistry = DateTime.Now },
                new Usuario { IdUser = 2, FullName = "Hashed", Email = "hashed@x.com", IdRol = 1, Password = PasswordHasher.Hash("secret-123"), Status = true, DateRegistry = DateTime.Now },
                new Usuario { IdUser = 3, FullName = "Off", Email = "off@x.com", IdRol = 1, Password = PasswordHasher.Hash("secret-123"), Status = false, DateRegistry = DateTime.Now });
        });
    }

    public void Dispose() => _db.Dispose();

    private UsuarioService NewService(out SalesSystem.DAL.DbSalesContext.DBSalesContext ctx)
    {
        ctx = _db.NewContext();
        return new UsuarioService(new GenericRepository<Usuario>(ctx), SqliteDatabase.CreateMapper());
    }

    private string? StoredPassword(int idUser)
    {
        using var ctx = _db.NewContext();
        return ctx.Usuario.Single(u => u.IdUser == idUser).Password;
    }

    [Fact]
    public async Task ValidateCredentials_LegacyPlainTextPassword_LogsInAndIsRehashed()
    {
        var service = NewService(out var ctx);
        using (ctx)
        {
            var session = await service.ValidateCredentials("legacy@x.com", "plain-pass");
            Assert.NotNull(session);
            Assert.Equal("Admin", session!.RolDescription);
        }

        string? stored = StoredPassword(1);
        Assert.True(PasswordHasher.IsHashed(stored));
        Assert.True(PasswordHasher.Verify("plain-pass", stored, out _));
    }

    [Theory]
    [InlineData("hashed@x.com", "wrong-password")]
    [InlineData("nobody@x.com", "secret-123")]
    [InlineData("hashed@x.com", "")]
    public async Task ValidateCredentials_InvalidCredentials_ReturnsNull(string email, string password)
    {
        var service = NewService(out var ctx);
        using (ctx)
        {
            Assert.Null(await service.ValidateCredentials(email, password));
        }
    }

    [Fact]
    public async Task ValidateCredentials_InactiveUser_IsRejected()
    {
        var service = NewService(out var ctx);
        using (ctx)
        {
            var ex = await Assert.ThrowsAsync<BusinessException>(() => service.ValidateCredentials("off@x.com", "secret-123"));
            Assert.Equal(403, ex.StatusCode);
        }
    }

    [Fact]
    public async Task List_NeverReturnsPasswords()
    {
        var service = NewService(out var ctx);
        using (ctx)
        {
            var users = await service.List();
            Assert.All(users, u => Assert.Null(u.Password));
        }
    }

    [Fact]
    public async Task Create_HashesPassword_AndRejectsShortOnes()
    {
        var service = NewService(out var ctx);
        using (ctx)
        {
            await Assert.ThrowsAsync<BusinessException>(() => service.Create(new UsuarioDTO { FullName = "New", Email = "new@x.com", IdRol = 1, Password = "short", Status = 1 }));

            var created = await service.Create(new UsuarioDTO { FullName = "New", Email = "new@x.com", IdRol = 1, Password = "long-enough", Status = 1 });
            Assert.Null(created.Password);
            Assert.True(PasswordHasher.Verify("long-enough", StoredPassword(created.IdUser), out _));
        }
    }

    [Fact]
    public async Task Update_WithEmptyPassword_KeepsCurrentPassword()
    {
        string? before = StoredPassword(2);
        var service = NewService(out var ctx);
        using (ctx)
        {
            await service.Update(new UsuarioDTO { IdUser = 2, FullName = "Renamed", Email = "hashed@x.com", IdRol = 1, Password = "", Status = 1 });
        }

        Assert.Equal(before, StoredPassword(2));
    }
}

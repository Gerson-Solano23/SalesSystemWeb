using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SalesSystem.DAL.DbSalesContext;
using SalesSystem.Entity;
using SalesSystem.Utility;

namespace SalesSystem.Tests;

/// <summary>
/// In-memory SQLite database built from the real EF model, so repository and service
/// logic runs real SQL (transactions, ExecuteUpdate) without needing SQL Server.
/// </summary>
public sealed class SqliteDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        using var context = NewContext();
        context.Database.EnsureCreated();
    }

    public DBSalesContext NewContext()
    {
        var options = new DbContextOptionsBuilder<DBSalesContext>().UseSqlite(_connection).Options;
        return new SqliteSalesContext(options);
    }

    /// <summary>
    /// SQLite cannot SUM decimal columns (SQL Server can), so decimals are stored as REAL in tests only.
    /// </summary>
    private sealed class SqliteSalesContext : DBSalesContext
    {
        public SqliteSalesContext(DbContextOptions<DBSalesContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetProperties())
                         .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetValueConverter(property.ClrType == typeof(decimal)
                    ? new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<decimal, double>(v => (double)v, v => (decimal)v)
                    : new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<decimal?, double?>(v => (double?)v, v => (decimal?)v));
            }
        }
    }

    public static IMapper CreateMapper() =>
        new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>()).CreateMapper();

    public void Seed(Action<DBSalesContext> seed)
    {
        using var context = NewContext();
        seed(context);
        context.SaveChanges();
    }

    public void SeedCatalog()
    {
        Seed(ctx =>
        {
            ctx.Categories.Add(new Category { IdCategory = 1, Name = "General", Status = true, DateRegistry = DateTime.Now });
            ctx.Products.AddRange(
                new Product { IdProduct = 1, Name = "Coffee", IdCategory = 1, Stock = 5, Price = 10m, Status = true, DateRegistry = DateTime.Now },
                new Product { IdProduct = 2, Name = "Tea", IdCategory = 1, Stock = 3, Price = 2.5m, Status = true, DateRegistry = DateTime.Now },
                new Product { IdProduct = 3, Name = "Old item", IdCategory = 1, Stock = 10, Price = 1m, Status = false, DateRegistry = DateTime.Now });
            ctx.DocumentNumbers.Add(new DocumentNumber { IdDocumentNumber = 1, LastNumber = 0, DateRegistry = DateTime.Now });
        });
    }

    public void Dispose() => _connection.Dispose();
}

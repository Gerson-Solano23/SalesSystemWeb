using Microsoft.EntityFrameworkCore;
using SalesSystem.DAL.Repositories;
using SalesSystem.Entity;
using SalesSystem.Utility;

namespace SalesSystem.Tests;

public class SalesRepositoryTests : IDisposable
{
    private readonly SqliteDatabase _db = new();

    public SalesRepositoryTests() => _db.SeedCatalog();

    public void Dispose() => _db.Dispose();

    private static Sale NewSale(params (int IdProduct, int Quantity)[] lines)
    {
        var sale = new Sale { PaymentType = "Cash" };
        foreach (var (idProduct, quantity) in lines)
        {
            // A client-supplied price must be ignored.
            sale.SaleDetails.Add(new SaleDetail { IdProduct = idProduct, Quantity = quantity, Price = 0.01m, Total = 0.01m });
        }
        return sale;
    }

    [Fact]
    public async Task AddRegistry_DiscountsStock_UsesCatalogPrices_AndNumbersDocument()
    {
        using (var ctx = _db.NewContext())
        {
            var sale = await new SalesRepository(ctx).addRegistry(NewSale((1, 2), (2, 2)));

            Assert.Equal("0001", sale.numberDocument);
            Assert.Equal(25m, sale.Total);
            Assert.Equal(new[] { 10m, 2.5m }, sale.SaleDetails.Select(d => d.Price!.Value));
            Assert.Equal(new[] { 20m, 5m }, sale.SaleDetails.Select(d => d.Total!.Value));
        }

        using var check = _db.NewContext();
        Assert.Equal(3, (await check.Products.FindAsync(1))!.Stock);
        Assert.Equal(1, (await check.Products.FindAsync(2))!.Stock);
        Assert.Equal(1, (await check.DocumentNumbers.SingleAsync()).LastNumber);
    }

    [Fact]
    public async Task AddRegistry_GivesConsecutiveDocumentNumbers()
    {
        var numbers = new List<string?>();
        for (int i = 0; i < 3; i++)
        {
            using var ctx = _db.NewContext();
            numbers.Add((await new SalesRepository(ctx).addRegistry(NewSale((1, 1)))).numberDocument);
        }

        Assert.Equal(new[] { "0001", "0002", "0003" }, numbers);
    }

    [Fact]
    public async Task AddRegistry_WithoutEnoughStock_FailsAndRollsBackEverything()
    {
        using (var ctx = _db.NewContext())
        {
            // Coffee has stock, Tea (stock 3) does not: the coffee discount must be rolled back too.
            var ex = await Assert.ThrowsAsync<BusinessException>(() => new SalesRepository(ctx).addRegistry(NewSale((1, 2), (2, 4))));
            Assert.Contains("Not enough stock", ex.Message);
        }

        using var check = _db.NewContext();
        Assert.Equal(5, (await check.Products.FindAsync(1))!.Stock);
        Assert.Equal(3, (await check.Products.FindAsync(2))!.Stock);
        Assert.Equal(0, (await check.DocumentNumbers.SingleAsync()).LastNumber);
        Assert.Empty(check.Sales);
    }

    [Fact]
    public async Task AddRegistry_RejectsInactiveAndUnknownProducts()
    {
        using (var ctx = _db.NewContext())
        {
            await Assert.ThrowsAsync<BusinessException>(() => new SalesRepository(ctx).addRegistry(NewSale((3, 1))));
        }
        using (var ctx = _db.NewContext())
        {
            var ex = await Assert.ThrowsAsync<BusinessException>(() => new SalesRepository(ctx).addRegistry(NewSale((99, 1))));
            Assert.Equal(404, ex.StatusCode);
        }
    }
}

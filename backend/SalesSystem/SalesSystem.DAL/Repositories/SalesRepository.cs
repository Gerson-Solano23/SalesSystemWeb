using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SalesSystem.DAL.Repositories.Contract;
using SalesSystem.DAL.DbSalesContext;
using SalesSystem.Entity;
using SalesSystem.Utility;

namespace SalesSystem.DAL.Repositories
{
    public class SalesRepository : GenericRepository<Sale>, ISalesRepository
    {
        private const int DocumentNumberLength = 4;

        private readonly DBSalesContext _context;

        public SalesRepository(DBSalesContext dBSalesContext) : base(dBSalesContext)
        {
            _context = dBSalesContext;
        }

        /// <summary>
        /// Registers a sale in one transaction: discounts stock, takes the next document number and saves the sale.
        /// Stock and the document counter are updated with single atomic UPDATE statements, so concurrent
        /// sales can neither oversell a product nor receive the same document number.
        /// Prices come from the product catalog, never from the client.
        /// </summary>
        public async Task<Sale> addRegistry(Sale sale)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            decimal saleTotal = 0;
            foreach (SaleDetail sd in sale.SaleDetails)
            {
                int quantity = sd.Quantity ?? 0;
                Product? product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.IdProduct == sd.IdProduct);

                if (product == null)
                {
                    throw BusinessException.NotFound($"Product {sd.IdProduct} does not exist");
                }
                if (product.Status != true)
                {
                    throw new BusinessException($"Product '{product.Name}' is inactive");
                }

                int updated = await _context.Products
                    .Where(p => p.IdProduct == sd.IdProduct && p.Stock >= quantity)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock - quantity));

                if (updated == 0)
                {
                    throw new BusinessException($"Not enough stock for '{product.Name}' (available: {product.Stock ?? 0})");
                }

                sd.Price = product.Price ?? 0;
                sd.Total = sd.Price * quantity;
                saleTotal += sd.Total.Value;
            }

            int counterRows = await _context.DocumentNumbers.ExecuteUpdateAsync(s => s
                .SetProperty(d => d.LastNumber, d => d.LastNumber + 1)
                .SetProperty(d => d.DateRegistry, DateTime.Now));
            if (counterRows == 0)
            {
                throw new InvalidOperationException("DocumentNumber table has no rows");
            }

            // The row is locked by the UPDATE above until commit, so this read sees our own number.
            int lastNumber = await _context.DocumentNumbers.AsNoTracking().Select(d => d.LastNumber).FirstAsync();

            sale.numberDocument = lastNumber.ToString().PadLeft(DocumentNumberLength, '0');
            sale.Total = saleTotal;
            sale.DateRegistry = DateTime.Now;

            await _context.Sales.AddAsync(sale);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return sale;
        }
    }
}

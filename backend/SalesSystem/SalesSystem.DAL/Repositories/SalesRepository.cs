using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesSystem.DAL.Repositories.Contract;
using SalesSystem.DAL.DbSalesContext;
using SalesSystem.Entity;
namespace SalesSystem.DAL.Repositories
{
    public class SalesRepository : GenericRepository<Sale>, ISalesRepository
    {
        private readonly DBSalesContext _context;

        public SalesRepository(DBSalesContext dBSalesContext) : base(dBSalesContext)
        {
            _context = dBSalesContext;
        }

        public async Task<IEnumerable<Sale>> addRegistry(Sale sale)
        {
            Sale saleAdd = new Sale();

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    foreach (SaleDetail sd in sale.SaleDetails)
                    {
                        Product productFound = _context.Products.Where(p=>p.IdProduct == sd.IdProduct).First();

                        productFound.Stock -= sd.Quantity;

                        _context.Products.Update(productFound);

                    }
                    await _context.SaveChangesAsync();


                    DocumentNumber documentNumber = _context.DocumentNumbers.First();

                    documentNumber.LastNumber += 1;
                    documentNumber.DateRegistry = DateTime.Now;
                    _context.DocumentNumbers.Update(documentNumber);

                    await _context.SaveChangesAsync();


                    int numbersQuantity = 4;
                    string ceros = string.Concat(Enumerable.Repeat("0", numbersQuantity));
                    string numeroVenta = ceros + documentNumber.LastNumber.ToString();

                    numeroVenta = numeroVenta.Substring(numeroVenta.Length - numbersQuantity, numbersQuantity);

                    sale.NumeroDocumento = numeroVenta;

                    await _context.AddAsync(sale);
                    await _context.SaveChangesAsync();


                    saleAdd = sale;

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }

        }
    }
}

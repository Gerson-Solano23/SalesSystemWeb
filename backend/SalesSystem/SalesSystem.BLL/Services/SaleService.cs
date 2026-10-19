using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DAL.Repositories.Contract;
using SalesSystem.DTO;
using SalesSystem.Entity;
using SalesSystem.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalesSystem.BLL.Services
{
    public class SaleService : ISale
    {
        private readonly ISalesRepository _saleRepository;
        private readonly IGenericRepository<SaleDetail> _saleDetailsRespository;
        private readonly IMapper _mapper;

        public SaleService(ISalesRepository saleRepository, IGenericRepository<SaleDetail> saleDetailsRespository, IMapper mapper)
        {
            _saleRepository = saleRepository;
            _saleDetailsRespository = saleDetailsRespository;
            _mapper = mapper;
        }

        public async Task<SaleDTO> Create(SaleDTO entity)
        {
            if (entity.SaleDetails == null || entity.SaleDetails.Count == 0)
            {
                throw new BusinessException("The sale must contain at least one product");
            }

            if (entity.SaleDetails.Any(d => d.IdProduct == null || d.Quantity == null || d.Quantity <= 0))
            {
                throw new BusinessException("Every product in the sale needs a valid quantity");
            }

            // Prices and totals sent by the client are ignored; the repository computes them from the catalog.
            var sale = new Sale
            {
                PaymentType = entity.PaymentType,
            };
            foreach (var detail in entity.SaleDetails)
            {
                sale.SaleDetails.Add(new SaleDetail
                {
                    IdProduct = detail.IdProduct,
                    Quantity = detail.Quantity
                });
            }

            var created = await _saleRepository.addRegistry(sale);
            return _mapper.Map<SaleDTO>(created);
        }

        public async Task<List<SaleDTO>> History(string searchBy, string saleNumber, string startdate, string endDate)
        {
            IQueryable<Sale> query = await _saleRepository.Consult();

            if (searchBy == "date")
            {
                var (start, end) = DateParser.ParseRange(startdate, endDate);
                var endExclusive = end.AddDays(1);
                query = query.Where(x => x.DateRegistry >= start && x.DateRegistry < endExclusive);
            }
            else
            {
                query = query.Where(x => x.numberDocument == saleNumber);
            }

            var resultList = await query.Include(ds => ds.SaleDetails)
                                        .ThenInclude(p => p.IdProductNavigation)
                                        .ToListAsync();

            return _mapper.Map<List<SaleDTO>>(resultList);
        }

        public async Task<List<ReportDTO>> Report(string startDate, string endDate)
        {
            var (start, end) = DateParser.ParseRange(startDate, endDate);
            var endExclusive = end.AddDays(1);

            IQueryable<SaleDetail> query = await _saleDetailsRespository.Consult();
            var resultList = await query.Include(p => p.IdProductNavigation)
                .Include(s => s.IdSaleNavigation)
                .Where(x => x.IdSaleNavigation!.DateRegistry >= start && x.IdSaleNavigation.DateRegistry < endExclusive)
                .ToListAsync();

            return _mapper.Map<List<ReportDTO>>(resultList);
        }
    }
}

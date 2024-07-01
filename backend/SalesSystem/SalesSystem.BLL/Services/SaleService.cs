using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DAL.Repositories.Contract;
using SalesSystem.DTO;
using SalesSystem.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
            try
            {
                var genericSale = await _saleRepository.addRegistry(_mapper.Map<Sale>(entity));

                if (genericSale.IdSale == 0)
                {
                    throw new TaskCanceledException("Unable to create");
                }

                return _mapper.Map<SaleDTO>(genericSale);
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<List<SaleDTO>> History(string searchBy, string saleNumber, string startdate, string endDate)
        {
            IQueryable<Sale> query = await _saleRepository.Consult();
            var ResultList = new List<Sale>();
            try
            {
                if (searchBy == "date")
                {
                    DateTime StartDate = DateTime.ParseExact(startdate, "dd/MM/yyyy", new CultureInfo("es-CR"));
                    DateTime EndDate = DateTime.ParseExact(endDate, "dd/MM/yyyy", new CultureInfo("es-CR"));

                    ResultList = await query.Where(x => x.DateRegistry.Value.Date >= StartDate.Date &&
                                                   x.DateRegistry.Value.Date <= EndDate.Date
                                                   ).Include(ds => ds.SaleDetails)
                                                   .ThenInclude(p => p.IdProductNavigation)
                                                   .ToListAsync();

                }
                else
                {
                    ResultList = await query.Where(x => x.NumeroDocumento == saleNumber
                                                 ).Include(ds => ds.SaleDetails)
                                                 .ThenInclude(p => p.IdProductNavigation)
                                                 .ToListAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
            return _mapper.Map<List<SaleDTO>>(ResultList);
        }

        public async Task<List<ReportDTO>> Report(string startDate, string endDate)
        {
            IQueryable<SaleDetail> query = await _saleDetailsRespository.Consult();
            var ResultList = new List<SaleDetail>();
            try
            {
                DateTime StartDate = DateTime.ParseExact(startDate, "dd/MM/yyyy", new CultureInfo("es-CR"));
                DateTime EndDate = DateTime.ParseExact(endDate, "dd/MM/yyyy", new CultureInfo("es-CR"));

                ResultList = await query.Include(p => p.IdProductNavigation)
                    .Include(s => s.IdSaleNavigation)
                    .Where(x=>x.IdSaleNavigation.DateRegistry.Value.Date >= StartDate.Date && 
                     x.IdSaleNavigation.DateRegistry.Value.Date <= EndDate.Date)
                    .ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }

            return _mapper.Map<List<ReportDTO>>(ResultList);
        }

    }
}

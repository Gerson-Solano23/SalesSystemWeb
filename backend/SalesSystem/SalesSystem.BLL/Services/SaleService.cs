using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DAL.Repositories.Contract;
using SalesSystem.DTO;
using SalesSystem.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.BLL.Services
{
    public class SaleService : ISale
    {
        private readonly IGenericRepository<Sale> _saleRepository;
        private readonly IMapper _mapper;

        public SaleService(IGenericRepository<Sale> repositorySale, IMapper mapper)
        {
            _mapper = mapper;
            _saleRepository = repositorySale;
        }

        public async Task<SaleDTO> Create(SaleDTO entity)
        { 
            try
            {
                var newSale = await _saleRepository.AddAsync(_mapper.Map<Sale>(entity));

                if (newSale.IdSale == 0)
                {
                    throw new TaskCanceledException("Unable to create the sale");
                }

                return _mapper.Map<SaleDTO>(newSale);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<SaleDTO>> List()
        {
            try
            {
                var query = await _saleRepository.Consult();

                var list = query.Include(x => x.IdSale).ToList();

                return _mapper.Map<List<SaleDTO>>(list);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}

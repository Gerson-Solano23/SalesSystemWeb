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
    public class ProductService : IProduct
    {
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IMapper _mapper;

        public ProductService(IGenericRepository<Product> repositoryProduct, IMapper mapper)
        {
            _mapper = mapper;
            _productRepository = repositoryProduct;
        }

        public async Task<ProductDTO> Create(ProductDTO entity)
        {
            try
            {
                var product = await _productRepository.AddAsync(_mapper.Map<Product>(entity));

                if (product.IdProduct == 0)
                {
                    throw new TaskCanceledException("Unable to create");
                }

                var query = await _productRepository.Consult(x => x.Name == entity.Name);

                product = query.Include(rol => rol.IdCategoryNavigation).First();

                return _mapper.Map<ProductDTO>(product);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<bool> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ProductDTO> Get(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ProductDTO>> List()
        {
            try
            {
                var query = await _productRepository.Consult();

                var productList = query.Include(x => x.IdCategoryNavigation).ToList();

                return _mapper.Map<List<ProductDTO>>(productList.ToList());
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<bool> Update(ProductDTO entity)
        {
            throw new NotImplementedException();
        }
    }
}

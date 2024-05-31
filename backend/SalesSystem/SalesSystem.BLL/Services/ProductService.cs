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

        public async Task<bool> Delete(int id)
        {
            try
            {
                var productFound = await _productRepository.GetByIdAsync(id);

                if (productFound == null)
                {
                    throw new TaskCanceledException("Product not exist");
                }

                bool result = await _productRepository.DeleteAsync(productFound);

                if (!result)
                {
                    throw new TaskCanceledException("Unable to delete product");
                }
                return result;  
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<ProductDTO> Get(int id)
        {
            try
            {
                var productFound = await _productRepository.GetByIdAsync(id);

                if (productFound == null)
                {
                    throw new TaskCanceledException("Product not exist");
                }

                return _mapper.Map<ProductDTO>(productFound);
            }
            catch (Exception)
            {

                throw;
            }
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

        public async Task<bool> Update(ProductDTO entity)
        {
            try
            {
                var productFound = await _productRepository.GetByIdAsync(entity.IdProduct);

                if (productFound == null)
                {
                    throw new TaskCanceledException("Product not exist");
                }

                var product = _mapper.Map<Product>(entity);

                bool result = await _productRepository.UpdateAsync(product);
                if (!result)
                {
                    throw new TaskCanceledException("Unable to update product");
                }
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}

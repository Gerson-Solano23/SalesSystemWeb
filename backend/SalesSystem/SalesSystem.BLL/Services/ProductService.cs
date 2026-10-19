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
    public class ProductService : IProduct
    {
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<SaleDetail> _saleDetailRepository;
        private readonly IMapper _mapper;

        public ProductService(IGenericRepository<Product> repositoryProduct, IGenericRepository<SaleDetail> saleDetailRepository, IMapper mapper)
        {
            _mapper = mapper;
            _productRepository = repositoryProduct;
            _saleDetailRepository = saleDetailRepository;
        }

        public async Task<ProductDTO> Create(ProductDTO entity)
        {
            var newProduct = _mapper.Map<Product>(entity);
            Validate(newProduct);

            var product = await _productRepository.CreateAsync(newProduct);
            if (product.IdProduct == 0)
            {
                throw new BusinessException("Unable to create");
            }

            var query = await _productRepository.Consult(x => x.IdProduct == product.IdProduct);
            product = await query.Include(c => c.IdCategoryNavigation).FirstAsync();

            return _mapper.Map<ProductDTO>(product);
        }

        public async Task<bool> Delete(int id)
        {
            var productFound = await _productRepository.GetByIdAsync(id);
            if (productFound == null)
            {
                throw BusinessException.NotFound("Product not exist");
            }

            var sales = await _saleDetailRepository.Consult(x => x.IdProduct == id);
            if (await sales.AnyAsync())
            {
                throw new BusinessException("This product already has sales and cannot be deleted. Set it as inactive instead.");
            }

            return await _productRepository.DeleteAsync(productFound);
        }

        public async Task<ProductDTO> Get(int id)
        {
            var query = await _productRepository.Consult(x => x.IdProduct == id);
            var productFound = await query.Include(c => c.IdCategoryNavigation).FirstOrDefaultAsync();

            if (productFound == null)
            {
                throw BusinessException.NotFound("Product not exist");
            }

            return _mapper.Map<ProductDTO>(productFound);
        }

        public async Task<List<ProductDTO>> List()
        {
            var query = await _productRepository.Consult();
            var productList = await query.Include(x => x.IdCategoryNavigation).ToListAsync();

            return _mapper.Map<List<ProductDTO>>(productList);
        }

        public async Task<bool> Update(ProductDTO entity)
        {
            var productEntity = _mapper.Map<Product>(entity);
            Validate(productEntity);

            var productFound = await _productRepository.GetByIdAsync(entity.IdProduct);
            if (productFound == null)
            {
                throw BusinessException.NotFound("Product not exist");
            }

            productFound.IdCategory = productEntity.IdCategory;
            productFound.Name = productEntity.Name;
            productFound.Price = productEntity.Price;
            productFound.Stock = productEntity.Stock;
            productFound.Status = productEntity.Status;

            return await _productRepository.UpdateAsync(productFound);
        }

        private static void Validate(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                throw new BusinessException("Product name is required");
            }
            if (product.Price == null || product.Price < 0)
            {
                throw new BusinessException("Price must be zero or greater");
            }
            if (product.Stock == null || product.Stock < 0)
            {
                throw new BusinessException("Stock must be zero or greater");
            }
        }
    }
}

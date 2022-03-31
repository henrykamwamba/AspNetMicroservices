using Catalog.Api.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Catalog.Api.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductModel>> GetProducts();
        Task<ProductModel> GetProduct(string id);
        Task<IEnumerable<ProductModel>> GetProductByName(string name);
        Task<IEnumerable<ProductModel>> GetProductByCategory(string categoryName);

        Task CreateProduct(ProductModel product);
        Task<bool> UpdateProduct(ProductModel product);
        Task<bool> DeleteProduct(string id);
    }
}

using Catalog.Api.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Api.Data
{
    public class CatalogContextSeed
    {
        public static void SeedData(IMongoCollection<ProductModel> productCollection) 
        {
            bool productExists = productCollection.Find(p => true).Any();
            if (!productExists)
            {

            }
        }
    }
}

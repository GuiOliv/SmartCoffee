using SmartCoffee.Data;
using SmartCoffee.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCoffee.Business
{
    public class AddProduct
    {
        public AddProduct()
        {
        }

        public Product Product { get; set; } = new Product();

        public async Task OnAddProduct()
        {
            var MLModel = new MLModel();
            using (var context= new Context())
            {
                await MLModel.IndexFiles(Product);
                await context.Products.AddAsync(Product);
                await context.SaveChangesAsync();
            }
        }

    }
}

using SmartCoffee.Data;
using SmartCoffee.Data.Entities;

namespace SmartCoffee.Business
{
    public class HomeModel
    {
        public HomeModel()
        {
            using (var context = new Context())
            {
                Products = context.Products.ToList();
            }
        }

        public List<Product> Products { get; set; }

        public async Task DeleteProduct(int id)
        {
            using (var context = new Context())
            {
                context.Products.Remove(context.Products.First(w => w.Id == id));
                await context.SaveChangesAsync();
            }
        }
    }
}

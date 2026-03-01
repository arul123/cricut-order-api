namespace Cricut.Orders.Domain.Models
{
    public class OrderItem
    {
        public Product Product { get; set; } = new Product();

        public int Quantity { get; set; }

        public double Total => Product.Price * Quantity;
    }
}
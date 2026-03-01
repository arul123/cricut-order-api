using Cricut.Orders.Domain.Models;

namespace Cricut.Orders.Domain
{
    public interface IOrderStore
    {
        Task<Order> SaveOrderAsync(Order order);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<Order[]> GetAllOrdersAsync();
        Task<Order[]> GetAllOrdersForCustomerAsync(int customerId);
        Task DeleteOrderByIdAsync(int orderId);
    }
}

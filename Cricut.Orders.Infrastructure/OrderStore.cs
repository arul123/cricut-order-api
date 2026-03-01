using Cricut.Orders.Domain;
using Cricut.Orders.Domain.Models;

namespace Cricut.Orders.Infrastructure
{
    public class OrderStore : IOrderStore
    {
        public OrderStore()
        {
            GenerateStaticOrders();
        }

        private Dictionary<int, Order> _orders = new Dictionary<int, Order>();
        private Random _random = new Random();

        public Task DeleteOrderByIdAsync(int orderId)
        {
            _orders.Remove(orderId);
            return Task.CompletedTask;
        }

        public Task<Order[]> GetAllOrdersAsync()
        {
            return Task.FromResult(_orders.Values.ToArray());
        }

        public Task<Order[]> GetAllOrdersForCustomerAsync(int customerId)
        {
            var customersOrders = _orders.Values.Where(o => o.Customer.Id == customerId);
            return Task.FromResult(customersOrders.ToArray());
        }

        public Task<Order?> GetOrderByIdAsync(int orderId)
        {
            var order = _orders.ContainsKey(orderId) ? _orders[orderId] : null;
            return Task.FromResult(order);
        }

        public Task<Order> SaveOrderAsync(Order order)
        {
            order.Id = order.Id ?? _random.Next(100, 1000000);

            _orders[order.Id.Value] = order;

            return Task.FromResult(order);
        }

        private void GenerateStaticOrders()
        {
            var customer1 = new Customer
            {
                Id = 12345,
                Name = "John Jacobs",
                Email = "jj@gmail.com",
                Address = "321 Gordon Ave"
            };

            var customer2 = new Customer
            {
                Id = 54321,
                Name = "John Jacobs",
                Email = "jj@gmail.com",
                Address = "321 Gordon Ave"
            };

            for (int i = 1; i <= 10; i++)
            {
                var orderItems = Enumerable.Range(1, i).Select(x => new OrderItem
                {
                    Quantity = i,
                    Product = new Product
                    {
                        Id = x,
                        Name = $"Product {x}",
                        Price = 1.5 * x
                    }
                });

                var order = new Order
                {
                    Id = i,
                    Customer = i % 2 == 0 ? customer1 : customer2,
                    OrderItems = orderItems.ToArray()
                };

                _orders.Add(i, order);
            }
        }
    }
}

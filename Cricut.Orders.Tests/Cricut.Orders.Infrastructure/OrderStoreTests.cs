using AutoBogus;
using Cricut.Orders.Domain.Models;
using Cricut.Orders.Infrastructure;
using FluentAssertions;

namespace Cricut.Orders.Tests.Cricut.Orders.Infrastructure
{
    [TestClass]
    public class OrderStoreTests
    {
        [TestMethod]
        public async Task CanManageOrdersInTheStore()
        {
            var newOrder = new AutoFaker<Order>()
                .Ignore(x => x.Id)
                .Generate();

            var orderStore = GetConfiguredOrderStore();

            var savedOrder = await orderStore.SaveOrderAsync(newOrder);
            savedOrder.Id.Should().NotBeNull();

            var retrievedOrder = await orderStore.GetOrderByIdAsync(savedOrder.Id!.Value);
            retrievedOrder.Should().NotBeNull();
            retrievedOrder.Should().BeEquivalentTo(savedOrder);

            await orderStore.DeleteOrderByIdAsync(retrievedOrder!.Id!.Value);
            var orderAfterDelete = await orderStore.GetOrderByIdAsync(retrievedOrder.Id!.Value);
            orderAfterDelete.Should().BeNull();
        }

        [TestMethod]
        public async Task CanGetStaticOrders()
        {
            var orderStore = GetConfiguredOrderStore();

            var allOrders = await orderStore.GetAllOrdersAsync();
            allOrders.Should().HaveCount(10);

            var customer1Orders = await orderStore.GetAllOrdersForCustomerAsync(12345);
            customer1Orders.Should().HaveCount(5);

            var customer2Orders = await orderStore.GetAllOrdersForCustomerAsync(54321);
            customer2Orders.Should().HaveCount(5);
        }

        private OrderStore GetConfiguredOrderStore() =>
            new OrderStore();
    }
}

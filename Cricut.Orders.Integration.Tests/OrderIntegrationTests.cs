using AutoBogus;
using Cricut.Orders.Api.ViewModels;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace Cricut.Orders.Integration.Tests
{
    [TestClass]
    public class OrderIntegrationTests
    {
        [DataTestMethod]
        [DataRow(3, 2, 1.5, false)]
        [DataRow(3, 2, 1.5, false)]
        [DataRow(1, 1, 25, true)]
        [DataRow(3, 4, 8, true)]
        [DataRow(1, 1, 30, true)]
        public async Task CreateNewOrder_Does_Apply_Discount(int lineItems, int quantityOfEach, double priceOfEach, bool shouldApplyDiscount)
        {
            var newOrderBelowDiscount = CreateOrderWithItems(lineItems, quantityOfEach, priceOfEach);
            var client = OrdersApiTestClientFactory.CreateTestClient();

            var request = new HttpRequestMessage(HttpMethod.Post, "v1/orders");
            request.Content = JsonContent.Create(newOrderBelowDiscount);

            var response = await client.SendAsync(request);
            response.IsSuccessStatusCode.Should().BeTrue();
            var order = await response.Content.ReadFromJsonAsync<OrderViewModel>();

            order.Should().BeEquivalentTo(newOrderBelowDiscount);

            var expectedTotal = (lineItems * quantityOfEach * priceOfEach);
            var expectedTotalMinusDiscount = expectedTotal - (expectedTotal * .1);
            if (shouldApplyDiscount)
            {
                order!.Total.Should().Be(expectedTotalMinusDiscount);
            }
            else
            {
                order!.Total.Should().Be(expectedTotal);
            }
        }

        [TestMethod]
        public async Task GetOrdersForCustomer_Returns_Orders_For_Known_Customer()
        {
            var client = OrdersApiTestClientFactory.CreateTestClient();

            var response = await client.GetAsync("v1/orders/customer/12345");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var orders = await response.Content.ReadFromJsonAsync<OrderViewModel[]>();
            orders.Should().NotBeNull();
            orders!.Length.Should().Be(5);
            orders.Should().AllSatisfy(o => o.Customer.Id.Should().Be(12345));
        }

        [TestMethod]
        public async Task GetOrdersForCustomer_Returns_Empty_For_Unknown_Customer()
        {
            var client = OrdersApiTestClientFactory.CreateTestClient();

            var response = await client.GetAsync("v1/orders/customer/99999");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var orders = await response.Content.ReadFromJsonAsync<OrderViewModel[]>();
            orders.Should().NotBeNull();
            orders!.Should().BeEmpty();
        }

        private NewOrderViewModel CreateOrderWithItems(int numberOfLineItems, int quantityOfEachItem, double priceOfEachItem)
        {
            var orderItems = new AutoFaker<OrderItemViewModel>()
                .RuleFor(x => x.Quantity, quantityOfEachItem)
                .RuleFor(x => x.Product, new AutoFaker<ProductViewModel>()
                    .RuleFor(x => x.Id, p => p.Random.Int(min: 1))
                    .RuleFor(x => x.Price, priceOfEachItem))
                .Generate(numberOfLineItems)
                .ToArray();

            return new AutoFaker<NewOrderViewModel>()
                .RuleFor(x => x.OrderItems, orderItems);
        }
    }
}

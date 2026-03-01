using AutoBogus;
using Cricut.Orders.Api.Mappings;
using Cricut.Orders.Domain.Models;
using FluentAssertions;

namespace Cricut.Orders.Tests.Cricut.Orders.Api.Mappings
{
    [TestClass]
    public class ToViewModelMappingsTests
    {
        [TestMethod]
        public void Order_ToViewModel_MapsCorrectly()
        {
            var domainModel = new AutoFaker<Order>()
                .Generate();

            var viewModel = domainModel.ToViewModel();
            viewModel.Should().BeEquivalentTo(domainModel, opts =>
                opts.Excluding(x => x.OrderItems));

            viewModel.OrderItems.Length.Should().Be(domainModel.OrderItems.Length);
            for (var i = 0; i < viewModel.OrderItems.Length; i++)
            {
                viewModel.OrderItems[i].Should().BeEquivalentTo(domainModel.OrderItems[i], opts =>
                    opts.Excluding(x => x.Total));
            }
        }
    }
}

using AutoBogus;
using Cricut.Orders.Api.Mappings;
using Cricut.Orders.Api.ViewModels;
using FluentAssertions;

namespace Cricut.Orders.Tests.Cricut.Orders.Api.Mappings
{
    [TestClass]
    public class ToDomainModelMappingsTests
    {
        [TestMethod]
        public void NewOrderViewModel_ToDomain_MapsCorrectly()
        {
            var viewModel = new AutoFaker<NewOrderViewModel>()
                .Generate();

            var domainModel = viewModel.ToDomainModel();
            domainModel.Should().BeEquivalentTo(viewModel);
            domainModel.Id.Should().BeNull();
        }
    }
}

using Cricut.Orders.Api.ViewModels;
using Cricut.Orders.Domain.Models;

namespace Cricut.Orders.Api.Mappings
{
    public static class ToDomainModelMappings
    {
        public static Order ToDomainModel(this NewOrderViewModel viewModel)
        {
            return new Order
            {
                Id = null,
                Customer = viewModel.Customer.ToDomainModel(),
                OrderItems = viewModel.OrderItems.ToDomainModel()
            };
        }

        private static Customer ToDomainModel(this CustomerViewModel viewModel)
        {
            return new Customer
            {
                Id = viewModel.Id,
                Address = viewModel.Address,
                Email = viewModel.Email,
                Name = viewModel.Name,
            };
        }

        private static OrderItem[] ToDomainModel(this IEnumerable<OrderItemViewModel> viewModels) =>
            viewModels.Select(vm => vm.ToDomainModel()).ToArray();

        private static OrderItem ToDomainModel(this OrderItemViewModel viewModel)
        {
            return new OrderItem
            {
                Product = viewModel.Product.ToDomainModel(),
                Quantity = viewModel.Quantity
            };
        }

        private static Product ToDomainModel(this ProductViewModel viewModel)
        {
            return new Product
            {
                Id = viewModel.Id,
                Name = viewModel.Name,
                Price = viewModel.Price,
            };
        }
    }
}

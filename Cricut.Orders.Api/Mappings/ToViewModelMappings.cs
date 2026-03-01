using Cricut.Orders.Api.ViewModels;
using Cricut.Orders.Domain.Models;

namespace Cricut.Orders.Api.Mappings
{
    public static class ToViewModelMappings
    {
        public static OrderViewModel ToViewModel(this Order domainModel)
        {
            return new OrderViewModel
            {
                Id = domainModel.Id ?? 0,
                Customer = domainModel.Customer.ToViewModel(),
                OrderItems = domainModel.OrderItems.ToViewModel(),
                Total = domainModel.Total
            };
        }

        private static CustomerViewModel ToViewModel(this Customer domainModel)
        {
            return new CustomerViewModel
            {
                Id = domainModel.Id,
                Address = domainModel.Address,
                Email = domainModel.Email,
                Name = domainModel.Name,
            };
        }

        private static OrderItemViewModel[] ToViewModel(this IEnumerable<OrderItem> domainModels) =>
            domainModels.Select(vm => vm.ToViewModel()).ToArray();

        private static OrderItemViewModel ToViewModel(this OrderItem domainModel)
        {
            return new OrderItemViewModel
            {
                Product = domainModel.Product.ToViewModel(),
                Quantity = domainModel.Quantity
            };
        }

        private static ProductViewModel ToViewModel(this Product domainModel)
        {
            return new ProductViewModel
            {
                Id = domainModel.Id,
                Name = domainModel.Name,
                Price = domainModel.Price,
            };
        }
    }
}

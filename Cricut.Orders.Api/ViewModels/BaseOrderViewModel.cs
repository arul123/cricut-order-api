using System.ComponentModel.DataAnnotations;

namespace Cricut.Orders.Api.ViewModels
{
    public abstract record BaseOrderViewModel
    {
        [Required]
        public CustomerViewModel Customer { get; init; } = new CustomerViewModel();

        [Required, MinLength(1)]
        public OrderItemViewModel[] OrderItems { get; init; } = Array.Empty<OrderItemViewModel>();
    }
}

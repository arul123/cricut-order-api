using System.ComponentModel.DataAnnotations;

namespace Cricut.Orders.Api.ViewModels
{
    public record OrderItemViewModel
    {
        [Required]
        public ProductViewModel Product { get; init; } = new ProductViewModel();

        [Required, Range(0, int.MaxValue)]
        public int Quantity { get; init; }
    }
}

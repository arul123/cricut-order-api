using System.ComponentModel.DataAnnotations;

namespace Cricut.Orders.Api.ViewModels
{
    public record ProductViewModel
    {
        [Required, Range(1, int.MaxValue)]
        public int Id { get; init; }

        public string? Name { get; init; }

        [Required, Range(0, double.MaxValue)]
        public double Price { get; init; }

    }
}

using System.ComponentModel.DataAnnotations;

namespace Cricut.Orders.Api.ViewModels
{
    public record CustomerViewModel
    {
        [Required]
        public int Id { get; init; }

        [Required]
        public string Name { get; init; } = string.Empty;

        public string Address { get; init; } = string.Empty;

        [Required]
        public string Email { get; init; } = string.Empty;
    }
}

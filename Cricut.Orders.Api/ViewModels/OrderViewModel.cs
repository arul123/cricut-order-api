namespace Cricut.Orders.Api.ViewModels
{
    public record OrderViewModel : BaseOrderViewModel
    {
        public int Id { get; init; }
        public OrderStatus Status { get; init; }
        public double Total { get; init; }
    }
}

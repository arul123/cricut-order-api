using Cricut.Orders.Api.Mappings;
using Cricut.Orders.Api.ViewModels;
using Cricut.Orders.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Cricut.Orders.Api.Controllers
{
    [Route("v1/orders")]
    [ApiController]
    public class OrdersController : Controller
    {
        private readonly IOrderDomain _orderDomain;

        public OrdersController(IOrderDomain orderDomain)
        {
            _orderDomain = orderDomain;
        }

        [HttpPost]
        public async Task<ActionResult<OrderViewModel>> CreateNewOrderAsync([FromBody] NewOrderViewModel newOrderVM)
        {
            var newOrder = newOrderVM.ToDomainModel();
            var savedOrder = await _orderDomain.CreateNewOrderAsync(newOrder);
            return Ok(savedOrder.ToViewModel());
        }

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<OrderViewModel[]>> GetOrdersForCustomerAsync(int customerId)
        {
            var orders = await _orderDomain.GetOrdersForCustomerAsync(customerId);
            return Ok(orders.Select(o => o.ToViewModel()).ToArray());
        }
    }
}

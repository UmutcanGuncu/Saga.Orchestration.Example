using Microsoft.AspNetCore.Mvc.Rendering;

namespace Order.API.ViewModels;

public class CreateOrderViewModel
{
    public int BuyerId { get; set; }
    public ICollection<OrderItemViewModel> OrderItems { get; set; }
}
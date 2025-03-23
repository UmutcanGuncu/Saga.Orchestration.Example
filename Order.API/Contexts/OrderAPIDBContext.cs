using Microsoft.EntityFrameworkCore;
using Order.API.Models;

namespace Order.API.Contexts;

public class OrderAPIDBContext : DbContext
{
    public OrderAPIDBContext(DbContextOptions<OrderAPIDBContext> options) : base(options)
    {
        
    }
    public DbSet<Models.Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
}
using System.ComponentModel.DataAnnotations.Schema;

namespace CornerStore.Models;

public class Order
{
    public int Id { get; set; }
    public int CashierId { get; set; }
    public Cashier Cashier { get; set; }
    public DateTime? PaidOnDate { get; set; }
    public List<OrderProduct> OrderProducts { get; set; }

    [NotMapped]
    public decimal Total => OrderProducts.Sum(op => op.Product.Price * op.Quantity);
}

namespace CornerStore.Models.DTOs;

public class OrderDTO
{
    public int Id { get; set; }
    public int CashierId { get; set; }
    public CashierDTO Cashier { get; set; } 
    public DateTime? PaidOnDate { get; set; }
    public decimal Total { get; set; }
    public List<OrderProductDTO> OrderProducts { get; set; }
}


// This is the DTO we are receiving, so when someone creates an order we know were getting a CashierId, the Date, and the ProductIds.
public class CreateOrderDTO
{
    public int CashierId { get; set; }
    public DateTime? PaidOnDate { get; set; }
    public List<int> ProductIds { get; set; }
}
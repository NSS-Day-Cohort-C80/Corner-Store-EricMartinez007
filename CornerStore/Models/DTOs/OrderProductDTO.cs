namespace CornerStore.Models.DTOs;

public class OrderProductDTO
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public ProductDTO Product { get; set; }
}

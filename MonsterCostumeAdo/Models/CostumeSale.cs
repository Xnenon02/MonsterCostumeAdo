namespace MonsterCostumeAdo.Models;

public class CostumeSale
{
    public int Id { get; set; }
    public int CostumeId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime SaleDate { get; set; }
    public string? Notes { get; set; }

    // Navigation property
    public Costume Costume { get; set; } = null!;
}

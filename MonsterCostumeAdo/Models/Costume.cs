namespace MonsterCostumeAdo.Models;

public class Costume
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Monster { get; set; } = string.Empty;
    public int Stock { get; set; }
    public decimal Price { get; set; }

    // Navigation property
    public List<CostumeSale> CostumeSales { get; set; } = new();
}

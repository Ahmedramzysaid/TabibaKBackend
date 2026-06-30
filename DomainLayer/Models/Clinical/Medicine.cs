namespace DomainLayer.Models;

public class Medicine
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ArabicName { get; set; } = string.Empty;
    public string ArabicNameNormalized { get; set; } = string.Empty;
    public string? Price { get; set; }
    public string? Company { get; set; }
    public string? ActiveIngredient { get; set; }
    public string? Description { get; set; }
    public string? ProductUrl { get; set; }
}

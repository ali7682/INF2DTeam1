public class DiscountModel
{
    public int ID { get; init; }
    public string? Code { get; set; }
    public decimal Percentage { get; set; } 
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? LocationsAllowed { get; set; }
    public string? TimesAllowed { get; set; }
    public string? Conditions { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int MaxUses { get; set; }
    public int Uses { get; set; }
    public bool IsActive { get; set; } 
    public string? AllowedPlates { get; set; } 
}
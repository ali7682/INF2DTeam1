public class DiscountModel
{
    public int ID { get; init; }
    public string? Code { get; set; }
    public DateTime? Percentage { get; set; }
    public DateTime? ValidFrom { get; set; }
    public int? ValidTo { get; set; }
    public string? LocationsAllowed { get; set; }
    public string? TimesAllowed { get; set; }
    public string? Conditions { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int MaxUses { get; set; }
    public int Uses { get; set; }
    public int IsActive { get; set; }
}

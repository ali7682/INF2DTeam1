public sealed class PaymentModel
{
    public int TransactionId { get; init; }
    public string Transaction { get; init; }
    public decimal? Amount { get; set; }
    public string? Initiator { get; set; }
    public DateTime? Created_at { get; set; }
    public int? Completed { get; set; }
    public string? Hash { get; set; }
    public PaymentDetailsModel? T_data { get; set; }
    public int? DiscountCodeId { get; set; }
}
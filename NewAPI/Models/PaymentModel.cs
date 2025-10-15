public sealed class PaymentModel
{
    public int TransactionId { get; init; }
    public double? Amount { get; set; }
    public string? Initiator { get; set; }
    public DateTime? Created_at { get; set; }
    public DateTime? Completed { get; set; }
    public string? Hash { get; set; }
    public PaymentDetailModel? T_data { get; set; }
}
public sealed class PaymentModel
{
    public int TransactionId { get; init; }
    public double? Amount { get; set; }
    public string? Initiator { get; set; }
    public DateTime? Created_at { get; set; }
    public DateTime? Completed { get; set; }
    public string? Hash { get; set; }
    public PaymentDetailModel? t_data { get; set; }
}
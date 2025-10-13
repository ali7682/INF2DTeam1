public sealed class PaymentDetailModel
{
    public int Id { get; init; }
    public int TransactionId { get; init; }
    public double? Amount { get; set; }
    public DateTime? Date { get; set; }
    public string? Method { get; set; }
    public string? Issuer { get; set; }
    public string? Bank { get; set; }
}
public sealed class PaymentDetailModel
{
    public readonly int Id { get; set; }
    public readonly int TransactionId { get; set; }
    public double? Amount { get; set; }
    public DateTime? Date { get; set; }
    public string? Method { get; set; }
    public string? Issuer { get; set; }
    public string? Bank { get; set; }
}
public class PaymentModel
{
    public string TransactionId { get; set; }

    public double Amount { get; set; }

    public string Initiator { get; set; }

    public string Created_at { get; set; }
    
    public string Completed { get; set; }

    public string Hash { get; set; }

    public PaymentModel(string transactionId, double amount, string initiator, string created_at, string completed, string hash)
    {
        TransactionId = transactionId;
        Amount = amount;
        Initiator = initiator;
        Created_at = created_at;
        Completed = completed;
        Hash = hash;
    }
}

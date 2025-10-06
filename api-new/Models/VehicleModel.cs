public class VehicleModel
{
    public Int64 ID { get; set; }
    public Char TransactionID { get; set; }
    public Decimal Amount { get; set; }

    public String Initatior { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime Completed { get; set; }

    public Char Hash { get; set; }

    public VehicleModel(Int64 id, char transactionid, decimal amount, string initatior, DateTime createdAt, DateTime completed, Char hash)
    {
        ID = id;
        TransactionID = transactionid;
        Amount = amount;
        Initatior = initatior;
        CreatedAt = createdAt;
        Completed = completed;
        Hash = hash ;
    }
    
    public VehicleModel() { }
}

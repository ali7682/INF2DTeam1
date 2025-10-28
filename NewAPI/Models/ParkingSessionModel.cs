public class ParkingSessionModel
{
    public int ID { get; init; }
    public int? ParkingLotID { get; set; }
    public string? LicensePlate { get; set; }
    public DateTime? Started { get; set; }
    public DateTime? Stopped { get; set; }
    public string? User { get; set; }
    public int? DurationMinutes { get; set; }
    public double? Cost { get; set; }
    public string? PaymentStatus { get; set; }
}

public class ReservationModel
{
    public readonly int ID { get; set; }
    public readonly int UserID { get; set; }
    public readonly int ParkinglotID { get; set; }
    public readonly int VehicleID { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Enum? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal Cost { get; set; }
}
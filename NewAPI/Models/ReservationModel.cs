public class ReservationModel
{
    public int ID { get; init; }
    public int UserID { get; init; }
    public int ParkinglotID { get; init; }
    public int VehicleID { get; init; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal Cost { get; set; }
}
public class ReservationModel
{
    public int ID { get; set; }
    public int UserID { get; set; }
    public int ParkinglotID { get; set; }
    public int VehicleID { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Enum? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal Cost { get; set; }

    public ReservationModel(int id, int userID, int parkinglotID, int vehicleID, DateTime startTime, DateTime endTime, Enum status, DateTime createdAt, decimal cost)
    {
        ID = id;
        UserID = userID;
        ParkinglotID = parkinglotID;
        VehicleID = vehicleID;
        StartTime = startTime;
        EndTime = endTime;
        Status = status;
        CreatedAt = createdAt;
        Cost = cost;
    }
}
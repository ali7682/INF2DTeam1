using Microsoft.VisualBasic;

public class VehicleModel
{
    public readonly int ID { get; set; }
    public readonly int UserID { get; set; }
    public String LicensePlate{ get; set; }

    public String Make { get; set; }

    public String Model { get; set; }

    public String Color { get; set; }

    public int Year { get; set; }
    
    public readonly DateTime CreatedAt { get; set; }

    public VehicleModel(int id, int userID, String licensePlate, String make, String model, String color, int year, DateTime createdAt)
    {
        ID = id;
        UserID = userID;
        LicensePlate = licensePlate;
        Make = make;
        Model = model;
        Color = color;
        Year = year;
        CreatedAt = createdAt;
    }
}

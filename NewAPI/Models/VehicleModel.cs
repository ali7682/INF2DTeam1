using Microsoft.VisualBasic;

public class VehicleModel
{
    public int ID { get; init; }
    public int UserID { get; init; }
    public DateTime CreatedAt { get; init; }

    public string LicensePlate { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public string Color { get; set; }
    public int Year { get; set; }
}
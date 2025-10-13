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

}

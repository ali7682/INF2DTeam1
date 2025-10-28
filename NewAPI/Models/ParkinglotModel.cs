public class ParkingLotModel
{
    public int ID { get; init; }
    public string? Name { get; set; }
    public string? Location { get; set; }
    public string? Address { get; set; }
    public int? Capacity { get; set; }
    public int? Reserved { get; set; }
    public decimal? Tariff { get; set; }
    public decimal? DayTariff { get; set; }
    public DateTime? CreatedAt { get; set; }
}


// A parking-lot DTO to help calculate and return the total profit of a given parking-lot
public class ParkingLotRevenueDto
{
    public int ParkingLotId { get; set; }
    public long TotalSessions { get; set; }
    public decimal TotalRevenue { get; set; }
}

// A parking-lot DTO to help calculate and return the occupancy grade per parking lot
public class ParkingLotOccupancyDto
{
    public int ParkingLotId { get; set; }
    public long TotalSessions { get; set; }
}
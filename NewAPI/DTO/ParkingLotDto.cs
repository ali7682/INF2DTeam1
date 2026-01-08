
// A parking-lot DTO to help calculate and return the total profit of a given parking-lot
public record ParkingLotRevenueDto(
    int ParkingLotId,
    int TotalSessions,
    decimal TotalRevenue
);


public record ParkingLotOccupancyDto(
    int ParkingLotId,
    int TotalSessions
);

// A parking-lot DTO to help calculate and return the total profit of a given parking-lot
public record ParkingLotDto(
    int ParkingLotId,
    int TotalSessions,
    decimal TotalRevenue
);
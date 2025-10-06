public class ParkinglotModel
{
    public int ID { get; set; }
    public string? Name { get; set; }
    public string? Location { get; set; }
    public string? Address { get; set; }
    public int Capacity { get; set; }
    public int Reserved { get; set; }
    public decimal Tariff { get; set; }
    public decimal DayTariff { get; set; }
    public DateTime CreatedAt { get; set; }

    public ParkinglotModel(int id, string name, string location, string address, int capacity, int reserved, decimal tariff, decimal dayTariff, DateTime createdAt)
    {
        ID = id;
        Name = name;
        Location = location;
        Address = address;
        Capacity = capacity;
        Reserved = reserved;
        Tariff = tariff;
        DayTariff = dayTariff;
        CreatedAt = createdAt;
    }
}
namespace NewAPI.Models
{
    public class FeedbackModel
    {
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string Description { get; set; }
        public int ParkingSessionId { get; set; }
    }
}

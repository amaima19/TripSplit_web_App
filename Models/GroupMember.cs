namespace TripSplit.Models
{
    public class GroupMember
    {
        public int Id { get; set; } 
        public String UserId { get; set; }
        public int GroupId { get; set; }
        public string Role { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; }
    }
}

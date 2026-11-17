namespace TripSplit.Models
{
    public class Group
    {
        public int Id {  get; set; }
        public string Name {  get; set; }
        public string Description { get; set; } 
        public string createdBy {  get; set; }
        
        public bool IsActive {  get; set; }

    }
}

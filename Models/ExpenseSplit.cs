namespace TripSplit.Models
{
    public class ExpenseSplit
    {
        public int Id { get; set; }
        public int ExpenseId { set; get; }
        public string UserId { set; get; }
        public int SharedAmount { set; get; }
        public DateTime CreateAt { set; get; }
    }
}

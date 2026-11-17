namespace TripSplit.Models
{
    public class Expense
    {
        public int ExpenseId { get; set; }
        public int GroupId { get; set; }
        public string Description { get; set; }
        public int Amount { get; set; }
        public string PaidById { get; set; }
        public string AddedById { get; set; }
        public DateTime Date { get; set; }
    }
}

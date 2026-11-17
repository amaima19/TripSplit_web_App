using System;

namespace TripSplit.Models.ViewModel
{
    public class ExpenseRow
    {
        public string Description { get; set; }

        public int Amount { get; set; }

        public string PaidByName { get; set; }

        public DateTime Date { get; set; }
    }
}
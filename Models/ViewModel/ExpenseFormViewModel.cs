using System.Collections.Generic;

namespace TripSplit.Models.ViewModel
{
    public class ExpenseFormViewModel
    {
        public int GroupId { get; set; }

        public string GroupName { get; set; }

        public string Description { get; set; }

        public int Amount { get; set; }

        public string PaidById { get; set; }

        public string Error { get; set; }

        public List<MemberOption> Members { get; set; }
            = new List<MemberOption>();

        public List<string> SharedWith { get; set; }
            = new List<string>();
    }
}
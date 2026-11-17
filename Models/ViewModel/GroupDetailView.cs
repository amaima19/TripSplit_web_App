using System.Collections.Generic;
using TripSplit.Models;

namespace TripSplit.Models.ViewModel
{
    public class GroupDetailsViewModel
    {
        public Group Group { get; set; }

        public bool IsAdmin { get; set; }

        public List<MemberRow> Members { get; set; }
            = new List<MemberRow>();

        public List<ExpenseRow> Expenses { get; set; }
            = new List<ExpenseRow>();
    }
}
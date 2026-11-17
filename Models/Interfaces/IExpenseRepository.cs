namespace TripSplit.Models.Interfaces
{
    public interface IExpenseRepository
    {
       List< Expense> GetByGroup(int groupId);
        void Save(Expense expense);
        void Add(Expense expense,List<ExpenseSplit>splits);
    }
}

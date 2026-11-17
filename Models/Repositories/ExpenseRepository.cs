
using Dapper;
using TripSplit.Data;
using TripSplit.Models.Interfaces;

namespace TripSplit.Models.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly DbConnectionFactory _factory;

        public ExpenseRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public void Add(Expense expense, List<ExpenseSplit> splits)
        {
            using var db = _factory.CreateConnection();
            db.Open();

            using var transaction = db.BeginTransaction();

            try
            {
                string expenseSql = @"INSERT INTO Expenses
                    (GroupId, Description, Amount, PaidById, AddedById, Date)
                    VALUES
                    (@GroupId, @Description, @Amount, @PaidById, @AddedById, @Date);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                expense.ExpenseId = db.ExecuteScalar<int>(
                    expenseSql, expense, transaction);

                string splitSql = @"INSERT INTO ExpenseSplits
                    (ExpenseId, UserId, SharedAmount, CreateAt)
                    VALUES
                    (@ExpenseId, @UserId, @SharedAmount, @CreateAt)";

                foreach (var split in splits)
                {
                    split.ExpenseId = expense.ExpenseId;
                }

                db.Execute(splitSql, splits, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Expense> GetByGroup(int groupId)
        {
            using var db = _factory.CreateConnection();

            string sql = @"SELECT *
                           FROM Expenses
                           WHERE GroupId = @GroupId
                           ORDER BY Date DESC";

            return db.Query<Expense>(sql, new { GroupId = groupId }).ToList();
        }

        public void Save(Expense expense)
        {
            using var db = _factory.CreateConnection();

            string sql = @"UPDATE Expenses
                           SET Description = @Description,
                               Amount = @Amount,
                               PaidById = @PaidById
                           WHERE ExpenseId = @ExpenseId";

            db.Execute(sql, expense);
        }
    }
}

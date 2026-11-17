
using Dapper;
using TripSplit.Data;
using TripSplit.Models.Interfaces;

namespace TripSplit.Models.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly DbConnectionFactory _factory;

        public GroupRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public void Add(Group group)
        {
            using var db = _factory.CreateConnection();

            string sql = @"INSERT INTO Groups
                           (Name, Description, createdBy, IsActive)
                           VALUES
                           (@Name, @Description, @createdBy, @IsActive);
                           SELECT CAST(SCOPE_IDENTITY() AS INT);";

            group.Id = db.ExecuteScalar<int>(sql, group);
        }

        public Group? GetById(int id)
        {
            using var db = _factory.CreateConnection();

            string sql = "SELECT * FROM Groups WHERE Id = @Id";

            return db.QueryFirstOrDefault<Group>(sql, new { Id = id });
        }

        public List<Group> GetGroupsForUser(string userId)
        {
            using var db = _factory.CreateConnection();

            string sql = @"SELECT g.*
                           FROM Groups g
                           INNER JOIN GroupMembers m
                           ON g.Id = m.GroupId
                           WHERE m.UserId = @UserId
                           AND m.IsActive = 1
                           AND g.IsActive = 1";

            return db.Query<Group>(sql, new { UserId = userId }).ToList();
        }

        public void Save(Group group)
        {
            using var db = _factory.CreateConnection();

            string sql = @"UPDATE Groups
                           SET Name = @Name,
                               Description = @Description,
                               IsActive = @IsActive
                           WHERE Id = @Id";

            db.Execute(sql,group);
        }
    }
}

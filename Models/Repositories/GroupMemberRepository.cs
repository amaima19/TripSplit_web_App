
using Dapper;
using TripSplit.Data;
using TripSplit.Models.Interfaces;

namespace TripSplit.Models.Repositories
{
    public class GroupMemberRepository : IGroupMemberRepository
    {
        private readonly DbConnectionFactory _factory;

        public GroupMemberRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public void AddMember(GroupMember member)
        {
            using var db = _factory.CreateConnection();

            string sql = @"INSERT INTO GroupMembers
                           (UserId, GroupId, Role, JoinedAt, IsActive)
                           VALUES
                           (@UserId, @GroupId, @Role, @JoinedAt, @IsActive)";

            db.Execute(sql, member);
        }

        public List<GroupMember> GetActiveMembers(int groupId)
        {
            using var db = _factory.CreateConnection();

            string sql = @"SELECT *
                           FROM GroupMembers
                           WHERE GroupId = @GroupId
                           AND IsActive = 1";

            return db.Query<GroupMember>(sql, new { GroupId = groupId }).ToList();
        }

        public GroupMember? GetMembership(int groupId, string userId)
        {
            using var db = _factory.CreateConnection();

            string sql = @"SELECT *
                           FROM GroupMembers
                           WHERE GroupId = @GroupId
                           AND UserId = @UserId
                           AND IsActive = 1";

            return db.QueryFirstOrDefault<GroupMember>(
                sql, new { GroupId = groupId, UserId = userId });
        }

        public void saveMember(GroupMember member)
        {
            using var db = _factory.CreateConnection();

            string sql = @"UPDATE GroupMembers
                           SET Role = @Role,
                               IsActive = @IsActive
                           WHERE Id = @Id";

            db.Execute(sql, member);
        }
    }
}

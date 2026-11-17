namespace TripSplit.Models.Interfaces
{
    public interface IGroupMemberRepository
    {
       void AddMember(GroupMember member);
        List<GroupMember> GetActiveMembers(int groupId);
        GroupMember? GetMembership(int groupId,String userId);
        void saveMember(GroupMember member);

    }
}

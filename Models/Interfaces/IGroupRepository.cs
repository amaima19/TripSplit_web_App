namespace TripSplit.Models.Interfaces
{
    public interface IGroupRepository
    {
        void Add(Group group);
       Group ? GetById(int id);
        List<Group> GetGroupsForUser(string userId);
        void Save(Group group);
    }
}

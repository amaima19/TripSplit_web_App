
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TripSplit.Data;
using TripSplit.Models;
using TripSplit.Models.Interfaces;
using TripSplit.Models.ViewModel;

namespace TripSplit.Controllers
{
    [Authorize]
    public class GroupController : Controller
    {
        private readonly IGroupRepository _groups;
        private readonly IGroupMemberRepository _members;
        private readonly IExpenseRepository _expenses;
        private readonly UserManager<ApplicationUser> _users;

        public GroupController(
            IGroupRepository groups,
            IGroupMemberRepository members,
            IExpenseRepository expenses,
            UserManager<ApplicationUser> users)
        {
            _groups = groups;
            _members = members;
            _expenses = expenses;
            _users = users;
        }

        // Get user names using Identity
        private Dictionary<string, string> NamesFor(List<string> ids)
        {
            Dictionary<string, string> names =
                new Dictionary<string, string>();

            foreach (var user in _users.Users)
            {
                foreach (var id in ids)
                {
                    if (user.Id == id && !names.ContainsKey(user.Id))
                    {
                        if (string.IsNullOrWhiteSpace(user.FullName))
                        {
                            names.Add(user.Id, user.Email ?? "User");
                        }
                        else
                        {
                            names.Add(user.Id, user.FullName);
                        }
                    }
                }
            }

            return names;
        }

        // My Groups
        public IActionResult Index()
        {
            string uid = _users.GetUserId(User)!;

            List<GroupCardViewModel> cards =
                new List<GroupCardViewModel>();

            List<Group> groups = _groups.GetGroupsForUser(uid);

            foreach (var group in groups)
            {
                List<GroupMember> members =
                    _members.GetActiveMembers(group.Id);

                string role = "Member";

                foreach (var member in members)
                {
                    if (member.UserId == uid)
                    {
                        role = member.Role;
                    }
                }

                List<string> otherIds = new List<string>();

                foreach (var member in members)
                {
                    if (member.UserId != uid)
                    {
                        otherIds.Add(member.UserId);
                    }
                }

                Dictionary<string, string> names = NamesFor(otherIds);

                List<string> memberNames = new List<string>();

                foreach (var name in names.Values)
                {
                    memberNames.Add(name);
                }

                string displayNames = "Just you so far";

                if (memberNames.Count > 0)
                {
                    displayNames = string.Join(", ", memberNames);
                }

                GroupCardViewModel card = new GroupCardViewModel();

                card.Id = group.Id;
                card.Name = group.Name;
                card.Role = role;
                card.MemberNames = displayNames;

                cards.Add(card);
            }

            return View(cards);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ViewBag.Error = "Please enter a group name.";
                return View();
            }

            string uid = _users.GetUserId(User)!;

            Group group = new Group();

            group.Name = name.Trim();
            group.Description = description?.Trim() ?? "";
            group.createdBy = uid;
            group.IsActive = true;

            _groups.Add(group);

            GroupMember member = new GroupMember();

            member.GroupId = group.Id;
            member.UserId = uid;
            member.Role = "Admin";
            member.JoinedAt = DateTime.Now;
            member.IsActive = true;

            _members.AddMember(member);

            return RedirectToAction("Details", new { id = group.Id });
        }

        public IActionResult Details(int id)
        {
            string uid = _users.GetUserId(User)!;

            GroupMember me = _members.GetMembership(id, uid);

            if (me == null)
            {
                return NotFound();
            }

            Group group = _groups.GetById(id);

            if (group == null || !group.IsActive)
            {
                return NotFound();
            }

            List<GroupMember> members =
                _members.GetActiveMembers(id);

            List<Expense> expenses =
                _expenses.GetByGroup(id);

            List<string> ids = new List<string>();

            foreach (var member in members)
            {
                ids.Add(member.UserId);
            }

            foreach (var expense in expenses)
            {
                ids.Add(expense.PaidById);
            }

            Dictionary<string, string> names = NamesFor(ids);

            GroupDetailsViewModel vm = new GroupDetailsViewModel();

            vm.Group = group;
            vm.IsAdmin = me.Role == "Admin";

            foreach (var member in members)
            {
                string memberName = "Unknown";

                if (names.ContainsKey(member.UserId))
                {
                    memberName = names[member.UserId];
                }

                MemberRow row = new MemberRow();

                row.Name = memberName;
                row.Role = member.Role;

                vm.Members.Add(row);
            }

            foreach (var expense in expenses)
            {
                string paidByName = "Unknown";

                if (names.ContainsKey(expense.PaidById))
                {
                    paidByName = names[expense.PaidById];
                }

                ExpenseRow row = new ExpenseRow();

                row.Description = expense.Description;
                row.Amount = expense.Amount;
                row.PaidByName = paidByName;
                row.Date = expense.Date;

                vm.Expenses.Add(row);
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int groupId, string email)
        {
            string uid = _users.GetUserId(User)!;

            GroupMember me = _members.GetMembership(groupId, uid);

            if (me == null || me.Role != "Admin")
            {
                return Forbid();
            }

            var user = await _users.FindByEmailAsync(email.Trim());

            if (user == null)
            {
                TempData["Error"] =
                    "No registered user has that email.";
            }
            else
            {
                GroupMember existing =
                    _members.GetMembership(groupId, user.Id);

                if (existing != null)
                {
                    TempData["Error"] =
                        "That person is already in this group.";
                }
                else
                {
                    GroupMember member = new GroupMember();

                    member.GroupId = groupId;
                    member.UserId = user.Id;
                    member.Role = "Member";
                    member.JoinedAt = DateTime.Now;
                    member.IsActive = true;

                    _members.AddMember(member);

                    TempData["Message"] = "Member added.";
                }
            }

            return RedirectToAction("Details", new { id = groupId });
        }
    }
}

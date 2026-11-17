
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
    public class ExpenseController : Controller
    {
        private readonly IGroupRepository _groups;
        private readonly IGroupMemberRepository _members;
        private readonly IExpenseRepository _expenses;
        private readonly UserManager<ApplicationUser> _users;

        public ExpenseController(
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

        private List<MemberOption> MemberOptions(int groupId)
        {
            List<MemberOption> options = new List<MemberOption>();

            List<GroupMember> members =
                _members.GetActiveMembers(groupId);

            foreach (var user in _users.Users)
            {
                foreach (var member in members)
                {
                    if (user.Id == member.UserId)
                    {
                        MemberOption option = new MemberOption();

                        option.UserId = user.Id;

                        if (string.IsNullOrWhiteSpace(user.FullName))
                        {
                            option.Name = user.Email ?? "User";
                        }
                        else
                        {
                            option.Name = user.FullName;
                        }

                        options.Add(option);
                    }
                }
            }

            return options;
        }

        [HttpGet]
        public IActionResult Create(int groupId)
        {
            string uid = _users.GetUserId(User)!;

            GroupMember me = _members.GetMembership(groupId, uid);

            if (me == null)
            {
                return NotFound();
            }

            Group group = _groups.GetById(groupId);

            if (group == null || !group.IsActive)
            {
                return NotFound();
            }

            List<MemberOption> options = MemberOptions(groupId);

            ExpenseFormViewModel vm = new ExpenseFormViewModel();

            vm.GroupId = groupId;
            vm.GroupName = group.Name;
            vm.PaidById = uid;
            vm.Members = options;

            foreach (var option in options)
            {
                vm.SharedWith.Add(option.UserId);
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ExpenseFormViewModel vm)
        {
            string uid = _users.GetUserId(User)!;

            GroupMember me = _members.GetMembership(vm.GroupId, uid);

            if (me == null)
            {
                return NotFound();
            }

            Group group = _groups.GetById(vm.GroupId);

            if (group == null || !group.IsActive)
            {
                return NotFound();
            }

            List<MemberOption> options = MemberOptions(vm.GroupId);

            List<string> memberIds = new List<string>();

            foreach (var option in options)
            {
                memberIds.Add(option.UserId);
            }

            List<string> shared = new List<string>();

            foreach (var selectedId in vm.SharedWith)
            {
                foreach (var memberId in memberIds)
                {
                    if (selectedId == memberId &&
                        !shared.Contains(selectedId))
                    {
                        shared.Add(selectedId);
                    }
                }
            }

            string error = "";

            if (string.IsNullOrWhiteSpace(vm.Description))
            {
                error = "Please enter what the expense was for.";
            }
            else if (vm.Amount < 1)
            {
                error = "Enter an amount of 1 or more.";
            }
            else if (!memberIds.Contains(vm.PaidById))
            {
                error = "Choose who paid.";
            }
            else if (shared.Count == 0)
            {
                error = "Choose at least one person.";
            }

            if (error != "")
            {
                vm.Error = error;
                vm.GroupName = group.Name;
                vm.Members = options;

                return View(vm);
            }

            Expense expense = new Expense();

            expense.GroupId = vm.GroupId;
            expense.Description = vm.Description.Trim();
            expense.Amount = vm.Amount;
            expense.PaidById = vm.PaidById;
            expense.AddedById = uid;
            expense.Date = DateTime.Now;

            List<ExpenseSplit> splits = new List<ExpenseSplit>();

            int baseShare = vm.Amount / shared.Count;
            int remainder = vm.Amount % shared.Count;

            int counter = 0;

            foreach (var userId in shared)
            {
                ExpenseSplit split = new ExpenseSplit();

                split.UserId = userId;
                split.SharedAmount = baseShare;

                if (counter < remainder)
                {
                    split.SharedAmount++;
                }

                split.CreateAt = DateTime.Now;

                splits.Add(split);

                counter++;
            }

            _expenses.Add(expense, splits);

            return RedirectToAction(
                "Details", "Group", new { id = vm.GroupId });
        }
    }
}

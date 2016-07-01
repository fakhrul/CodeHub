using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Users;
using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class TeamMembersViewModel : UsersViewModel
    {
        private readonly IApplicationService _applicationService;

        public long TeamId { get; }

        public TeamMembersViewModel(long teamId)
        {
            _applicationService = GetService<IApplicationService>();

            TeamId = teamId;
            Title = "Members";
            EmptyMessage = "There are no team members.";
        }

        protected override async Task<bool> Load(ReactiveList<BasicUserModel> users, int page)
        {
            var request = _applicationService.Client.Teams[TeamId].GetMembers(page);
            var items = await _applicationService.Client.ExecuteAsync(request);
            users.AddRange(items.Data);
            return items.More != null;
        }
    }
}
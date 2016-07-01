using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Users;
using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationMembersViewModel : UsersViewModel
    {
        private readonly IApplicationService _applicationService;

        public string OrganizationName { get; }

        public OrganizationMembersViewModel(string organizationName)
        {
            _applicationService = GetService<IApplicationService>();

            OrganizationName = organizationName;
            Title = "Members";
            EmptyMessage = "There are no members.";
        }

        protected override async Task<bool> Load(ReactiveList<BasicUserModel> users, int page)
        {
            var request = _applicationService.Client.Organizations[OrganizationName].GetMembers(page);
            var items = await _applicationService.Client.ExecuteAsync(request);
            users.AddRange(items.Data);
            return items.More != null;
        }
    }
}


using System.Threading.Tasks;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Users
{
    public class UserFollowingsViewModel : UsersViewModel
    {
        private readonly IApplicationService _applicationService;

        public string Username { get; }

        public UserFollowingsViewModel(string username)
        {
            _applicationService = GetService<IApplicationService>();

            Username = username;
            Title = "Following";
            EmptyMessage = "There are no followers.";
        }

        protected override async Task<bool> Load(ReactiveList<BasicUserModel> users, int page)
        {
            var request = _applicationService.Client.Users[Username].GetFollowing(page);
            var items = await _applicationService.Client.ExecuteAsync(request);
            users.AddRange(items.Data);
            return items.More != null;
        }
    }
}
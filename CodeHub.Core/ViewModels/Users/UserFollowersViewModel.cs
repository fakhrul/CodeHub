using System.Threading.Tasks;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Users
{
    public class UserFollowersViewModel : UsersViewModel
    {
        private readonly IApplicationService _applicationService;

        public string Username { get; }

        public UserFollowersViewModel(string username)
        {
            _applicationService = GetService<IApplicationService>();

            Username = username;
            Title = "Followers";
            EmptyMessage = "There are no followers.";
        }

        protected override async Task<bool> Load(ReactiveList<BasicUserModel> users, int page)
        {
            var request = _applicationService.Client.Users[Username].GetFollowers(page);
            var items = await _applicationService.Client.ExecuteAsync(request);
            users.AddRange(items.Data);
            return items.More != null;
        }
    }
}


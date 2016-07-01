using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Users;
using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoryStargazersViewModel : UsersViewModel
    {
        private readonly IApplicationService _applicationService;

        public string Username { get; }

        public string Repository { get; }

        public RepositoryStargazersViewModel(string username, string repository)
        {
            _applicationService = GetService<IApplicationService>();
            Username = username;
            Repository = repository;
            Title = "Stargazers";
            EmptyMessage = "There are no stargazers.";
        }

        protected override async Task<bool> Load(ReactiveList<BasicUserModel> users, int page)
        {
            var request = _applicationService.Client.Users[Username].Repositories[Repository].GetStargazers(page);
            var items = await _applicationService.Client.ExecuteAsync(request);
            users.AddRange(items.Data);
            return items.More != null;
        }
    }

    public class RepositoryWatchersViewModel : UsersViewModel
    {
        private readonly IApplicationService _applicationService;

        public string Username { get; }

        public string Repository { get; }

        public RepositoryWatchersViewModel(string username, string repository)
        {
            _applicationService = GetService<IApplicationService>();
            Username = username;
            Repository = repository;
            Title = "Watchers";
            EmptyMessage = "There are no watchers.";
        }

        protected override async Task<bool> Load(ReactiveList<BasicUserModel> users, int page)
        {
            var request = _applicationService.Client.Users[Username].Repositories[Repository].GetWatchers(page);
            var items = await _applicationService.Client.ExecuteAsync(request);
            users.AddRange(items.Data);
            return items.More != null;
        }
    }
}


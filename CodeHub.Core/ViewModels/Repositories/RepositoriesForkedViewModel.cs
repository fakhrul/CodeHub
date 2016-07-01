using System.Threading.Tasks;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesForkedViewModel : RepositoriesViewModel
    {
        private readonly IApplicationService _applicationService;

        public string Username { get; }

        public string Repository { get; }

        public RepositoriesForkedViewModel(string username, string repository)
        {
            _applicationService = GetService<IApplicationService>();

            Username = username;
            Repository = repository;
            Title = "Forks";
        }

        protected override async Task<bool> Load(IReactiveList<RepositoryModel> repositories, int page)
        {
            var request = _applicationService.Client.Users[Username].Repositories[Repository].GetForks(page: page);
            var items = await _applicationService.Client.ExecuteAsync(request);
            repositories.AddRange(items.Data);
            return items.More != null;
        }
    }
}


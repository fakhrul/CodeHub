using System.Threading.Tasks;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesWatchedViewModel : RepositoriesViewModel
    {
        private readonly IApplicationService _applicationService;

        public RepositoriesWatchedViewModel()
        {
            _applicationService = GetService<IApplicationService>();

            Title = "Watched";
            ShowRepositoryOwner = true;
        }

        protected override async Task<bool> Load(IReactiveList<RepositoryModel> repositories, int page)
        {
            var request = _applicationService.Client.AuthenticatedUser.Repositories.GetWatching(page);
            var items = await _applicationService.Client.ExecuteAsync(request);
            repositories.AddRange(items.Data);
            return items.More != null;
        }
    }
}


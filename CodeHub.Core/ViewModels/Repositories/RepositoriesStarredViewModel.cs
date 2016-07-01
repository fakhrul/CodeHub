using System.Threading.Tasks;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesStarredViewModel : RepositoriesViewModel
    {
        private readonly IApplicationService _applicationService;

        public RepositoriesStarredViewModel()
        {
            _applicationService = GetService<IApplicationService>();

            Title = "Starred";
        }

        protected override async Task<bool> Load(IReactiveList<RepositoryModel> repositories, int page)
        {
            var request = _applicationService.Client.AuthenticatedUser.Repositories.GetStarred(page);
            var items = await _applicationService.Client.ExecuteAsync(request);
            repositories.AddRange(items.Data);
            return items.More != null;
        }
    }
}


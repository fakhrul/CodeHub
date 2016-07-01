using System.Threading.Tasks;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class OrganizationRepositoriesViewModel : RepositoriesViewModel
    {
        private readonly IApplicationService _applicationService;

        public string OrganizationName { get; }

        public OrganizationRepositoriesViewModel(string organizationName)
        {
            _applicationService = GetService<IApplicationService>();

            Title = organizationName;
            OrganizationName = organizationName;
            ShowRepositoryOwner = false;
        }

        protected override async Task<bool> Load(IReactiveList<RepositoryModel> repositories, int page)
        {
            var request = _applicationService.Client.Organizations[OrganizationName].Repositories.GetAll(page);
            var items = await _applicationService.Client.ExecuteAsync(request);
            repositories.AddRange(items.Data);
            return items.More != null;
        }
    }
}


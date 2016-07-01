using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using GitHubSharp;
using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class UserRepositoriesViewModel : RepositoriesViewModel
    {
        private readonly IApplicationService _applicationService;

        public string Username { get; }

        public UserRepositoriesViewModel(string username)
        {
            _applicationService = GetService<IApplicationService>();

            Username = username;
            ShowRepositoryOwner = false;
        }

        protected override async Task<bool> Load(IReactiveList<RepositoryModel> repositories, int page)
        {
            GitHubRequest<List<RepositoryModel>> request;
            if (string.Equals(this.GetApplication().Account.Username, Username, StringComparison.OrdinalIgnoreCase))
                request = this.GetApplication().Client.AuthenticatedUser.Repositories.GetAll("owner", page);
            else
                request = this.GetApplication().Client.Users[Username].Repositories.GetAll(page);

            var items = await _applicationService.Client.ExecuteAsync(request);
            repositories.AddRange(items.Data);
            return items.More != null;
        }
    }
}

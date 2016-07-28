using System;
using GitHubSharp.Models;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using CodeHub.Core.Utilities;
using CodeHub.Core.Utils;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesExploreViewModel : BaseViewModel, IListViewModel<RepositoryItemViewModel>
    {
        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { this.RaiseAndSetIfChanged(ref _searchText, value); }
        }

        public IReactiveCommand<Unit> SearchCommand { get; }

        public IReadOnlyReactiveList<RepositoryItemViewModel> Items { get; }

        public bool IsEmpty => false;

        public RepositoriesExploreViewModel()
        {
            var applicationService = GetService<IApplicationService>();
            var showDescription = applicationService.Account.ShowRepositoryDescriptionInList;

            Title = "Explore";

            var repositories = new ReactiveList<RepositorySearchModel.RepositoryModel>();
            Items = repositories.CreateDerivedCollection(x =>
            {
                var description = showDescription ? x.Description : string.Empty;
                var viewModel = new RepositoryItemViewModel(x.Name, description, x.Owner?.Login, x.StargazersCount, x.ForksCount, new GitHubAvatar(x.Owner?.AvatarUrl));
                viewModel.GoToCommand.Subscribe(_ =>
                {
                    var id = RepositoryIdentifier.FromFullName(x.FullName);
                    NavigateTo(new RepositoryViewModel(id.Owner, id.Name));
                });
                return viewModel;
            });

            SearchCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.SearchText).Select(x => !string.IsNullOrEmpty(x)),
                async _ =>
                {
                    try
                    {
                        repositories.Clear();
                        var request = this.GetApplication().Client.Repositories.SearchRepositories(new[] { SearchText }, new string[] { });
                        var response = await this.GetApplication().Client.ExecuteAsync(request);
                        repositories.AddRange(response.Data.Items);
                    }
                    catch
                    {
                        DisplayAlert("Unable to search for repositories. Please try again.");
                    }
                });
        }
    }
}


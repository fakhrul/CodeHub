using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.Utilities;
using CodeHub.Core.Utils;
using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Repositories
{
    public abstract class RepositoriesViewModel 
        : BaseViewModel, ILoadableViewModel, IListViewModel<RepositoryItemViewModel>, IPaginatableViewModel
    {
        public IReactiveCommand<Unit> LoadCommand { get; }

        public IReadOnlyReactiveList<RepositoryItemViewModel> Items { get; }

        private readonly ObservableAsPropertyHelper<bool> _isEmpty;
        public bool IsEmpty => _isEmpty.Value;

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { this.RaiseAndSetIfChanged(ref _searchText, value); }
        }

        private bool _hasMoreToLoad;
        private bool HasMoreToLoad
        {
            get { return _hasMoreToLoad; }
            set { this.RaiseAndSetIfChanged(ref _hasMoreToLoad, value); }
        }

        public IReactiveCommand<Unit> LoadMoreCommand { get; }

        protected bool ShowRepositoryOwner { get; set; } = true;

        protected RepositoriesViewModel()
        {
            var applicationService = GetService<IApplicationService>();

            Title = "Repositories";

            var showDescription = applicationService.Account.ShowRepositoryDescriptionInList;
            var repositories = new ReactiveList<RepositoryModel>();

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
            }, x => x.Name.ContainsKeyword(SearchText), signalReset: this.WhenAnyValue(x => x.SearchText));

            var page = 1;

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                page = 1;
                repositories.Clear();
                HasMoreToLoad = await Load(repositories, page);
            });

            LoadMoreCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.HasMoreToLoad),
                async _ =>
                {
                    page++;
                    HasMoreToLoad = await Load(repositories, page);
                });

            LoadCommand.IsExecuting.CombineLatest(repositories.IsEmptyChanged, (x, y) => !x && y)
                       .ToProperty(this, x => x.IsEmpty, out _isEmpty);
        }

        protected abstract Task<bool> Load(IReactiveList<RepositoryModel> repositories, int page);
    }
}
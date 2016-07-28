using System;
using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive;
using CodeHub.Core.Services;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Source
{
    public class BranchesViewModel 
        : BaseViewModel, ILoadableViewModel, IListViewModel<RefItemViewModel>
    {
        public IReactiveCommand<Unit> LoadCommand { get; }

        public IReadOnlyReactiveList<RefItemViewModel> Items { get; }

        public bool IsEmpty => false;

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { this.RaiseAndSetIfChanged(ref _searchText, value); }
        }

        public static BranchesViewModel ForCommits(string username, string repository)
            => new BranchesViewModel(username, repository, x => new ChangesetViewModel(username, repository, x.Name));

        public static BranchesViewModel ForSource(string username, string repository)
        => new BranchesViewModel(username, repository, x => new SourceTreeViewModel(username, repository, x.Name, trueBranch: true));

        private BranchesViewModel(string username, string repository, Func<BranchModel, IBaseViewModel> navigateDelegate)
        {
            var application = GetService<IApplicationService>();

            Title = "Branches";

            var branches = new ReactiveList<BranchModel>();
            Items = branches.CreateDerivedCollection(
                x =>
                {
                    var vm = new RefItemViewModel(x.Name);
                    vm.GoToCommand.Subscribe(_ => NavigateTo(navigateDelegate(x)));
                    return vm;
                }, 
                x => x.Name.ContainsKeyword(SearchText), 
                signalReset: this.WhenAnyValue(x => x.SearchText));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                var request = application.Client.Users[username].Repositories[repository].GetBranches();
                branches.Clear();

                while (request != null)
                {
                    var items = await application.Client.ExecuteAsync(request);
                    branches.AddRange(items.Data);
                    request = items.More;
                }
            });
        }
    }
}


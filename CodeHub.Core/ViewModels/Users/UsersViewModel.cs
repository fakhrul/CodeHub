using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Users
{
    public abstract class UsersViewModel : BaseViewModel, ILoadableViewModel, IListViewModel<UserItemViewModel>, IPaginatableViewModel
    {
        private string _emptyMessage;
        public string EmptyMessage
        {
            get { return _emptyMessage; }
            protected set { this.RaiseAndSetIfChanged(ref _emptyMessage, value); }
        }

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

        public IReactiveCommand<Unit> LoadCommand { get; }

        public IReadOnlyReactiveList<UserItemViewModel> Items { get; }

        public IReactiveCommand<Unit> LoadMoreCommand { get; }

        protected UsersViewModel()
        {
            var users = new ReactiveList<BasicUserModel>();
            Items = users.CreateDerivedCollection(
                CreateViewModel,
                x => x.Login.ContainsKeyword(SearchText),
                signalReset: this.WhenAnyValue(x => x.SearchText));

            var page = 1;

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                page = 1;
                users.Clear();
                HasMoreToLoad = await Load(users, page);
            });

            LoadMoreCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.HasMoreToLoad),
                async _ =>
                {
                    page++;
                    HasMoreToLoad = await Load(users, page);
                });

            _isEmpty = LoadCommand
                .IsExecuting
                .Skip(1)
                .Select(x => !x && users.Count == 0)
                .ToProperty(this, x => x.IsEmpty);
        }

        private UserItemViewModel CreateViewModel(BasicUserModel model)
        {
            var vm = new UserItemViewModel(model.Login, new Utilities.GitHubAvatar(model.AvatarUrl));
            vm.GoToCommand.Subscribe(_ => NavigateTo(new UserViewModel(model.Login)));
            return vm;
        }

        protected abstract Task<bool> Load(ReactiveList<BasicUserModel> users, int page);
    }
}

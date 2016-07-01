//using System;
//using System.Reactive;
//using CodeHub.Core.Services;
//using GitHubSharp.Models;
//using ReactiveUI;
//using Splat;

//namespace CodeHub.Core.ViewModels.Source
//{
//    public class BranchesListViewModel
//        : ReactiveObject, ILoadableViewModel, IListViewModel<RefItemViewModel>, IPaginatableViewModel
//    {
//        private readonly IApplicationService _applicationService;
//        private readonly IDisposable _pullRequestEditSubscription;

//        private readonly IReactiveList<BranchModel> _items = new ReactiveList<BranchModel>();

//        public IReactiveCommand<Unit> LoadCommand { get; }

//        public IReadOnlyReactiveList<RefItemViewModel> Items { get; }

//        private readonly ObservableAsPropertyHelper<bool> _isEmpty;
//        public bool IsEmpty => _isEmpty.Value;

//        private string _searchText;
//        public string SearchText
//        {
//            get { return _searchText; }
//            set { this.RaiseAndSetIfChanged(ref _searchText, value); }
//        }

//        private IReactiveCommand<Unit> _loadMoreCommand;
//        public IReactiveCommand<Unit> LoadMoreCommand
//        {
//            get { return _loadMoreCommand; }
//            private set { this.RaiseAndSetIfChanged(ref _loadMoreCommand, value); }
//        }

//        public string Username { get; }

//        public string Repository { get; }

//        public BranchesListViewModel(string username, string repository)
//        {
//            Username = username;
//            Repository = repository;

//            var application = _applicationService = Locator.Current.GetService<IApplicationService>();

//            Items = _items.CreateDerivedCollection(
//                x =>
//                {
//                    var vm = new PullRequestItemViewModel(x);
//                    vm.GoToCommand.Subscribe(_ => NavigateTo(new PullRequestViewModel(Username, Repository, x.Number, x)));
//                    return vm;
//                },
//                x => x.Title.ContainsKeyword(SearchText),
//                signalReset: this.WhenAnyValue(x => x.SearchText));

//            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
//            {
//                _items.Clear();
//                var state = SelectedFilter == 0 ? "open" : "closed";
//                var request = application.Client.Users[Username].Repositories[Repository].PullRequests.GetAll(state: state);
//                var response = await application.Client.ExecuteAsync(request);
//                _items.AddRange(response.Data);
//                SetLoadMore(response.More);
//            });

//            LoadCommand.IsExecuting.CombineLatest(_items.IsEmptyChanged, (x, y) => !x && y)
//                       .ToProperty(this, x => x.IsEmpty, out _isEmpty);

//            this.WhenAnyValue(x => x.SelectedFilter)
//                .Skip(1)
//                .InvokeCommand(LoadCommand);

//            _pullRequestEditSubscription = Messenger.Subscribe<PullRequestEditMessage>(x =>
//            {
//                var url = x?.PullRequest?.Url;
//                var match = _items.FirstOrDefault(y => url == y.Url);
//                if (match != null)
//                {
//                    var index = _items.IndexOf(match);
//                    _items.RemoveAt(index);
//                    _items.Insert(index, x.PullRequest);
//                }
//            });
//        }


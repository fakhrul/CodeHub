using System;
using GitHubSharp.Models;
using CodeHub.Core.Messages;
using System.Linq;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive;
using GitHubSharp;
using CodeHub.Core.Services;
using System.Collections.Generic;
using Splat;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestListViewModel
        : ReactiveObject, ILoadableViewModel, IListViewModel<PullRequestItemViewModel>, IPaginatableViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly IDisposable _pullRequestEditSubscription;

        private readonly IReactiveList<PullRequestModel> _pullRequests = new ReactiveList<PullRequestModel>();

        public IReactiveCommand<Unit> LoadCommand { get; }

        public IReadOnlyReactiveList<PullRequestItemViewModel> Items { get; }

        private readonly ObservableAsPropertyHelper<bool> _isEmpty;
        public bool IsEmpty => _isEmpty.Value;

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { this.RaiseAndSetIfChanged(ref _searchText, value); }
        }

        private IReactiveCommand<Unit> _loadMoreCommand;
        public IReactiveCommand<Unit> LoadMoreCommand
        {
            get { return _loadMoreCommand; }
            private set { this.RaiseAndSetIfChanged(ref _loadMoreCommand, value); }
        }

        public PullRequestListViewModel(string username, string repository, Action<IBaseViewModel> navigate, bool open = true)
        {
            var application = _applicationService = Locator.Current.GetService<IApplicationService>();

            Items = _pullRequests.CreateDerivedCollection(
                x =>
                {
                    var vm = new PullRequestItemViewModel(x);
                    vm.GoToCommand.Subscribe(_ => navigate(new PullRequestViewModel(username, repository, x.Number, x)));
                    return vm;
                },
                x => x.Title.ContainsKeyword(SearchText),
                signalReset: this.WhenAnyValue(x => x.SearchText));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                _pullRequests.Clear();
                var state = open ? "open" : "closed";
                var request = application.Client.Users[username].Repositories[repository].PullRequests.GetAll(state: state);
                var response = await application.Client.ExecuteAsync(request);
                _pullRequests.AddRange(response.Data);
                SetLoadMore(response.More);
            });

            LoadCommand.IsExecuting.CombineLatest(_pullRequests.IsEmptyChanged, (x, y) => !x && y)
                       .ToProperty(this, x => x.IsEmpty, out _isEmpty);

            _pullRequestEditSubscription = Messenger.Subscribe<PullRequestEditMessage>(x =>
            {
                var url = x?.PullRequest?.Url;
                var match = _pullRequests.FirstOrDefault(y => url == y.Url);
                if (match != null)
                {
                    var index = _pullRequests.IndexOf(match);
                    _pullRequests.RemoveAt(index);
                    _pullRequests.Insert(index, x.PullRequest);
                }
            });
        }

        private void SetLoadMore(GitHubRequest<List<PullRequestModel>> nextRequest)
        {
            if (nextRequest == null)
            {
                LoadMoreCommand = null;
            }
            else
            {
                LoadMoreCommand = ReactiveCommand.CreateAsyncTask(async _ =>
                {
                    var response = await _applicationService.Client.ExecuteAsync(nextRequest);
                    _pullRequests.AddRange(response.Data);
                    SetLoadMore(response.More);
                });
            }
        }
    }
}

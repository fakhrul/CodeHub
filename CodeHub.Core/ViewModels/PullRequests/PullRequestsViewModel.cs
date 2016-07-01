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

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestsViewModel
        : BaseViewModel, ILoadableViewModel, IListViewModel<PullRequestItemViewModel>, IPaginatableViewModel
    {
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

        private int _selectedFilter;
        public int SelectedFilter
        {
            get { return _selectedFilter; }
            set { this.RaiseAndSetIfChanged(ref _selectedFilter, value); }
        }

        public PullRequestsViewModel(string username, string repository) 
        {
            var openViewModel = new PullRequestListViewModel(username, repository, NavigateTo, true);
            var closedViewModel = new PullRequestListViewModel(username, repository, NavigateTo, false);

            Title = "Pull Requests";

            var items = new ReactiveList<PullRequestItemViewModel>();

            this.WhenAnyValue(x => x.SearchText)
                .Subscribe(x =>
                {
                    openViewModel.SearchText = x;
                    closedViewModel.SearchText = x;
                });

            this.WhenAnyValue(x => x.SelectedFilter)
                .Subscribe(x =>
                {
                    items.Reset(
                });

            Items = items;

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                _pullRequests.Clear();
                var state = SelectedFilter == 0 ? "open" : "closed";
                var request = application.Client.Users[Username].Repositories[Repository].PullRequests.GetAll(state: state);
                var response = await application.Client.ExecuteAsync(request);
                _pullRequests.AddRange(response.Data);
                SetLoadMore(response.More);
            });

            LoadCommand.IsExecuting.CombineLatest(_pullRequests.IsEmptyChanged, (x, y) => !x && y)
                       .ToProperty(this, x => x.IsEmpty, out _isEmpty);

            this.WhenAnyValue(x => x.SelectedFilter)
                .Skip(1)
                .InvokeCommand(LoadCommand);
        }
    }
}

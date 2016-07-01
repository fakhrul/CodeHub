using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using GitHubSharp;
using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Gists
{
    public abstract class GistsViewModel
        : BaseViewModel, ILoadableViewModel, IListViewModel<GistItemViewModel>, IPaginatableViewModel
    {
        protected readonly IReactiveList<GistModel> Gists = new ReactiveList<GistModel>();

        public IReactiveCommand<Unit> LoadCommand { get; }

        public IReadOnlyReactiveList<GistItemViewModel> Items { get; }

        private readonly ObservableAsPropertyHelper<bool> _isEmpty;
        public bool IsEmpty => _isEmpty.Value;

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { this.RaiseAndSetIfChanged(ref _searchText, value); }
        }

        private GitHubRequest<List<GistModel>> _nextRequest;
        private GitHubRequest<List<GistModel>> NextRequest
        {
            get { return _nextRequest; }
            set { this.RaiseAndSetIfChanged(ref _nextRequest, value); }
        }

        public IReactiveCommand<Unit> LoadMoreCommand { get; }

        protected GistsViewModel()
        {
            var application = GetService<IApplicationService>();

            var transform1 = Gists.CreateDerivedCollection(x =>
            {
                var title = x.Files?.Select(y => y.Key).FirstOrDefault() ?? "Gist #" + x.Id;
                var viewModel = new GistItemViewModel(title, x.Owner?.AvatarUrl, x.Description, x.UpdatedAt);
                viewModel.GoToCommand.Subscribe(_ => NavigateTo(new GistViewModel(x)));
                return viewModel;
            });

            Items = transform1.CreateDerivedCollection(
                x => x, 
                x => x.Title.ContainsKeyword(SearchText) || x.Description.ContainsKeyword(SearchText), 
                signalReset: this.WhenAnyValue(x => x.SearchText));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                Gists.Clear();
                var request = CreateRequest();
                var response = await application.Client.ExecuteAsync(request);
                Gists.AddRange(response.Data);
                NextRequest = response.More;
            });

            LoadMoreCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.NextRequest).Select(x => x != null),
                async _ =>
                {
                    var response = await application.Client.ExecuteAsync(NextRequest);
                    Gists.AddRange(response.Data);
                    NextRequest = response.More;
                });

            LoadCommand.IsExecuting.CombineLatest(Gists.IsEmptyChanged, (x, y) => !x && y)
                       .ToProperty(this, x => x.IsEmpty, out _isEmpty);
        }

        protected abstract GitHubRequest<List<GistModel>> CreateRequest();
    }
}


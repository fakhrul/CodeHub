using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationsViewModel
        : BaseViewModel, ILoadableViewModel, IListViewModel<OrganizationItemViewModel>, IPaginatableViewModel
    {
        private readonly IApplicationService _applicationService;

        public string Username { get; }

        public IReactiveCommand<Unit> LoadCommand { get; }

        public IReadOnlyReactiveList<OrganizationItemViewModel> Items { get; }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { this.RaiseAndSetIfChanged(ref _searchText, value); }
        }

        private GitHubSharp.GitHubRequest<List<BasicUserModel>> _nextRequest;
        private GitHubSharp.GitHubRequest<List<BasicUserModel>> NextRequest
        {
            get { return _nextRequest; }
            set { this.RaiseAndSetIfChanged(ref _nextRequest, value); }
        }

        private readonly ObservableAsPropertyHelper<bool> _isEmpty;
        public bool IsEmpty => _isEmpty.Value;

        public IReactiveCommand<Unit> LoadMoreCommand { get; }

        public OrganizationsViewModel(string username)
        {
            _applicationService = GetService<IApplicationService>();

            Username = username;
            Title = "Organizations";

            var teams = new ReactiveList<BasicUserModel>();
            Items = teams.CreateDerivedCollection(x =>
            {
                var viewModel = new OrganizationItemViewModel(x.Login, new Utilities.GitHubAvatar(x.AvatarUrl));
                viewModel.GoToCommand.Subscribe(_ => NavigateTo(new OrganizationViewModel(x.Login)));
                return viewModel;
            }, x => x.Name.ContainsKeyword(SearchText), signalReset: this.WhenAnyValue(x => x.SearchText));

            LoadCommand = ReactiveCommand.CreateAsyncTask(_ =>
            {
                teams.Clear();
                return Load(teams);
            });

            LoadMoreCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.NextRequest).Select(x => x != null),
                _ => Load(teams));

            LoadCommand.IsExecuting.CombineLatest(teams.IsEmptyChanged, (x, y) => !x && y)
                       .ToProperty(this, x => x.IsEmpty, out _isEmpty);
        }

        private async Task Load(IReactiveList<BasicUserModel> teams)
        {
            var request = _applicationService.Client.Users[Username].GetOrganizations();
            var response = await _applicationService.Client.ExecuteAsync(request);
            teams.AddRange(response.Data);
            NextRequest = response.More;
        }
    }
}


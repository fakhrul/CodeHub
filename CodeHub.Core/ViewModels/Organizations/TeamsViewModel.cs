using GitHubSharp.Models;
using System.Threading.Tasks;
using ReactiveUI;
using System;
using System.Reactive;
using CodeHub.Core.Services;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class TeamsViewModel
        : BaseViewModel, ILoadableViewModel, IListViewModel<TeamItemViewModel>, IPaginatableViewModel
    {
        private readonly IApplicationService _applicationService;

        public string TeamName { get; }

        public IReactiveCommand<Unit> LoadCommand { get; }

        public IReadOnlyReactiveList<TeamItemViewModel> Items { get; }

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

        private readonly ObservableAsPropertyHelper<bool> _isEmpty;
        public bool IsEmpty => _isEmpty.Value;

        public IReactiveCommand<Unit> LoadMoreCommand { get; }

        public TeamsViewModel(string teamName)
        {
            _applicationService = GetService<IApplicationService>();

            TeamName = teamName;
            Title = "Teams";

            var teams = new ReactiveList<TeamShortModel>();
            Items = teams.CreateDerivedCollection(x =>
            {
                var viewModel = new TeamItemViewModel(x.Name);
                viewModel.GoToCommand.Subscribe(_ => NavigateTo(new TeamMembersViewModel(x.Id)));
                return viewModel;
            }, x => x.Name.ContainsKeyword(SearchText), signalReset: this.WhenAnyValue(x => x.SearchText));

            var page = 1;

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                page = 1;
                teams.Clear();
                HasMoreToLoad = await Load(teams, page);
            });

            LoadMoreCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.HasMoreToLoad),
                async _ =>
                {
                    page++;
                    HasMoreToLoad = await Load(teams, page);
                });

            LoadCommand.IsExecuting.CombineLatest(teams.IsEmptyChanged, (x, y) => !x && y)
                       .ToProperty(this, x => x.IsEmpty, out _isEmpty);
        }

        private async Task<bool> Load(IReactiveList<TeamShortModel> teams, int page)
        {
            var request = _applicationService.Client.Organizations[TeamName].GetTeams(page);
            var response = await _applicationService.Client.ExecuteAsync(request);
            teams.AddRange(response.Data);
            return response.More != null;
        }
    }
}
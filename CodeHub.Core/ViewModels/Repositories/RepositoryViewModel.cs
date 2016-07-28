using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.Users;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.ViewModels.PullRequests;
using CodeHub.Core.ViewModels.Source;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoryViewModel : LoadableViewModel
    {
        public string Username { get; }

        public string RepositoryName { get; }

        public string ImageUrl { get; set; }

        private bool? _starred;
        public bool? IsStarred
        {
            get { return _starred; }
            private set { this.RaiseAndSetIfChanged(ref _starred, value); }
        }

        private bool? _watched;
        public bool? IsWatched
        {
            get { return _watched; }
            private set { this.RaiseAndSetIfChanged(ref _watched, value); }
        }

        private RepositoryModel _repository;
        public RepositoryModel Repository
        {
            get { return _repository; }
            private set { this.RaiseAndSetIfChanged(ref _repository, value); }
        }

        private ContentModel _readme;
        public ContentModel Readme
        {
            get { return _readme; }
            private set { this.RaiseAndSetIfChanged(ref _readme, value); }
        }

        private List<BranchModel> _branches;
        public List<BranchModel> Branches
        {
            get { return _branches; }
            private set { this.RaiseAndSetIfChanged(ref _branches, value); }
        }

        public RepositoryViewModel(string username, string repository)
        {
            Username = username;
            Title = RepositoryName = repository;

            GoToOwnerCommand.Subscribe(_ => NavigateTo(new UserViewModel(Username)));

            GoToForkParentCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Repository.Parent).Select(x => x != null));
            GoToForkParentCommand.Subscribe(_ => NavigateTo(new RepositoryViewModel(Repository.Parent.Owner.Login, Repository.Parent.Name)));

            GoToStargazersCommand.Subscribe(_ => NavigateTo(new RepositoryStargazersViewModel(Username, RepositoryName)));
            GoToWatchersCommand.Subscribe(_ => NavigateTo(new RepositoryWatchersViewModel(Username, RepositoryName)));
            GoToForkedCommand.Subscribe(_ => NavigateTo(new RepositoriesForkedViewModel(Username, RepositoryName)));
            GoToEventsCommand.Subscribe(_ => NavigateTo(new RepositoryEventsViewModel(Username, RepositoryName)));
            GoToIssuesCommand.Subscribe(_ => NavigateTo(new IssuesViewModel(Username, RepositoryName)));
            GoToReadmeCommand.Subscribe(_ => NavigateTo(new ReadmeViewModel(Username, RepositoryName)));
            GoToCommitsCommand.Subscribe(_ => ShowCommits());
            GoToPullRequestsCommand.Subscribe(_ => NavigateTo(new PullRequestsViewModel(Username, RepositoryName)));
            GoToSourceCommand.Subscribe(_ => NavigateTo(new BranchesAndTagsViewModel(Username, RepositoryName)));

            GoToHtmlUrlCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Repository.HtmlUrl).Select(x => !string.IsNullOrEmpty(x)));
            GoToHtmlUrlCommand.Subscribe(_ => NavigateTo(new WebBrowserViewModel(Repository.HtmlUrl)));

            PinCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Repository).Select(x => x != null));
            PinCommand.Subscribe(_ => PinRepository());

            ToggleWatchCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.IsWatched).Select(x => x != null),
                t => ToggleWatch());

            ToggleStarCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.IsStarred).Select(x => x != null),
                t => ToggleStar());
        }

        public IReactiveCommand<object> GoToOwnerCommand { get; } = ReactiveCommand.Create();

        public IReactiveCommand<object> GoToForkParentCommand { get; }

        public IReactiveCommand<object> GoToStargazersCommand { get; } = ReactiveCommand.Create();

        public IReactiveCommand<object> GoToWatchersCommand { get; } = ReactiveCommand.Create();

        public IReactiveCommand<object> GoToForkedCommand { get; } = ReactiveCommand.Create();

        public IReactiveCommand<object> GoToEventsCommand { get; } = ReactiveCommand.Create();

        public IReactiveCommand<object> GoToIssuesCommand { get; } = ReactiveCommand.Create();

        public IReactiveCommand<object> GoToReadmeCommand { get; } = ReactiveCommand.Create();

        public IReactiveCommand<object> GoToCommitsCommand { get; } = ReactiveCommand.Create();

        public IReactiveCommand<object> GoToPullRequestsCommand { get; } = ReactiveCommand.Create();

        public IReactiveCommand<object> GoToSourceCommand { get; } = ReactiveCommand.Create();

        public IReactiveCommand<object> GoToHtmlUrlCommand { get; }

        private void ShowCommits()
        {
            if (Branches != null && Branches.Count == 1)
                NavigateTo(new ChangesetsViewModel(Username, RepositoryName, null));
            else
                NavigateTo(BranchesViewModel.ForCommits(Username, RepositoryName));
        }

        public IReactiveCommand<object> PinCommand { get; }

        private void PinRepository()
        {
            var repoOwner = Repository.Owner.Login;
            var repoName = Repository.Name;

            //Is it pinned already or not?
            var pinnedRepo = this.GetApplication().Account.PinnnedRepositories.GetPinnedRepository(repoOwner, repoName);
            if (pinnedRepo == null)
                this.GetApplication().Account.PinnnedRepositories.AddPinnedRepository(repoOwner, repoName, repoName, ImageUrl);
            else
                this.GetApplication().Account.PinnnedRepositories.RemovePinnedRepository(pinnedRepo.Id);
        }


        protected override Task Load()
        {
            var t1 = this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].Get(), response => Repository = response.Data);

            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].GetReadme(), 
                response => Readme = response.Data).ToBackground();

            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].GetBranches(), 
                response => Branches = response.Data).ToBackground();

            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].IsWatching(), 
                response => IsWatched = response.Data).ToBackground();
         
            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].IsStarred(), 
                response => IsStarred = response.Data).ToBackground();

            return t1;
        }

        public IReactiveCommand<Unit> ToggleWatchCommand { get; }

        private async Task ToggleWatch()
        {
            if (IsWatched == null)
                return;

            try
            {
                if (IsWatched.Value)
                    await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].StopWatching());
                else
                    await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].Watch());
                IsWatched = !IsWatched;
            }
            catch
            {
                DisplayAlert("Unable to toggle repository as " + (IsWatched.Value ? "unwatched" : "watched") + "! Please try again.");
            }
        }

        public IReactiveCommand<Unit> ToggleStarCommand { get; }

        public bool IsPinned
        {
            get { return this.GetApplication().Account.PinnnedRepositories.GetPinnedRepository(Username, RepositoryName) != null; }
        }

        private async Task ToggleStar()
        {
            if (IsStarred == null)
                return;

            try
            {
                if (IsStarred.Value)
                    await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].Unstar());
                else
                    await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[RepositoryName].Star());
                IsStarred = !IsStarred;
            }
            catch
            {
                DisplayAlert("Unable to " + (IsStarred.Value ? "unstar" : "star") + " this repository! Please try again.");
            }
        }
    }
}


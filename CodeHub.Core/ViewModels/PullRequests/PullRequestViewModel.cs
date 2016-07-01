using System;
using System.Threading.Tasks;
using System.Windows.Input;
using GitHubSharp.Models;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.Messages;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Users;
using ReactiveUI;
using System.Reactive;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestViewModel : LoadableViewModel
    {
        private readonly IDisposable _issueEditSubscription;
        private readonly IDisposable _pullRequestEditSubscription;
        private readonly IFeaturesService _featuresService;

        public long Id
        { 
            get; 
            private set; 
        }

        public string Username
        { 
            get; 
            private set; 
        }

        public string Repository
        { 
            get; 
            private set; 
        }

        public string MarkdownDescription
        {
            get { return PullRequest == null ? string.Empty : (GetService<IMarkdownService>().Convert(PullRequest.Body)); }
        }

        private bool _canPush;
        public bool CanPush
        {
            get { return _canPush; }
            private set { this.RaiseAndSetIfChanged(ref _canPush, value); }
        }

        private bool _isCollaborator;
        public bool IsCollaborator
        {
            get { return _isCollaborator; }
            private set { this.RaiseAndSetIfChanged(ref _isCollaborator, value); }
        }

        private bool _merged;
        public bool Merged
        {
            get { return _merged; }
            set { this.RaiseAndSetIfChanged(ref _merged, value); }
        }

        private IssueModel _issueModel;
        public IssueModel Issue
        {
            get { return _issueModel; }
            private set { this.RaiseAndSetIfChanged(ref _issueModel, value); }
        }

        private PullRequestModel _model;
        public PullRequestModel PullRequest
        { 
            get { return _model; }
            private set { this.RaiseAndSetIfChanged(ref _model, value); }
        }

        private bool _isModifying;
        public bool IsModifying
        {
            get { return _isModifying; }
            set { this.RaiseAndSetIfChanged(ref _isModifying, value); }
        }

        private bool? _isClosed;
        public bool? IsClosed
        {
            get { return _isClosed; }
            private set { this.RaiseAndSetIfChanged(ref _isClosed, value); }
        }

        public IReactiveCommand<object> GoToAssigneeCommand { get; }

        public IReactiveCommand<object> GoToMilestoneCommand { get; }

        public IReactiveCommand<object> GoToLabelsCommand { get; }

        public IReactiveCommand<object> GoToEditCommand { get; }

        public IReactiveCommand<Unit> ToggleStateCommand { get; }

        public ReactiveCommand<object> GoToOwner { get; }

        public IReactiveCommand<object> GoToCommitsCommand { get; } = ReactiveCommand.Create();

        public IReactiveCommand<object> GoToFilesCommand { get; } = ReactiveCommand.Create();

        public CollectionViewModel<IssueCommentModel> Comments { get; } = new CollectionViewModel<IssueCommentModel>();

        public CollectionViewModel<IssueEventModel> Events { get; } = new CollectionViewModel<IssueEventModel>();

        public string ConvertToMarkdown(string str)
        {
            return (GetService<IMarkdownService>().Convert(str));
        }

        public PullRequestViewModel(string username, string repository, long id, PullRequestModel model = null)
        {
            Username = username;
            Repository = repository;
            Id = id;
            PullRequest = model;

            _featuresService = GetService<IFeaturesService>();

            this.WhenAnyValue(x => x.PullRequest)
                .IsNotNull()
                .Select(x => string.Equals(x.State, "closed"))
                .Subscribe(x => IsClosed = x);

            GoToCommitsCommand.Subscribe(_ => NavigateTo(new PullRequestCommitsViewModel(Username, Repository, Id)));
            GoToFilesCommand.Subscribe(_ => NavigateTo(new PullRequestFilesViewModel(Username, Repository, Id)));

            GoToAssigneeCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.IsCollaborator));
            GoToMilestoneCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.IsCollaborator));
            GoToLabelsCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.IsCollaborator));

            GoToAssigneeCommand.Subscribe(
                _ => NavigateTo(new IssueAssignedViewModel(Username, Repository, Id, true, Issue?.User)));

            GoToMilestoneCommand.Subscribe(
                _ => NavigateTo(new IssueMilestonesViewModel(Username, Repository, Id, true, Issue?.Milestone)));

            GoToLabelsCommand.Subscribe(
                _ => NavigateTo(new IssueLabelsViewModel(Username, Repository, Id, true, Issue?.Labels)));

            GoToEditCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.IsCollaborator, x => x.Issue).Select(x => x.Item1 && x.Item2 != null));
            GoToEditCommand.Subscribe(
                _ => NavigateTo(new IssueEditViewModel(Username, Repository, Id, Issue)));

            ToggleStateCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Issue).Select(x => x != null),
                t => ToggleState(Issue.State == "open"));

            MergeCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.PullRequest, x => x.CanPush).Select(x => CanMerge(x.Item1) && x.Item2),
                t => Merge());

            //{
            //    get { return new MvxCommand(() => Merge(), CanMerge); }
            //}
            
            GoToOwner = ReactiveCommand.Create(this.WhenAnyValue(x => x.Issue).Select(x => x != null));
            GoToOwner
                .Select(_ => Issue?.User?.Login)
                .Where(x => x != null)
                .Select(x => new UserViewModel(x))
                .Subscribe(NavigateTo);

            _issueEditSubscription = Messenger.Subscribe<IssueEditMessage>(x =>
            {
                if (x.Issue == null || x.Issue.Number != Id)
                    return;
                Issue = x.Issue;
            });

            _pullRequestEditSubscription = Messenger.Subscribe<PullRequestEditMessage>(x =>
            {
                if (x.PullRequest == null || x.PullRequest.Number != Id)
                    return;
                PullRequest = x.PullRequest;
            });
        }

        public async Task<bool> AddComment(string text)
        {
            try
            {
                var comment = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].CreateComment(text));
                Comments.Items.Add(comment.Data);
                return true;
            }
            catch (Exception e)
            {
                DisplayAlert(e.Message);
                return false;
            }
        }

        private async Task ToggleState(bool closed)
        {
            try
            {
                IsModifying = true;
                var data = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[Repository].PullRequests[Id].UpdateState(closed ? "closed" : "open")); 
                Messenger.Publish(new PullRequestEditMessage { PullRequest = data.Data });
            }
            catch (Exception e)
            {
                DisplayAlert("Unable to " + (closed ? "close" : "open") + " the item. " + e.Message);
            }
            finally
            {
                IsModifying = false;
            }
        }

        private bool _shouldShowPro; 
        public bool ShouldShowPro
        {
            get { return _shouldShowPro; }
            protected set { this.RaiseAndSetIfChanged(ref _shouldShowPro, value); }
        }

        protected override Task Load()
        {
            ShouldShowPro = false;

            var pullRequest = this.GetApplication().Client.Users[Username].Repositories[Repository].PullRequests[Id].Get();
            var t1 = this.RequestModel(pullRequest, response => PullRequest = response.Data);
            Events.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].GetEvents()).ToBackground();
            Comments.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].GetComments()).ToBackground();
            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].Get(), response => Issue = response.Data).ToBackground();
            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].Get(), response => {
                CanPush = response.Data.Permissions.Push;
                ShouldShowPro = response.Data.Private && !_featuresService.IsProEnabled;
            }).ToBackground();
            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].IsCollaborator(this.GetApplication().Account.Username), 
                response => IsCollaborator = response.Data).ToBackground();
            return t1;
        }

        public async Task Merge()
        {
            try
            {
                var response = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[Repository].PullRequests[Id].Merge(string.Empty));
                if (!response.Data.Merged)
                    throw new Exception(response.Data.Message);

            }
            catch (Exception e)
            {
                this.AlertService.Alert("Unable to Merge!", e.Message).ToBackground();
            }

            await Load();
        }

        public IReactiveCommand<Unit> MergeCommand { get; }

        private static bool CanMerge(PullRequestModel model)
        {
            var isClosed = string.Equals(model.State, "closed", StringComparison.OrdinalIgnoreCase);
            var isMerged = model.Merged.GetValueOrDefault();
            return !isClosed && !isMerged;
        }
    }
}

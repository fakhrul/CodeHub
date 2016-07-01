using System.Threading.Tasks;
using GitHubSharp.Models;
using CodeHub.Core.Messages;
using CodeHub.Core.Services;
using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Users;
using ReactiveUI;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueViewModel : LoadableViewModel
    {
        private IDisposable _editToken;
        private readonly IFeaturesService _featuresService;
        private readonly IApplicationService _applicationService;

        public long Id { get; }

        public string Username { get; }

        public string Repository { get; }

        public string MarkdownDescription
        {
            get
            {
                if (Issue == null)
                    return string.Empty;
                return (GetService<IMarkdownService>().Convert(Issue.Body));
            }
        }

        private bool? _isClosed;
        public bool? IsClosed
        {
            get { return _isClosed; }
            private set { this.RaiseAndSetIfChanged(ref _isClosed, value); }
        }

        private bool _shouldShowPro; 
        public bool ShouldShowPro
        {
            get { return _shouldShowPro; }
            protected set { this.RaiseAndSetIfChanged(ref _shouldShowPro, value); }
        }

        private bool _isCollaborator;
        public bool IsCollaborator
        {
            get { return _isCollaborator; }
            private set { this.RaiseAndSetIfChanged(ref _isCollaborator, value); }
        }

        private IssueModel _issueModel;
        public IssueModel Issue
        {
            get { return _issueModel; }
            private set { this.RaiseAndSetIfChanged(ref _issueModel, value); }
        }

        private bool _isModifying;
        public bool IsModifying
        {
            get { return _isModifying; }
            set { this.RaiseAndSetIfChanged(ref _isModifying, value); }
        }

        public ReactiveCommand<object> GoToOwner { get; }

        public IReactiveCommand<object> GoToAssigneeCommand { get; }

        public IReactiveCommand<object> GoToMilestoneCommand { get; }

        public IReactiveCommand<object> GoToLabelsCommand { get; }

        public IReactiveCommand<object> GoToEditCommand { get; }

        public IReactiveCommand<Unit> ToggleStateCommand { get; }

        public CollectionViewModel<IssueCommentModel> Comments { get; } = new CollectionViewModel<IssueCommentModel>();

        public CollectionViewModel<IssueEventModel> Events { get; } = new CollectionViewModel<IssueEventModel>();

        protected override Task Load()
        {
            if (_featuresService.IsProEnabled)
                ShouldShowPro = false;
            else
            {
                var request = _applicationService.Client.Users[Username].Repositories[Repository].Get();
                _applicationService.Client.ExecuteAsync(request)
                    .ToBackground(x => ShouldShowPro = x.Data.Private && !_featuresService.IsProEnabled);
            }

            var t1 = this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].Get(), response => Issue = response.Data);
            Comments.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].GetComments()).ToBackground();
            Events.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].GetEvents()).ToBackground();
            this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].IsCollaborator(this.GetApplication().Account.Username), response => IsCollaborator = response.Data).ToBackground();
            return t1;
        }

        public string ConvertToMarkdown(string str)
        {
            return (GetService<IMarkdownService>().Convert(str));
        }

        public IssueViewModel(string username, string repository, long id)
        {
            _applicationService = GetService<IApplicationService>();
            _featuresService = GetService<IFeaturesService>();

            Username = username;
            Repository = repository;
            Id = id;

            Title = "Issue #" + Id;

            _editToken = Messenger.Subscribe<IssueEditMessage>(x =>
            {
                if (x.Issue == null || x.Issue.Number != Issue.Number)
                    return;
                Issue = x.Issue;
            });

            this.WhenAnyValue(x => x.Issue.State)
                .Where(x => x != null)
                .Select(x => string.Equals(x, "closed"))
                .Subscribe(x => IsClosed = x);

            GoToOwner = ReactiveCommand.Create(this.WhenAnyValue(x => x.Issue.User.Login).Select(x => x != null));
            GoToOwner
                .Select(_ => new UserViewModel(Issue?.User?.Login))
                .Subscribe(NavigateTo);

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
                var data = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Issue.Number].UpdateState(closed ? "closed" : "open")); 
                Messenger.Publish(new IssueEditMessage { Issue = data.Data });
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
    }
}


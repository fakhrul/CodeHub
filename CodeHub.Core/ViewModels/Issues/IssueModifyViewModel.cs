using System;
using GitHubSharp.Models;
using System.Threading.Tasks;
using CodeHub.Core.Messages;
using ReactiveUI;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Issues
{
    public abstract class IssueModifyViewModel : BaseViewModel
    {
        private string _title;
        private string _content;
        private BasicUserModel _assignedTo;
        private readonly CollectionViewModel<LabelModel> _labels = new CollectionViewModel<LabelModel>();
        private MilestoneModel _milestone;
        private IDisposable _labelsToken, _milestoneToken, _assignedToken;
        private bool _isSaving;

        public string IssueTitle
        {
            get { return _title; }
            set { this.RaiseAndSetIfChanged(ref _title, value); }
        }

        public string Content
        {
            get { return _content; }
            set { this.RaiseAndSetIfChanged(ref _content, value); }
        }

        public MilestoneModel Milestone
        {
            get { return _milestone; }
            set { this.RaiseAndSetIfChanged(ref _milestone, value); }
        }

        public CollectionViewModel<LabelModel> Labels
        {
            get { return _labels; }
        }

        public BasicUserModel AssignedTo
        {
            get { return _assignedTo; }
            set { this.RaiseAndSetIfChanged(ref _assignedTo, value); }
        }

        public bool IsSaving
        {
            get { return _isSaving; }
            protected set { this.RaiseAndSetIfChanged(ref _isSaving, value); }
        }

        public string Username { get; private set; }

        public string Repository { get; private set; }

        public IReactiveCommand<object> GoToLabelsCommand { get; } = ReactiveCommand.Create();

        public IReactiveCommand<object> GoToMilestonesCommand { get; } = ReactiveCommand.Create();

        public IReactiveCommand<object> GoToAssigneeCommand { get; } = ReactiveCommand.Create();

        public IReactiveCommand<Unit> SaveCommand { get; }

        protected IssueModifyViewModel(string username, string repository)
        {
            Username = username;
            Repository = repository;

            SaveCommand = ReactiveCommand.CreateAsyncTask(_ => Save());

            GoToAssigneeCommand.Subscribe(
                _ => NavigateTo(new IssueAssignedViewModel(Username, Repository, 0, false)));

            GoToMilestonesCommand.Subscribe(
                _ => NavigateTo(new IssueMilestonesViewModel(Username, Repository, 0, false)));

            GoToLabelsCommand.Subscribe(
                _ => NavigateTo(new IssueLabelsViewModel(Username, Repository, 0, false)));

            _labelsToken = Messenger.Subscribe<SelectIssueLabelsMessage>(x => Labels.Items.Reset(x.Labels));
            _milestoneToken = Messenger.Subscribe<SelectedMilestoneMessage>(x => Milestone = x.Milestone);
            _assignedToken = Messenger.Subscribe<SelectedAssignedToMessage>(x => AssignedTo = x.User);
        }

        protected abstract Task Save();
    }
}


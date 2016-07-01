using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CodeHub.Core.Filters;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.ViewModels.PullRequests;
using GitHubSharp.Models;
using CodeHub.Core.Messages;
using CodeHub.Core.ViewModels.Source;
using CodeHub.Core.ViewModels.Changesets;
using CodeHub.Core.Utils;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Notifications
{
    public class NotificationsViewModel : LoadableViewModel
    {
        private readonly FilterableCollectionViewModel<NotificationModel, NotificationsFilterModel> _notifications;

        public FilterableCollectionViewModel<NotificationModel, NotificationsFilterModel> Notifications
        {
            get { return _notifications; }
        }

        private int _shownIndex;
        public int ShownIndex
        {
            get { return _shownIndex; }
            set { this.RaiseAndSetIfChanged(ref _shownIndex, value); }
        }

        private bool _isMarking;
        public bool IsMarking
        {
            get { return _isMarking; }
            set { this.RaiseAndSetIfChanged(ref _isMarking, value); }
        }

        public IReactiveCommand<Unit> ReadRepositoriesCommand { get; }

        public IReactiveCommand<Unit> ReadAllCommand { get; }

        public IReactiveCommand<object> GoToNotificationCommand { get; } = ReactiveCommand.Create();
        
        private void GoToNotification(NotificationModel x)
        {
            var subject = x.Subject.Type.ToLower();
            if (subject.Equals("issue"))
            {
                Read(x).ToBackground();
                var node = x.Subject.Url.Substring(x.Subject.Url.LastIndexOf('/') + 1);
                NavigateTo(new IssueViewModel(x.Repository.Owner.Login, x.Repository.Name, long.Parse(node)));
            }
            else if (subject.Equals("pullrequest"))
            {
                Read(x).ToBackground();
                var node = x.Subject.Url.Substring(x.Subject.Url.LastIndexOf('/') + 1);
                NavigateTo(new PullRequestViewModel(x.Repository.Owner.Login, x.Repository.Name, long.Parse(node)));
            }
            else if (subject.Equals("commit"))
            {
                Read(x).ToBackground();
                var node = x.Subject.Url.Substring(x.Subject.Url.LastIndexOf('/') + 1);
                NavigateTo(new ChangesetViewModel(x.Repository.Owner.Login, x.Repository.Name, node));
            }
            else if (subject.Equals("release"))
            {
                Read(x).ToBackground();
                NavigateTo(new BranchesAndTagsViewModel(x.Repository.Owner.Login, x.Repository.Name, false));
            }
        }

        public NotificationsViewModel()
        {
            _notifications = new FilterableCollectionViewModel<NotificationModel, NotificationsFilterModel>("Notifications");
            _notifications.GroupingFunction = (n) => n.GroupBy(x => x.Repository.FullName);
            _notifications.WhenAnyValue(x => x.Filter).Skip(1).InvokeCommand(LoadCommand);

            GoToNotificationCommand
                .OfType<NotificationModel>()
                .Subscribe(GoToNotification);
            //    get { return _readAllCommand ?? (_readAllCommand = new MvxCommand(() => MarkAllAsRead(), () => ShownIndex != 2 && !IsLoading && !IsMarking && Notifications.Any())); }

            var canReadAll = Observable.CombineLatest(
                LoadCommand.IsExecuting,
                this.WhenAnyValue(x => x.ShownIndex),
                this.WhenAnyValue(x => x.IsMarking),
                (x, y, z) => !x && y != 2 && !z); 

            ReadAllCommand = ReactiveCommand.CreateAsyncTask(canReadAll, _ => MarkAllAsRead());

            ReadRepositoriesCommand = ReactiveCommand.CreateAsyncTask(async t =>
            {
                if (t is string)
                    await MarkRepoAsRead(t as string);
            });

            this.WhenAnyValue(x => x.ShownIndex)
                .Skip(1)
                .Subscribe(x => {
                    if (x == 0) _notifications.Filter = NotificationsFilterModel.CreateUnreadFilter();
                    else if (x == 1) _notifications.Filter = NotificationsFilterModel.CreateParticipatingFilter();
                    else _notifications.Filter = NotificationsFilterModel.CreateAllFilter();
                });

            if (_notifications.Filter.Equals(NotificationsFilterModel.CreateUnreadFilter()))
                _shownIndex = 0;
            else if (_notifications.Filter.Equals(NotificationsFilterModel.CreateParticipatingFilter()))
                _shownIndex = 1;
            else
                _shownIndex = 2;

        }

        protected override Task Load()
        {
            return this.RequestModel(this.GetApplication().Client.Notifications.GetAll(all: Notifications.Filter.All, participating: Notifications.Filter.Participating), response => {
                Notifications.Items.Reset(response.Data);
                UpdateAccountNotificationsCount();
            });
        }

        private async Task Read(NotificationModel model)
        {
            // If its already read, ignore it
            if (!model.Unread)
                return;

            try
            {
                var response = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Notifications[model.Id].MarkAsRead());
                if (response.Data) 
                {
                    //We just read it
                    model.Unread = false;
     
                    //Update the notifications count on the account
                    Notifications.Items.Remove(model);
                    UpdateAccountNotificationsCount();
                }
            }
            catch
            {
                DisplayAlert("Unable to mark notification as read. Please try again.");
            }
        }

        private async Task MarkRepoAsRead(string repo)
        {
            try
            {
                IsMarking = true;
                var repoId = RepositoryIdentifier.FromFullName(repo);
                if (repoId == null) return;
                await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Notifications.MarkRepoAsRead(repoId.Owner, repoId.Name));
                Notifications.Items.RemoveRange(Notifications.Items.Where(x => string.Equals(x.Repository.FullName, repo, StringComparison.OrdinalIgnoreCase)).ToList());
                UpdateAccountNotificationsCount();
            }
            catch
            {
                DisplayAlert("Unable to mark repositories' notifications as read. Please try again.");
            }
            finally
            {
                IsMarking = false;
            }
        }

        private async Task MarkAllAsRead()
        {
            // Make sure theres some sort of notification
            if (!Notifications.Any())
                return;

            try
            {
                IsMarking = true;
                await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Notifications.MarkAsRead());
                Notifications.Items.Clear();
                UpdateAccountNotificationsCount();
            }
            catch
            {
                DisplayAlert("Unable to mark all notifications as read. Please try again.");
            }
            finally
            {
                IsMarking = false;
            }
        }

        private void UpdateAccountNotificationsCount()
        {
            // Only update if we're looking at 
            if (!Notifications.Filter.All && !Notifications.Filter.Participating)
                Messenger.Publish(new NotificationCountMessage { Count = Notifications.Items.Sum(x => x.Unread ? 1 : 0) });
        }
    }
}


using System;
using GitHubSharp.Models;
using System.Threading.Tasks;
using CodeHub.Core.Messages;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueAssignedViewModel : LoadableViewModel
    {
        private BasicUserModel _selectedUser;
        public BasicUserModel SelectedUser
        {
            get { return _selectedUser; }
            set { this.RaiseAndSetIfChanged(ref _selectedUser, value); }
        }

        private bool _isSaving;
        public bool IsSaving
        {
            get { return _isSaving; }
            private set { this.RaiseAndSetIfChanged(ref _isSaving, value); }
        }

        public CollectionViewModel<BasicUserModel> Users { get; } = new CollectionViewModel<BasicUserModel>();

        public string Username  { get; private set; }

        public string Repository { get; private set; }

        public long Id { get; private set; }

        public bool SaveOnSelect { get; private set; }

        public IssueAssignedViewModel(string username, string repository, long id, bool saveOnSelect, BasicUserModel selectedUser = null) 
        {
            Username = username;
            Repository = repository;
            Id = id;
            SaveOnSelect = saveOnSelect;
            SelectedUser = selectedUser;

            Title = "Assignees";

            this.WhenAnyValue(x => x.SelectedUser).Skip(1).Subscribe(x => SelectUser(x).ToBackground());
        }

        private async Task SelectUser(BasicUserModel x)
        {
            if (SaveOnSelect)
            {
                try
                {
                    IsSaving = true;
                    var assignee = x != null ? x.Login : null;
                    var updateReq = this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].UpdateAssignee(assignee);
                    var newIssue = await this.GetApplication().Client.ExecuteAsync(updateReq);
                    Messenger.Publish(new IssueEditMessage { Issue = newIssue.Data });
        
                }
                catch
                {
                    DisplayAlert("Unable to assign issue to selected user! Please try again.");
                }
                finally
                {
                    IsSaving = false;
                }
            }
            else
            {
                Messenger.Publish(new SelectedAssignedToMessage { User = x });
            }

            Dismiss();
        }

        protected override Task Load()
        {
            return Users.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].GetAssignees());
        }
    }
}


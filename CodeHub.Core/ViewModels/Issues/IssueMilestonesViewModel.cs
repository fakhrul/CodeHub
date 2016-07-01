using System.Threading.Tasks;
using GitHubSharp.Models;
using CodeHub.Core.Messages;
using System;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueMilestonesViewModel : LoadableViewModel
    {
        private MilestoneModel _selectedMilestone;
        public MilestoneModel SelectedMilestone
        {
            get { return _selectedMilestone; }
            set { this.RaiseAndSetIfChanged(ref _selectedMilestone, value); }
        }

        private bool _isSaving;
        public bool IsSaving
        {
            get { return _isSaving; }
            private set { this.RaiseAndSetIfChanged(ref _isSaving, value); }
        }

        public CollectionViewModel<MilestoneModel> Milestones { get; } = new CollectionViewModel<MilestoneModel>();

        public string Username  { get; private set; }

        public string Repository { get; private set; }

        public long Id { get; private set; }

        public bool SaveOnSelect { get; private set; }

        public IssueMilestonesViewModel(string username, string repository, long id, bool saveOnSelect, MilestoneModel selectedMilestone = null)
        {
            Username = username;
            Repository = repository;
            Id = id;
            SaveOnSelect = saveOnSelect;
            SelectedMilestone = selectedMilestone;

            Title = "Milestones";

            this.WhenAnyValue(x => x.SelectedMilestone)
                .Skip(1)
                .Subscribe(x => SelectMilestone(x));
        }

        private async Task SelectMilestone(MilestoneModel x)
        {
            if (SaveOnSelect)
            {
                try
                {
                    IsSaving = true;
                    int? milestone = null;
                    if (x != null) milestone = x.Number;
                    var updateReq = this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].UpdateMilestone(milestone);
                    var newIssue = await this.GetApplication().Client.ExecuteAsync(updateReq);
                    Messenger.Publish(new IssueEditMessage { Issue = newIssue.Data });
                }
                catch
                {
                    DisplayAlert("Unable to to save milestone! Please try again.");
                }
                finally
                {
                    IsSaving = false;
                }
            }
            else
            {
                Messenger.Publish(new SelectedMilestoneMessage { Milestone = x });
            }

            Dismiss();
        }

        protected override Task Load()
        {
            return Milestones.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].Milestones.GetAll());
        }
    }
}


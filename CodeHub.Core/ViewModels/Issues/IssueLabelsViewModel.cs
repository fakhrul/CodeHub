using System.Threading.Tasks;
using GitHubSharp.Models;
using System.Collections.Generic;
using CodeHub.Core.Messages;
using System.Linq;
using ReactiveUI;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueLabelsViewModel : LoadableViewModel
    {
        private IEnumerable<LabelModel> _originalLables;

        private bool _isSaving;
        public bool IsSaving
        {
            get { return _isSaving; }
            private set { this.RaiseAndSetIfChanged(ref _isSaving, value); }
        }

        public CollectionViewModel<LabelModel> Labels { get; } = new CollectionViewModel<LabelModel>();

        public CollectionViewModel<LabelModel> SelectedLabels { get; } = new CollectionViewModel<LabelModel>();

        public string Username  { get; private set; }

        public string Repository { get; private set; }

        public long Id { get; private set; }

        public bool SaveOnSelect { get; private set; }

        public IReactiveCommand<Unit> SaveLabelChoices { get; }

        public IssueLabelsViewModel(string username, string repository, long id, bool saveOnSelect, IEnumerable<LabelModel> selectedLabels = null)
        {
            Username = username;
            Repository = repository;
            Id = id;
            SaveOnSelect = saveOnSelect;
            _originalLables = selectedLabels?.ToList();
            SelectedLabels.Items.Reset(_originalLables);

            Title = "Labels";

            SaveLabelChoices = ReactiveCommand.CreateAsyncTask(t => SelectLabels(SelectedLabels));
        }

        private async Task SelectLabels(IEnumerable<LabelModel> x)
        {
            //If nothing has changed, dont do anything...
            if (_originalLables != null && _originalLables.Count() == x.Count() && _originalLables.Intersect(x).Count() == x.Count())
            {
                Dismiss();
                return;
            }
                
            if (SaveOnSelect)
            {
                try
                {
                    IsSaving = true;
                    var labels = x != null ? x.Select(y => y.Name).ToArray() : null;
                    var updateReq = this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].UpdateLabels(labels);
                    var newIssue = await this.GetApplication().Client.ExecuteAsync(updateReq);
                    Messenger.Publish(new IssueEditMessage { Issue = newIssue.Data });
                }
                catch
                {
                    DisplayAlert("Unable to save labels! Please try again.");
                }
                finally
                {
                    IsSaving = false;
                }
            }
            else
            {
                Messenger.Publish(new SelectIssueLabelsMessage { Labels = SelectedLabels.Items.ToArray() });
            }

            Dismiss();
        }

        protected override Task Load()
        {
            return Labels.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].Labels.GetAll());
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public long Id { get; set; }
            public bool SaveOnSelect { get; set; }
        }
    }
}


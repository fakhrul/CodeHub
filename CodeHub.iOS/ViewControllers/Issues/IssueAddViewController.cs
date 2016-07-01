using System;
using CodeHub.Core.ViewModels.Issues;
using UIKit;
using System.Linq;
using CodeHub.Utilities;
using CodeHub.DialogElements;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.ViewControllers.Issues
{
    public class IssueAddViewController : ViewModelDrivenDialogViewController<IssueAddViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 44f;

            var saveButton = new UIBarButtonItem { Image = Images.Buttons.SaveButton };
            NavigationItem.RightBarButtonItem = saveButton;

            var title = new EntryElement("Title", string.Empty, string.Empty);
            var assignedTo = new StringElement("Responsible", "Unassigned", UITableViewCellStyle.Value1);
            var milestone = new StringElement("Milestone", "None", UITableViewCellStyle.Value1);
            var labels = new StringElement("Labels", "None", UITableViewCellStyle.Value1);
            var content = new MultilinedElement("Description");

            Root.Reset(new Section { title, assignedTo, milestone, labels }, new Section { content });

            OnActivation(d =>
            {
                this.WhenAnyValue(x => x.ViewModel.IssueTitle).Subscribe(x => title.Value = x).AddTo(d);
                title.Changed.Subscribe(x => ViewModel.IssueTitle = x).AddTo(d);

                this.WhenAnyValue(x => x.ViewModel.Content).Subscribe(x => content.Details = x).AddTo(d);
                labels.Clicked.InvokeCommand(ViewModel.GoToLabelsCommand).AddTo(d);
                milestone.Clicked.InvokeCommand(ViewModel.GoToMilestonesCommand).AddTo(d);
                assignedTo.Clicked.InvokeCommand(ViewModel.GoToAssigneeCommand).AddTo(d);
                this.WhenAnyValue(x => x.ViewModel.IsSaving).SubscribeStatus("Saving...").AddTo(d);

                this.WhenAnyValue(x => x.ViewModel.AssignedTo).Subscribe(x => {
                    assignedTo.Value = x == null ? "Unassigned" : x.Login;
                }).AddTo(d);

                this.WhenAnyValue(x => x.ViewModel.Milestone).Subscribe(x => {
                    milestone.Value = x == null ? "None" : x.Title;
                }).AddTo(d);

                this.BindCollection(x => x.ViewModel.Labels).Subscribe(_ => {
                    labels.Value = ViewModel.Labels.Items.Count == 0 ? "None" : string.Join(", ", ViewModel.Labels.Items.Select(i => i.Name));
                }).AddTo(d);

                saveButton.GetClickedObservable()
                          .Do(_ => View.EndEditing(true))
                          .InvokeCommand(ViewModel.SaveCommand)
                          .AddTo(d);

                content.Clicked.Subscribe(_ => {
                    var composer = new MarkdownComposerViewController { Title = "Issue Description", Text = content.Details };
                    composer.NewComment(this, (text) => {
                        ViewModel.Content = text;
                        composer.CloseComposer();
                    });
                }).AddTo(d);
            });
        }
    }
}


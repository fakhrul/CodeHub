using System;
using System.Linq;
using UIKit;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Utilities;
using CodeHub.DialogElements;
using ReactiveUI;

namespace CodeHub.ViewControllers.Issues
{
    public class IssueEditViewController : ViewModelDrivenDialogViewController<IssueEditViewModel>
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
            var state = new BooleanElement("Open", true);

            Root.Reset(new Section { title, assignedTo, milestone, labels }, new Section { state }, new Section { content });

            OnActivation(d =>
            {
                ViewModel.WhenAnyValue(x => x.IssueTitle).Subscribe(x => title.Value = x).AddTo(d);
                title.Changed.Subscribe(x => ViewModel.IssueTitle = x).AddTo(d);

                assignedTo.Clicked.InvokeCommand(ViewModel.GoToAssigneeCommand).AddTo(d);
                milestone.Clicked.InvokeCommand(ViewModel.GoToMilestonesCommand).AddTo(d);
                labels.Clicked.InvokeCommand(ViewModel.GoToLabelsCommand).AddTo(d);

                ViewModel.WhenAnyValue(x => x.IsOpen).Subscribe(x => state.Value = x).AddTo(d);
                ViewModel.WhenAnyValue(x => x.IsSaving).SubscribeStatus("Updating...").AddTo(d);

                state.Changed.Subscribe(x => ViewModel.IsOpen = x).AddTo(d);
                ViewModel.WhenAnyValue(x => x.Content).Subscribe(x => content.Details = x).AddTo(d);

                ViewModel.WhenAnyValue(x => x.AssignedTo).Subscribe(x => {
                    assignedTo.Value = x == null ? "Unassigned" : x.Login;
                }).AddTo(d);

                ViewModel.WhenAnyValue(x => x.Milestone).Subscribe(x => {
                    milestone.Value = x == null ? "None" : x.Title;
                }).AddTo(d);

                ViewModel.BindCollection(x => x.Labels, true).Subscribe(x => {
                    labels.Value = ViewModel.Labels.Items.Count == 0 ? "None" : string.Join(", ", ViewModel.Labels.Items.Select(i => i.Name));
                }).AddTo(d);

                saveButton.GetClickedObservable().Subscribe(_ => {
                    View.EndEditing(true);
                    ViewModel.SaveCommand.Execute(null);
                }).AddTo(d);

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


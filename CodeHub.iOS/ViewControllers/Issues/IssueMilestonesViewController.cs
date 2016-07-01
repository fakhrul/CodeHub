using System;
using CodeHub.Core.ViewModels.Issues;
using System.Linq;
using UIKit;
using CodeHub.Utilities;
using CodeHub.DialogElements;
using CodeHub.Views;
using ReactiveUI;

namespace CodeHub.ViewControllers.Issues
{
    public class IssueMilestonesViewController : ViewModelCollectionDrivenDialogViewController<IssueMilestonesViewModel>
    {
        public IssueMilestonesViewController()
        {
            EnableSearch = false;
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Milestone.ToEmptyListImage(), "There are no milestones."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.RowHeight = 80f;
            TableView.SeparatorInset = new UIEdgeInsets(0, 0, 0, 0);

            BindCollection(ViewModel.Milestones, x => {
                var e = new MilestoneElement(x.Number, x.Title, x.OpenIssues, x.ClosedIssues, x.DueOn);
                e.Tapped += () => {
                    if (ViewModel.SelectedMilestone != null && ViewModel.SelectedMilestone.Number == x.Number)
                        ViewModel.SelectedMilestone = null;
                    else
                        ViewModel.SelectedMilestone = x;
                };
                if (ViewModel.SelectedMilestone != null && ViewModel.SelectedMilestone.Number == x.Number)
                    e.Accessory = UITableViewCellAccessory.Checkmark;
                return e;
            });

            this.WhenAnyValue(x => x.ViewModel.SelectedMilestone).Subscribe(x =>
            {
                if (Root.Count == 0)
                    return;
                foreach (var m in Root[0].Elements.Cast<MilestoneElement>())
                    m.Accessory = (x != null && m.Number == x.Number) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
            });

            this.WhenAnyValue(x => x.ViewModel.IsSaving).SubscribeStatus("Saving...");
        }
    }
}


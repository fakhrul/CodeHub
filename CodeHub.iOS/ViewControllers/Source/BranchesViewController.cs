using CodeHub.Core.ViewModels.Source;
using System;
using UIKit;
using CodeHub.Views;
using CodeHub.TableViewSources;

namespace CodeHub.ViewControllers.Source
{
    public class BranchesViewController : BaseTableViewController<BranchesViewModel, RefItemViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new RefTableViewSource(TableView, ViewModel.Items);
            TableView.Source = source;
            TableView.EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.GitBranch.ToEmptyListImage(), "There are no branches."));
        }
    }
}


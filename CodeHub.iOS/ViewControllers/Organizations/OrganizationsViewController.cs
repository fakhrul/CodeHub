using CodeHub.Core.ViewModels.Organizations;
using System;
using UIKit;
using CodeHub.Views;
using CodeHub.TableViewSources;
using ReactiveUI;

namespace CodeHub.ViewControllers.Organizations
{
    public class OrganizationsViewController : BaseTableViewController<OrganizationsViewModel, OrganizationItemViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new OrganizationTableViewSource(TableView, ViewModel.Items);
            TableView.Source = source;
            source.RequestMore.InvokeCommand(ViewModel.LoadMoreCommand);
            TableView.EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Organization.ToEmptyListImage(), "There are no organizations."));
        }
    }
}


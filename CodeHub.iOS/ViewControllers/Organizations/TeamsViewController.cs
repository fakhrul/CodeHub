using System;
using CodeHub.Core.ViewModels.Organizations;
using CodeHub.TableViewSources;
using CodeHub.Views;
using ReactiveUI;
using UIKit;

namespace CodeHub.ViewControllers.Organizations
{
    public class TeamsViewController : BaseTableViewController<TeamsViewModel, TeamItemViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new TeamTableViewSource(TableView, ViewModel.Items);
            TableView.Source = source;
            source.RequestMore.InvokeCommand(ViewModel.LoadMoreCommand);
            TableView.EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Organization.ToEmptyListImage(), "There are no teams."));
        }
    }
}
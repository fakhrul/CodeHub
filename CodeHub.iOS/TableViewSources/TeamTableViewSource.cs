using CodeHub.Core.ViewModels.Organizations;
using CodeHub.TableViewCells;
using ReactiveUI;
using UIKit;

namespace CodeHub.TableViewSources
{
    public class TeamTableViewSource : BaseTableViewSource<TeamItemViewModel>
    {
        public TeamTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<TeamItemViewModel> collection)
            : base(tableView, collection, TeamTableCellView.Key, 44)
        {
            tableView.RegisterClassForCellReuse(typeof(TeamTableCellView), TeamTableCellView.Key);
        }
    }
}
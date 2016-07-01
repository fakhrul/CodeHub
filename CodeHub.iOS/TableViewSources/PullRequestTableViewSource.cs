using ReactiveUI;
using CodeHub.Core.ViewModels.PullRequests;
using UIKit;
using CodeHub.TableViewCells;

namespace CodeHub.TableViewSources
{
    public class PullRequestTableViewSource : BaseTableViewSource<PullRequestItemViewModel>
    {
        public PullRequestTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<PullRequestItemViewModel> collection)
            : base(tableView, collection, PullRequestCellView.Key, UITableView.AutomaticDimension, 60.0f)
        {
            tableView.RegisterNibForCellReuse(PullRequestCellView.Nib, PullRequestCellView.Key);
        }
    }
}
using ReactiveUI;
using CodeHub.Core.ViewModels.Gists;
using UIKit;
using CodeHub.TableViewCells;

namespace CodeHub.TableViewSources
{
    public class GistTableViewSource : BaseTableViewSource<GistItemViewModel>
    {
        public GistTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<GistItemViewModel> collection)
            : base(tableView, collection, GistCellView.Key, UITableView.AutomaticDimension, 60f)
        {
            tableView.RegisterNibForCellReuse(GistCellView.Nib, GistCellView.Key);
        }
    }
}
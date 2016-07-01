using CodeHub.Core.ViewModels.Repositories;
using CodeHub.TableViewCells;
using ReactiveUI;
using UIKit;

namespace CodeHub.TableViewSources
{
    public class RepositoryTableViewSource : BaseTableViewSource<RepositoryItemViewModel>
    {
        public RepositoryTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<RepositoryItemViewModel> collection)
            : base(tableView, collection, RepositoryCellView.Key, UITableView.AutomaticDimension, 80)
        {
            tableView.RegisterNibForCellReuse(RepositoryCellView.Nib, RepositoryCellView.Key);
        }
    }
}
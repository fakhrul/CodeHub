using CodeHub.Core.ViewModels.Source;
using CodeHub.TableViewCells;
using ReactiveUI;
using UIKit;

namespace CodeHub.TableViewSources
{
    public class RefTableViewSource : BaseTableViewSource<RefItemViewModel>
    {
        public RefTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<RefItemViewModel> collection)
            : base(tableView, collection, RefTableViewCell.Key, 44)
        {
            tableView.RegisterClassForCellReuse(typeof(RefTableViewCell), RefTableViewCell.Key);
        }
    }
}
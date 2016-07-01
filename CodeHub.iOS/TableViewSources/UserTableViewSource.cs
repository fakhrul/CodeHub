using CodeHub.Core.ViewModels.Users;
using CodeHub.TableViewCells;
using ReactiveUI;
using UIKit;

namespace CodeHub.TableViewSources
{
    public class UserTableViewSource : BaseTableViewSource<UserItemViewModel>
    {
        public UserTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<UserItemViewModel> collection)
            : base(tableView, collection, UserTableViewCell.Key, 44)
        {
            tableView.RegisterClassForCellReuse(typeof(UserTableViewCell), UserTableViewCell.Key);
        }
    }
}
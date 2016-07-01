using CodeHub.Core.ViewModels.Organizations;
using CodeHub.TableViewCells;
using ReactiveUI;
using UIKit;

namespace CodeHub.TableViewSources
{
    public class OrganizationTableViewSource : BaseTableViewSource<OrganizationItemViewModel>
    {
        public OrganizationTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<OrganizationItemViewModel> collection)
            : base(tableView, collection, OrganizationTableViewCell.Key, 44)
        {
            tableView.RegisterClassForCellReuse(typeof(OrganizationTableViewCell), OrganizationTableViewCell.Key);
        }
    }
}
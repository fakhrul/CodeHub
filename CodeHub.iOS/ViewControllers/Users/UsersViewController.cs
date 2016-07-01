using System;
using CodeHub.Core.ViewModels.Users;
using UIKit;
using CodeHub.Views;
using CodeHub.TableViewSources;
using ReactiveUI;

namespace CodeHub.ViewControllers.Users
{
    public abstract class UsersViewController<TViewModel> : BaseTableViewController<TViewModel, UserItemViewModel>
        where TViewModel : UsersViewModel
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var emptyMessage = ViewModel.EmptyMessage ?? "There are no users.";
            TableView.EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Person.ToEmptyListImage(), emptyMessage));

            var source = new UserTableViewSource(TableView, ViewModel.Items);
            source.RequestMore.InvokeCommand(ViewModel.LoadMoreCommand);
            TableView.Source = source;
        }
    }
}


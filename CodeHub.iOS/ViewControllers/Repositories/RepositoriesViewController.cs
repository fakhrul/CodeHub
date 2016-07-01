using CodeHub.Core.ViewModels.Repositories;
using UIKit;
using System;
using CodeHub.Views;
using CodeHub.TableViewSources;
using ReactiveUI;

namespace CodeHub.ViewControllers.Repositories
{
    public abstract class RepositoriesViewController<TViewModel> : BaseTableViewController<TViewModel, RepositoryItemViewModel>
        where TViewModel : RepositoriesViewModel
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new RepositoryTableViewSource(TableView, ViewModel.Items);
            TableView.Source = source;
            source.RequestMore.InvokeCommand(ViewModel.LoadMoreCommand);
            TableView.EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Repo.ToEmptyListImage(), "There are no repositories."));
        }
    }
}
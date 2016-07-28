using CodeHub.Core.ViewModels.PullRequests;
using UIKit;
using System;
using ReactiveUI;
using CodeHub.TableViewSources;
using System.Reactive.Linq;
using CodeHub.Views;

namespace CodeHub.ViewControllers.PullRequests
{
    public class PullRequestListViewController : BaseTableViewController<PullRequestListViewModel, PullRequestItemViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var viewModel = ViewModel;
            var source = new PullRequestTableViewSource(TableView, ViewModel.Items);
            TableView.Source = source;
            source.RequestMore.Subscribe(_ => viewModel?.LoadMoreCommand.ExecuteIfCan());
            TableView.EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.GitPullRequest.ToEmptyListImage(), "There are no pull requests."));
        }
    }
}


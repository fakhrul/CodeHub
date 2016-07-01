using CodeHub.ViewControllers;
using CodeHub.Core.ViewModels.PullRequests;
using UIKit;
using System;
using ReactiveUI;
using CodeHub.TableViewSources;

namespace CodeHub.Views.PullRequests
{
    public class PullRequestsViewController : BaseTableViewController<PullRequestsViewModel, PullRequestItemViewModel>
    {
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] { "Open", "Closed" });

        public PullRequestsViewController()
        {
            NavigationItem.TitleView = _viewSegment;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new PullRequestTableViewSource(TableView, ViewModel.Items);
            TableView.Source = source;
            source.RequestMore.Subscribe(_ => ViewModel?.LoadMoreCommand.ExecuteIfCan());
            TableView.EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.GitPullRequest.ToEmptyListImage(), "There are no pull requests."));

            OnActivation(d =>
            {
                this.WhenAnyValue(x => x.ViewModel.SelectedFilter)
                    .Subscribe(x => _viewSegment.SelectedSegment = x)
                    .AddTo(d);

                _viewSegment
                    .GetChangedObservable()
                    .Subscribe(x => ViewModel.SelectedFilter = x)
                    .AddTo(d);
            });
        }
    }
}

